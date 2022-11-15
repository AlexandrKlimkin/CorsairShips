using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClansClientLib;
using ClansServerLib.Mongo;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using PestelLib.ServerCommon.Db;

namespace ClansServerLib
{
    class AggregationsByPeriod
    {
        public TimeSpan Period;
        public IMongoCollection<MongoClanTreasuryAuditRecord> Collection;
        public MemoryCache Cache;
    }

    class TreasuryAuditDb
    {
        public TreasuryAuditDb(MongoDb db)
        {
            _aggregations = new List<AggregationsByPeriod>();
            _config = ClansConfigCache.Get();
            {
                var item = new AggregationsByPeriod()
                {
                    Period = _config.TreasuryAuditHistoryTTL,
                    Collection = db.GetCollection<MongoClanTreasuryAuditRecord>("TreasuryAudit"),
                    Cache = new MemoryCache(new MemoryCacheOptions()
                    {
                        SizeLimit = _config.DonationsAuditMainCacheSize
                    })
                };

                item.Collection.Indexes.CreateOne(Builders<MongoClanTreasuryAuditRecord>.IndexKeys.Ascending(_ => _.Date));
                item.Collection.Indexes.CreateOne(Builders<MongoClanTreasuryAuditRecord>.IndexKeys.Ascending(_ => _.ClanId));
                item.Collection.Indexes.CreateOne(Builders<MongoClanTreasuryAuditRecord>.IndexKeys.Ascending(_ => _.PlayerId));

                _aggregations.Add(item);
            }
            foreach (var timeSpan in _config.DonationsAggregateByPeriod)
            {
                var secs = (int)timeSpan.TotalSeconds;
                var tableName = $"TreasuryAuditPeriod{secs}";
                var item = new AggregationsByPeriod()
                {
                    Period = timeSpan,
                    Collection = db.GetCollection<MongoClanTreasuryAuditRecord>(tableName),
                    Cache = new MemoryCache(new MemoryCacheOptions()
                    {
                        SizeLimit = _config.DonationsAuditSecondaryCacheSize,
                    })
                };

                item.Collection.Indexes.CreateOne(Builders<MongoClanTreasuryAuditRecord>.IndexKeys.Ascending(_ => _.Date));
                item.Collection.Indexes.CreateOne(Builders<MongoClanTreasuryAuditRecord>.IndexKeys.Ascending(_ => _.ClanId));

                _aggregations.Add(item);
            }
        }

        public async Task ClanDeleted(Guid clanId)
        {
            var filter = Builders<MongoClanTreasuryAuditRecord>.Filter.Eq(_ => _.ClanId, clanId);
            foreach (var aggregationsByPeriod in _aggregations)
            {
                var r = await aggregationsByPeriod.Collection.DeleteManyAsync(filter);
                if(r.DeletedCount > 0)
                {
                    var cacheKey = GetPeriodicDonationsKey(clanId);
                    aggregationsByPeriod.Cache.Remove(cacheKey);
                }
            }
        }

        public async Task UserHasLeftTheClan(Guid userId, Guid clanId)
        {
            var filter = Builders<MongoClanTreasuryAuditRecord>.Filter.And(
                Builders<MongoClanTreasuryAuditRecord>.Filter.Eq(_ => _.PlayerId, userId),
                Builders<MongoClanTreasuryAuditRecord>.Filter.Eq(_ => _.ClanId, clanId)
                );
            foreach (var aggregationsByPeriod in _aggregations)
            {
                var records = await (await aggregationsByPeriod.Collection.FindAsync(filter)).ToListAsync();
                if (records.Count == 0)
                    continue;

                await aggregationsByPeriod.Collection.DeleteManyAsync(filter);
                foreach (var clanAffected in records.Select(_ => _.ClanId).Distinct())
                {
                    var cacheKey = GetPeriodicDonationsKey(clanAffected);
                    aggregationsByPeriod.Cache.Remove(cacheKey);
                }
            }
        }

        // юзер удален через админку, необходимо удалить записи связанные с ним
        public async Task UserDeleted(Guid userId)
        {
            var filter = Builders<MongoClanTreasuryAuditRecord>.Filter.Eq(_ => _.PlayerId, userId);
            foreach (var aggregationsByPeriod in _aggregations)
            {
                var records = await (await aggregationsByPeriod.Collection.FindAsync(filter)).ToListAsync();
                if(records.Count == 0)
                    continue;

                await aggregationsByPeriod.Collection.DeleteManyAsync(filter);
                foreach (var clanAffected in records.Select(_ => _.ClanId).Distinct())
                {
                    var cacheKey = GetPeriodicDonationsKey(clanAffected);
                    aggregationsByPeriod.Cache.Remove(cacheKey);
                }
            }
        }

        public async Task Audit(DateTime time, Guid clanId, Guid playerId, int amount, int reason, int? consumableId = null)
        {
            var delta = (DateTime.UtcNow - time).TotalSeconds;

            foreach (var aggregationsByPeriod in _aggregations)
            {
                if(aggregationsByPeriod.Period.TotalSeconds < delta)
                    continue;

                await Audit(aggregationsByPeriod, time, clanId, playerId, amount, reason, consumableId);

                var cacheKey = GetPeriodicDonationsKey(clanId);
                aggregationsByPeriod.Cache.Remove(cacheKey);
            }
        }

        class MongoClanTreasuryAuditRecordClanId
        {
            public Guid ClanId;
        }

        /// <summary>
        /// Удаляет устаревшие записи
        /// </summary>
        /// <returns></returns>
        public async Task<long> Cleanup()
        {
            var count = 0l;
            foreach (var aggregationsByPeriod in _aggregations)
            {
                var threshold = DateTime.UtcNow - aggregationsByPeriod.Period;
                var opts = new FindOptions<MongoClanTreasuryAuditRecord, MongoClanTreasuryAuditRecordClanId>()
                {
                    Projection = Builders<MongoClanTreasuryAuditRecord>.Projection.Expression(_ => new MongoClanTreasuryAuditRecordClanId()
                    {
                        ClanId = _.ClanId
                    })
                };
                var filter = Builders<MongoClanTreasuryAuditRecord>.Filter.Lt(_ => _.Date, threshold);

                var affectedClans = await (await aggregationsByPeriod.Collection.FindAsync(filter, opts)).ToListAsync();
                var r = await aggregationsByPeriod.Collection.DeleteManyAsync(filter);

                if (affectedClans.Count > 0)
                {
                    lock (aggregationsByPeriod)
                    {
                        foreach (var clan in affectedClans)
                        {
                            var cacheKey = GetPeriodicDonationsKey(clan.ClanId);
                            aggregationsByPeriod.Cache.Remove(cacheKey);
                        }
                    }
                }

                count += r.DeletedCount;
            }

            return count;
        }

        public async Task<PeriodicClanDonationByPlayer> GetClanDonationsByPeriod(TimeSpan period, Guid clanId)
        {
            var aggregation = ChooseClosestAggregationToPeriod(period);
            var cacheKey = GetPeriodicDonationsKey(clanId);
            if(aggregation.Cache.TryGetValue(cacheKey, out var cachedValue))
                return cachedValue as PeriodicClanDonationByPlayer;

            var timeThreshold = DateTime.UtcNow - period;
            var filter = Builders<MongoClanTreasuryAuditRecord>.Filter.And(
                Builders<MongoClanTreasuryAuditRecord>.Filter.Eq(_ => _.ClanId, clanId),
                Builders<MongoClanTreasuryAuditRecord>.Filter.Gte(_ => _.Date, timeThreshold)
                );
            var items = await (await aggregation.Collection.FindAsync(filter)).ToListAsync();
            var itemsByPlayer = items.GroupBy(_ => _.PlayerId).ToDictionary(_ => _.Key, _ => _.ToArray());
            var result = new PeriodicClanDonationByPlayer();
            result.Period = aggregation.Period;
            result.DonationsByPlayer = new ClanPlayerTotalDonations[itemsByPlayer.Count];
            var idx = 0;
            var cacheSize = 16; // тут будем считать честный размер, учитывая данные по ссылкам
            foreach (var kv in itemsByPlayer)
            {
                var totalTreasury = 0;
                var consumables = new Dictionary<int, ClanPlayerConsumableDonation>();
                foreach (var auditRecord in kv.Value)
                {
                    if (auditRecord.Amount < 1)
                        continue;
                        
                    if (!auditRecord.IsConsumable)
                        totalTreasury += auditRecord.Amount;
                    else
                    {
                        if (!consumables.ContainsKey(auditRecord.ConsumableId))
                            consumables[auditRecord.ConsumableId] = new ClanPlayerConsumableDonation()
                                {CurrencyType = auditRecord.ConsumableId, TotalDonation = auditRecord.Amount};
                        else
                            consumables[auditRecord.ConsumableId].TotalDonation += auditRecord.Amount;
                    }

                }
                cacheSize += 20 + 8 * consumables.Count;
                result.DonationsByPlayer[idx++] = new ClanPlayerTotalDonations()
                {
                    PlayerId = kv.Key,
                    TotalDonation = totalTreasury,
                    CurrencyDonations = consumables.Values.ToArray()
                };
            }

            aggregation.Cache.Set(cacheKey, result, new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = _config.DonationsAuditCacheItemTTL,
                Size = cacheSize
            });

            return result;
        }

        private AggregationsByPeriod ChooseClosestAggregationToPeriod(TimeSpan period)
        {
            var sorted = _aggregations.Select(_ => _.Period).OrderBy(_ => _).ToList();
            var idx = sorted.BinarySearch(period);
            if (idx < 0) idx *= -1;
            if (idx >= _aggregations.Count)
                return _aggregations.Last();
            return _aggregations[idx];
        }

        private async Task Audit(AggregationsByPeriod collection, DateTime time, Guid clanId, Guid playerId, int amount, int reason, int? consumableId)
        {
            var auditRecord = new MongoClanTreasuryAuditRecord()
            {
                ClanId = clanId,
                PlayerId = playerId,
                Date = time,
                Amount = amount,
                Reason = reason,
                IsConsumable = consumableId.HasValue,
                ConsumableId = consumableId ?? 0
            };

            lock (collection)
            {
                var cacheKey = GetPeriodicDonationsKey(clanId);
                if (collection.Cache.TryGetValue(cacheKey, out var cachedValue))
                {
                    var donationStats = cachedValue as PeriodicClanDonationByPlayer;
                    for (int i = 0; i < donationStats.DonationsByPlayer.Length; i++)
                    {
                        if (donationStats.DonationsByPlayer[i].PlayerId == playerId)
                            donationStats.DonationsByPlayer[i].TotalDonation += amount;
                    }
                }
            }

            await collection.Collection.InsertOneAsync(auditRecord);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetPeriodicDonationsKey(Guid clanId)
        {
            return $"pd:{clanId}";
        }

        private List<AggregationsByPeriod> _aggregations;
        private ClansConfig _config;
    }
}
