using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClansClientLib;
using log4net;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PestelLib.ServerCommon.Db;
using PestelLib.ServerCommon.Db.Mongo;
using PestelLib.ServerCommon.Extensions;
using PestelLib.ServerCommon.Workers;
using ServerShared;
using UnityDI;

namespace ClansServerLib.Mongo
{
    [BsonIgnoreExtraElements]
    class MongoClanRequirement
    {
        [BsonId]
        public ObjectId Id;
        public Guid ClanId;
        public string Name;
        public int Value;
    }

    [BsonIgnoreExtraElements]
    class MongoPlayerToClanMapEntry
    {
        [BsonId]
        public Guid PlayerId;
        public Guid ClanId;
    }

    [BsonIgnoreExtraElements]
    class MongoClanTreasuryAuditRecord : ClanTreasuryAuditRecord
    {
        [BsonId] public ObjectId Id;
    }

    [BsonIgnoreExtraElements]
    class MongoClanJoinRequest
    {
        [BsonId]
        public Guid Id;

        public DateTime Time;
        public DateTime Expiry;
        public Guid ClanId;
        public Guid PlayerId;
        public JoinRequestState State;
    }

    static class MongoClanJoinRequestExtensions
    {
        public static ClanRequestRecord From(this MongoClanJoinRequest item)
        {
            return new ClanRequestRecord()
            {
                Id = item.Id,
                ClanId = item.ClanId,
                PlayerId = item.PlayerId,
                Time = item.Time,
                Status = item.State,
                Expiry = item.Expiry
            };
        }
    }

    class MongoClanDb : IClanDB, IClansDbPrivate, IDisposable
    {

        public MongoClanDb()
        {
            MongoClanDbMongoTypesRegistrar.Init();

            ContainerHolder.Container.BuildUp(this);

            _pestelStatsClient = PestelStatsClient.Create("Clans");
            _config = ClansConfigCache.Get();
            var url = new MongoUrl(_config.DbConnectionString);
            var server = url.GetServer();
            var db = server.GetDatabase(url.DatabaseName);
            _clansCollection = db.GetCollection<ClanRecord>("Clans");
            if (!_config.AllowDuplicatesForClanName)
                _clanNameChecker = MongoUniqueName.Create(db, "NameToClanId", !_config.NameCaseSensitive);
            if (!_config.AllowDuplicatesForClanTag)
                _clanTagChecker = MongoUniqueName.Create(db, "TagToClanId", !_config.TagCaseSensitive);

            _playerToClanMap = db.GetCollection<MongoPlayerToClanMapEntry>("PlayerToClanMap");
            _clanJoinRequestsCollection = db.GetCollection<MongoClanJoinRequest>("JoinRequests");
            _clanConsumablesCollection = db.GetCollection<ClanConsumables>("Consumables");
            _treasuryAuditDb = new TreasuryAuditDb(db);

            _clansCollection.Indexes.CreateOne(Builders<ClanRecord>.IndexKeys.Ascending(_ => _.Desc.Name));
            _clansCollection.Indexes.CreateOne(Builders<ClanRecord>.IndexKeys.Ascending(_ => _.Desc.Tag));
            _clansCollection.Indexes.CreateOne(Builders<ClanRecord>.IndexKeys.Ascending(_ => _.Rating));
            _clansCollection.Indexes.CreateOne(Builders<ClanRecord>.IndexKeys.Ascending(_ => _.Level));

            _clanRequirementsCollection = db.GetCollection<MongoClanRequirement>("JoinRequirements");
            _clanRequirementsCollection.Indexes.CreateOne(Builders<MongoClanRequirement>.IndexKeys.Combine(
                    Builders<MongoClanRequirement>.IndexKeys.Ascending(_ => _.Name),
                    Builders<MongoClanRequirement>.IndexKeys.Ascending(_ => _.ClanId)),
                new CreateIndexOptions() {Unique = true});

            _clanJoinRequestsCollection.Indexes.CreateOne(
                Builders<MongoClanJoinRequest>.IndexKeys.Ascending(_ => _.PlayerId));
            _clanJoinRequestsCollection.Indexes.CreateOne(
                Builders<MongoClanJoinRequest>.IndexKeys.Ascending(_ => _.ClanId));
            _clanJoinRequestsCollection.Indexes.CreateOne(
                Builders<MongoClanJoinRequest>.IndexKeys.Ascending(_ => _.Expiry));

            _clanConsumablesCollection.Indexes.CreateOne(new CreateIndexModel<ClanConsumables>(
                Builders<ClanConsumables>.IndexKeys.Combine(
                    Builders<ClanConsumables>.IndexKeys.Ascending(_ => _.ClanId),
                    Builders<ClanConsumables>.IndexKeys.Ascending(_ => _.ConsumableId)
                ), new CreateIndexOptions() {Unique = true}));

            _boosterCleaner = new MongoClanBoosterCleaner(db, _clansCollection);
            _periodicWorker = new PeriodicWorker(_config.BackgroundWorkerPeriod, _config.BackgroundWorkerPeriod);
            _periodicWorker.RegisterWork(async () => await _treasuryAuditDb.Cleanup());
            _periodicWorker.RegisterWork(async () => await _boosterCleaner.Run());

            _topClansCache = new MemoryCache(new MemoryCacheOptions());
            _topClansSemaphore = new SemaphoreSlim(1);
        }

        public async Task<ClanRecord> GetClanByPlayer(Guid playerId)
        {
            var clanId = await GetPlayerClanId(playerId);
            if (clanId == Guid.Empty)
                return null;

            return await GetClan(clanId);
        }

        private async Task<IEnumerable<ClanRequirement>> GetRequirements(Guid clanId)
        {
            var filter = Builders<MongoClanRequirement>.Filter.Eq(_ => _.ClanId, clanId);
            return (await _clanRequirementsCollection.FindAsync(filter)).ToEnumerable().Select(_ => new ClanRequirement()
            {
                Name = _.Name,
                Value = _.Value
            });
        }

        public async Task<ClanRecord> GetClan(Guid id)
        {
            ClanRecord result = _clanRecordCache.Get(id);
            if (result == null)
            {
                var filter = Builders<ClanRecord>.Filter.Eq(_ => _.Id, id);
                result = await (await _clansCollection.FindAsync(filter)).SingleOrDefaultAsync();
                if (result == null)
                    return null;
            }

            _boosterCleaner.CheckExpiredBoosts(result);
            SetClanTopPlace(result).ReportOnFail();
            return result;
        }

        public async Task<GetClanConsumablesResult> GetClanConsumables(Guid clanId)
        {
            var result = new GetClanConsumablesResult();
            var clan = await GetClan(clanId);
            if (clan == null)
            {
                result.Code = GetClanConsumablesCode.NoClan;
                return result;
            }

            var filter = Builders<ClanConsumables>.Filter.Eq(_ => _.ClanId, clanId);
            var consumables = await (await _clanConsumablesCollection.FindAsync(filter)).ToListAsync();
            result.Code = GetClanConsumablesCode.Success;
            result.Consumables = consumables.ToArray();

            return result;
        }

        public async Task<GiveConsumableResult> GiveConsumable(Guid clanId, Guid callerId, int consumableId, int amount, int reason)
        {
            var result = new GiveConsumableResult();
            if (amount < 1)
            {
                result.Code = AddConsumableCode.InvalidAmount;
                return result;
            }

            var clan = await GetClan(clanId);
            if (clan == null)
            {
                result.Code = AddConsumableCode.NoClan;
                return result;
            }

            if (clan.Members.All(_ => _.Id != callerId))
            {
                result.Code = AddConsumableCode.NotClanMember;
                return result;
            }

            var max = int.MaxValue - amount;
            var filter = Builders<ClanConsumables>.Filter.And(
                Builders<ClanConsumables>.Filter.Eq(_ => _.ClanId, clanId),
                Builders<ClanConsumables>.Filter.Lte(_ => _.Balance, max),
                Builders<ClanConsumables>.Filter.Eq(_ => _.ConsumableId, consumableId)
            );
            var update = Builders<ClanConsumables>.Update.Inc(_ => _.Balance, amount);
            var opt = new FindOneAndUpdateOptions<ClanConsumables>() {ReturnDocument = ReturnDocument.After, IsUpsert = true};
            var modified = await _clanConsumablesCollection.FindOneAndUpdateAsync(filter, update, opt);
            if (modified != null)
            {
                result.AmountAfter = modified.Balance;
            }
            else
            {
                filter = Builders<ClanConsumables>.Filter.And(
                    Builders<ClanConsumables>.Filter.Eq(_ => _.ClanId, clanId),
                    Builders<ClanConsumables>.Filter.Eq(_ => _.ConsumableId, consumableId));
                var count = await _clanConsumablesCollection.CountDocumentsAsync(filter);
                if (count == 0)
                {
                    result.Code = AddConsumableCode.InvalidAmount;
                    return result;
                }
                result.AmountAfter = amount;
            }
            _notifyPlayers.AskClanToUpdateConsumables(clanId);
            await _treasuryAuditDb.Audit(DateTime.UtcNow, clanId, callerId, amount, reason, consumableId);

            return result;
        }

        public async Task<TakeConsumableResult> TakeConsumable(Guid clanId, Guid callerId, int consumableId, int amount, int reason)
        {
            var result = new TakeConsumableResult();
            if (amount < 1)
            {
                result.Code = TakeConsumableCode.InvalidAmount;
                return result;
            }

            var clan = await GetClan(clanId);
            if (clan == null)
            {
                result.Code = TakeConsumableCode.NoClan;
                return result;
            }

            if (clan.Members.All(_ => _.Id != callerId))
            {
                result.Code = TakeConsumableCode.NotClanMember;
                return result;
            }

            if (!_config.AllClanMembersCanTakeConsumables && clan.Owner != callerId)
            {
                result.Code = TakeConsumableCode.NotOwner;
                return result;
            }

            var filter = Builders<ClanConsumables>.Filter.And(
                Builders<ClanConsumables>.Filter.Eq(_ => _.ClanId, clanId),
                Builders<ClanConsumables>.Filter.Eq(_ => _.ConsumableId, consumableId)
            );
            var update = Builders<ClanConsumables>.Update.Inc(_ => _.Balance, -amount);
            var opt = new FindOneAndUpdateOptions<ClanConsumables>() { ReturnDocument = ReturnDocument.After };
            var modified = await _clanConsumablesCollection.FindOneAndUpdateAsync(filter, update, opt);
            if (modified != null)
            {
                result.Code = TakeConsumableCode.NoMoney;
                return result;
            }
            else
            {
                result.AmountAfter = amount;
            }
            _notifyPlayers.AskClanToUpdateConsumables(clanId);
            await _treasuryAuditDb.Audit(DateTime.UtcNow, clanId, callerId, -amount, reason, consumableId);

            return result;
        }

        private async Task SetClanTopPlace(ClanRecord clan)
        {
            var list = await GetTopClans();
            for (var i = 0; i < list.Count; ++i)
            {
                if (list[i].Id == clan.Id)
                    clan.TopPlace = i + 1;
            }
        }

        private async Task SetClanTopPlace(IEnumerable<ClanRecord> clans)
        {
            var list = await GetTopClans();
            var placeMap = new Dictionary<Guid, int>();
            for (var i = 0; i < list.Count; ++i)
            {
                placeMap[list[i].Id] = i + 1;
            }

            foreach (var record in clans)
            {
                placeMap.TryGetValue(record.Id, out var place);
                record.TopPlace = place;
            }
        }

        public async Task<List<ClanRecord>> GetTopClans()
        {
            if (_topClansCache.TryGetValue(topClansCacheKey, out var list))
                return (List<ClanRecord>) list;

            await _topClansSemaphore.WaitAsync();
            List<ClanRecord> result = new List<ClanRecord>();
            try
            {
                var sw = Stopwatch.StartNew();
                if (_topClansCache.TryGetValue(topClansCacheKey, out var listAgain))
                    return (List<ClanRecord>)listAgain;

                var opt = new FindOptions<ClanRecord>();
                opt.Limit = _config.TopClansAmount;
                opt.Sort = Builders<ClanRecord>.Sort.Descending(_ => _.Rating);
                result = await (await _clansCollection.FindAsync(Builders<ClanRecord>.Filter.Empty, opt)).ToListAsync();

                var cacheTime = _config.TopClanCacheTTL;
                if (cacheTime == default)
                    cacheTime = sw.Elapsed * 20;
                _topClansCache.Set(topClansCacheKey, result, cacheTime);
                _pestelStatsClient.SendStat("gettoptime", sw.Elapsed.Milliseconds);
            }
            finally
            {
                _topClansSemaphore.Release();
            }

            await SetClanTopPlace(result);
            return result;
        }

        private FilterDefinition<ClanRecord> GetRequirementsFilter(params ClanRequirement[] requirements)
        {
            if (requirements.Length < 1)
                return Builders<ClanRecord>.Filter.Empty;

            // каждое требования в клане должно быть ниже или равно соотвествующему значению у игрока
            List<FilterDefinition<ClanRecord>> playerParamsFilters = new List<FilterDefinition<ClanRecord>>();
            for (int i = 0; i < requirements.Length; ++i)
            {
                playerParamsFilters.Add(Builders<ClanRecord>.Filter.ElemMatch(_ => _.Desc.Requirements, r =>
                    r.Name == requirements[i].Name &&
                    r.Value == requirements[i].Value));
            }
            // количество требования должно быть такимже как количество параметров у игрока
            playerParamsFilters.Add(Builders<ClanRecord>.Filter.Size(_ => _.Desc.Requirements, requirements.Length));

            // клан либо не содержит требований, либо требования ниже чем парамтеру игрока
            FilterDefinition<ClanRecord> requirementsFilter =
                Builders<ClanRecord>.Filter.Or(
                    Builders<ClanRecord>.Filter.Size(_ => _.Desc.Requirements, 0),
                    Builders<ClanRecord>.Filter.And(playerParamsFilters)
                );
            return requirementsFilter;
        }

        public async Task<List<ClanRecord>> FindByNameOrTag(string searchText)
        {
            var filter = Builders<ClanRecord>.Filter.And(
                Builders<ClanRecord>.Filter.Or(
                    Builders<ClanRecord>.Filter.Regex(_ => _.Desc.Name, new BsonRegularExpression(searchText, "i")), 
                    Builders<ClanRecord>.Filter.Regex(_ => _.Desc.Tag, new BsonRegularExpression(searchText, "i"))
                    )
            );
            var opts = new FindOptions<ClanRecord>() {Limit = _config.SearchByNameTagLimit};
            var r = await (await _clansCollection.FindAsync(filter, opts)).ToListAsync();

            await SetClanTopPlace(r);
            return r;
        }

        public async Task<List<ClanRecord>> FindByParamsExact(int level, params ClanRequirement[] requirements)
        {
            FilterDefinition<ClanRecord> requirementsFilter = null;
            if (requirements.Length > 0)
            {
                // каждое требования в клане должно быть равно соотвествующему значению у игрока
                List<FilterDefinition<ClanRecord>> playerParamsFilters = new List<FilterDefinition<ClanRecord>>();
                for (int i = 0; i < requirements.Length; ++i)
                {
                    var req = requirements[i];
                    playerParamsFilters.Add(Builders<ClanRecord>.Filter.ElemMatch(_ => _.Desc.Requirements, r =>
                        r.Name == req.Name &&
                        r.Value == req.Value));
                }

                // количество требования должно быть такимже как количество параметров у игрока
                playerParamsFilters.Add(Builders<ClanRecord>.Filter.Size(_ => _.Desc.Requirements,
                    requirements.Length));

                requirementsFilter = Builders<ClanRecord>.Filter.And(playerParamsFilters);
            }

            FilterDefinition<ClanRecord> filter;
            if (requirementsFilter != null)
                filter = Builders<ClanRecord>.Filter.And(
                    requirementsFilter,
                    Builders<ClanRecord>.Filter.Eq(_ => _.Level, level)
                );
            else
                filter = Builders<ClanRecord>.Filter.Eq(_ => _.Level, level);

            var opts = new FindOptions<ClanRecord>() { Limit = _config.SearchByExactParamsLimit };
            var r = await (await _clansCollection.FindAsync(filter, opts)).ToListAsync();
            await SetClanTopPlace(r);
            return r;
        }

        public async Task<List<ClanRecord>> FindByMaxParams(int maxLevel, ClanRequirement[] maxRequirements, bool includeClansWithoutRequirements, bool joinOpenOnly)
        {
            FilterDefinition<ClanRecord> requirementsFilter = null;
            if (maxRequirements.Length > 0)
            {
                // каждое требования в клане должно быть ниже или равно соотвествующему значению у игрока
                List<FilterDefinition<ClanRecord>> playerParamsFilters = new List<FilterDefinition<ClanRecord>>();
                for (int i = 0; i < maxRequirements.Length; ++i)
                {
                    var req = maxRequirements[i];
                    playerParamsFilters.Add(Builders<ClanRecord>.Filter.ElemMatch(_ => _.Desc.Requirements, _ =>
                        _.Name == req.Name &&
                        _.Value <= req.Value));
                }

                // количество требования должно быть такимже как количество параметров у игрока
                playerParamsFilters.Add(Builders<ClanRecord>.Filter.Size(_ => _.Desc.Requirements,
                    maxRequirements.Length));

                if (includeClansWithoutRequirements)
                    // клан либо не содержит требований, либо требования ниже чем параметры игрока
                    requirementsFilter =
                        Builders<ClanRecord>.Filter.Or(
                            Builders<ClanRecord>.Filter.Eq(_ => _.Desc.Requirements, null),
                            Builders<ClanRecord>.Filter.Exists(_ => _.Desc.Requirements, false),
                            Builders<ClanRecord>.Filter.Size(_ => _.Desc.Requirements, 0),
                            Builders<ClanRecord>.Filter.And(playerParamsFilters)
                        );
                else
                    requirementsFilter = Builders<ClanRecord>.Filter.And(playerParamsFilters);
            }

            FilterDefinition<ClanRecord> filter;
            if (requirementsFilter != null)
                filter = Builders<ClanRecord>.Filter.And(
                    requirementsFilter,
                    Builders<ClanRecord>.Filter.Lte(_ => _.Level, maxLevel)
                );
            else
                filter = Builders<ClanRecord>.Filter.Lte(_ => _.Level, maxLevel);

            if (joinOpenOnly)
            {
                var joinFilter = Builders<ClanRecord>.Filter.Eq(_ => _.Desc.JoinOpen, true);
                filter = Builders<ClanRecord>.Filter.And(filter, joinFilter);
            }

            var opts = new FindOptions<ClanRecord>() { Limit = _config.SearchByParamsLimit };
            var r = await (await _clansCollection.FindAsync(filter, opts)).ToListAsync();
            await SetClanTopPlace(r);
            return r;
        }

        public async Task<CreateClanResult> CreateClan(ClanDesc desc, Guid owner)
        {
            using var rollbackContext = new RollbackOnDispose();

            if (string.IsNullOrEmpty(desc.Name))
                return new CreateClanResult()
                {
                    Code = CreateClanResultCode.NameIsEmpty
                };
            if (string.IsNullOrEmpty(desc.Tag))
                return new CreateClanResult()
                {
                    Code = CreateClanResultCode.TagIsEmpty
                };
            /*
            // сторонний чекер имени
            if (!_nameFilter.CheckName(desc.Name))
            {
                return new CreateClanResult()
                {
                    Code = CreateClanResultCode.BadWordName
                };
            }

            // чек тэга
            if (!_nameFilter.CheckTag(desc.Tag))
            {
                return new CreateClanResult()
                {
                    Code = CreateClanResultCode.BadWordTag
                };
            }
            */

            var newClanId = Guid.NewGuid();
            // если уже в клане, то нельзя создать новый
            if (!await BindPlayerToClan(owner, newClanId))
            {
                return new CreateClanResult()
                {
                    Code = CreateClanResultCode.PlayerAlreadyInClan
                };
            }
            rollbackContext.AddRollback(async () => await UnbindPlayerFromClan(owner, newClanId));

            // запись на инсерт
            ClanRecord record = new ClanRecord();
            record.Id = newClanId;
            record.Level = 1;
            record.Owner = owner;
            record.Desc = desc;
            record.Members = new[]
            {
                new ClanPlayer() {Id = owner, Donated = 0, Rating = 0, Role = ""},
            };
            record.Boosters = new ClanBooster[] { };

            // если недопустимы дубликаты имени клана, то делаем так чтоб имя было уникалным или кидаем ошибку
            if (!_config.AllowDuplicatesForClanName)
            {
                var isNameUnique = await _clanNameChecker.BindUniqueName(desc.Name, record.Id);

                if (!isNameUnique)
                    return new CreateClanResult() {Code = CreateClanResultCode.NameAlreadyTaken};
                rollbackContext.AddRollback(async () => await _clanNameChecker.RemoveBinding(desc.Name, record.Id));
            }

            if (!_config.AllowDuplicatesForClanTag && !string.IsNullOrEmpty(record.Desc.Tag))
            {
                var isTagUnique = await _clanTagChecker.BindUniqueName(desc.Tag, record.Id);
                if (!isTagUnique)
                {
                    return new CreateClanResult()
                    {
                        Code = CreateClanResultCode.TagAlreadyTaken
                    };
                }
            }

            await _clansCollection.InsertOneAsync(record);
            rollbackContext.Clear();

            if (desc.Requirements?.Count > 0)
            {
                await SetClanJoinRequirements(record.Id, desc.Requirements.ToArray());
            }

            return new CreateClanResult()
            {
                Code = CreateClanResultCode.Success,
                Id = newClanId
            };
        }

        public async Task<AddPlayerToClanResult> AddPlayerToClan(Guid clanId, Guid playerId, string role, int playersLimit)
        {
            if (!await BindPlayerToClan(playerId, clanId))
            {
                return AddPlayerToClanResult.AlreadyInOtherClan;
            }

            var ok = false;
            try
            {
                var filter = Builders<ClanRecord>.Filter.And(
                    Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId),
                    Builders<ClanRecord>.Filter.SizeLt(_ => _.Members, playersLimit),
                    Builders<ClanRecord>.Filter.Not(
                        Builders<ClanRecord>.Filter.ElemMatch(_ => _.Members, player => player.Id == playerId)
                    )
                );

                var update = Builders<ClanRecord>.Update.Push(_ => _.Members, new ClanPlayer()
                {
                    Id = playerId,
                    Role = role,
                });

                var r = await _clansCollection.UpdateManyAsync(filter, update);
                ok = r.ModifiedCount == 1;

                if (ok)
                {
                    _clanRecordCache.Invalidate(clanId);
                    return AddPlayerToClanResult.Success;
                }
            }
            finally
            {
                if(!ok)
                    await UnbindPlayerFromClan(playerId, clanId);
            }

            // если апдейт не удался то либо клана нет, либо игрок уже в этом клане
            var clan = await GetClan(clanId);
            if (clan == null)
                return AddPlayerToClanResult.NoClan;

            if(clan.Members.Any(_ => _.Id == playerId))
                return AddPlayerToClanResult.AlreadyInThisClan;

            return AddPlayerToClanResult.MembersLimit;
        }

        public async Task<ClanJoinRequestResult> JoinClanRequest(Guid clanId, Guid playerId, TimeSpan requestTTL, int clanMembersLimit)
        {
            var result = new ClanJoinRequestResult();
            var clan = await GetClan(clanId);
            if (clan == null)
            {
                result.Code = ClanJoinRequestResultCode.NoClan;
                return result;
            }

            if (!clan.Desc.JoinOpen)
            {
                result.Code = ClanJoinRequestResultCode.JoinClosed;
                return result;
            }

            var currentClanId = await GetPlayerClanId(playerId);
            if (currentClanId != Guid.Empty)
            {
                if (currentClanId == clanId)
                {
                    result.Code = ClanJoinRequestResultCode.AlreadyInThisClan;
                    return result;
                }

                result.Code = ClanJoinRequestResultCode.AlreadyInOtherClan;
                return result;
            }

            /*
             // до лучших времён
            if (clan.Desc.JoinCheckEligibility && !await _clanPlayerRequirementsChecker.EligibleToJoin(clan, playerId))
            {
                result.Code = ClanJoinRequestResultCode.NotEligible;
                return result;
            }*/

            if (clan.Members.Length >= clanMembersLimit)
                return new ClanJoinRequestResult() { Code = ClanJoinRequestResultCode.MembersLimit };

            // джойн без подтверждения
            if (!clan.Desc.JoinAfterApprove)
            {
                var joinResult = await AddPlayerToClan(clanId, playerId, string.Empty, clanMembersLimit);
                if (joinResult == AddPlayerToClanResult.Success)
                {
                    _notifyPlayers.AskClanToUpdate(clanId);
                    return new ClanJoinRequestResult()
                    {
                        Code = ClanJoinRequestResultCode.Success,
                        Request = new ClanRequestRecord()
                        {
                            ClanId = clanId,
                            PlayerId = playerId,
                            Status = JoinRequestState.Approved,
                            Time = DateTime.UtcNow,
                            Id = Guid.Empty
                        }
                    };
                }
                if(joinResult == AddPlayerToClanResult.NoClan)
                    return new ClanJoinRequestResult() {Code = ClanJoinRequestResultCode.NoClan};
                if(joinResult == AddPlayerToClanResult.AlreadyInOtherClan)
                    return new ClanJoinRequestResult() { Code = ClanJoinRequestResultCode.AlreadyInOtherClan };
                if (joinResult == AddPlayerToClanResult.AlreadyInThisClan)
                    return new ClanJoinRequestResult() { Code = ClanJoinRequestResultCode.AlreadyInThisClan };
                if (joinResult == AddPlayerToClanResult.MembersLimit)
                    return new ClanJoinRequestResult() {Code = ClanJoinRequestResultCode.MembersLimit};
            }

            var alreadySent = await _clanJoinRequestsCollection.CountDocumentsAsync(Builders<MongoClanJoinRequest>.Filter.And(
                Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.ClanId, clanId),
                Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.PlayerId, playerId),
                Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.State, JoinRequestState.Pending)
            ));
            if (alreadySent > 0)
            {
                return new ClanJoinRequestResult()
                {
                    Code = ClanJoinRequestResultCode.AlreadySent
                };
            }

            var createTime = DateTime.UtcNow;
            var expiryTime = createTime + requestTTL;
            var item = new MongoClanJoinRequest()
            {
                ClanId = clanId,
                PlayerId = playerId,
                Time = createTime,
                Expiry = expiryTime,
                State = JoinRequestState.Pending,
                Id = Guid.NewGuid()
            };
            await _clanJoinRequestsCollection.InsertOneAsync(item);
            _notifyPlayers.AskPlayerToUpdateRequests(clan.Owner);

            return new ClanJoinRequestResult()
            {
                Code = ClanJoinRequestResultCode.Success,
                Request = item.From()
            };
        }

        public async Task<ClanRequestRecord[]> GetMyRequests(Guid playerId)
        {
            var filter = Builders<MongoClanJoinRequest>.Filter.And(
                Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.PlayerId, playerId),
                Builders<MongoClanJoinRequest>.Filter.In(_ => _.State, new []
                {
                    JoinRequestState.Approved,
                    JoinRequestState.Pending,
                    JoinRequestState.Rejected
                })
            );
            var r = await (await _clanJoinRequestsCollection.FindAsync(filter)).ToListAsync();
            return r.Select(_ => _.From()).ToArray();
        }

        public async Task<ClanRequestRecord[]> GetClanRequests(Guid clanId)
        {
            var filter = Builders<MongoClanJoinRequest>.Filter.And(
                Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.ClanId, clanId),
                Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.State, JoinRequestState.Pending),
                Builders<MongoClanJoinRequest>.Filter.Gt(_ => _.Expiry, DateTime.UtcNow)
            );
            var r = await (await _clanJoinRequestsCollection.FindAsync(filter)).ToListAsync();
            return r.Select(_ => _.From()).ToArray();
        }

        public Task<AcceptRejectClanRequestResult> AcceptClanRequest(Guid requestId, Guid playerId, string role, int membersLimit)
        {
            return AnswerClanRequest(requestId, playerId, JoinRequestState.Approved, role, membersLimit);
        }

        public Task<AcceptRejectClanRequestResult> RejectClanRequest(Guid requestId, Guid playerId)
        {
            return AnswerClanRequest(requestId, playerId, JoinRequestState.Rejected, string.Empty, 0);
        }

        private async Task<AcceptRejectClanRequestResult> AnswerClanRequest(Guid requestId, Guid playerId,
            JoinRequestState stateToSet, string role, int membersLimit)
        {
            var filter = Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.Id, requestId);
            var existingRequest = await (await _clanJoinRequestsCollection.FindAsync(filter)).SingleOrDefaultAsync();
            if (existingRequest == null)
                return AcceptRejectClanRequestResult.NoRequest;

            var clan = await GetClan(existingRequest.ClanId);
            if (clan == null)
            {
                // заявка есть, клана нет, что-то поломано в БД
                await CancelClanRequest(requestId, existingRequest.PlayerId);
                return AcceptRejectClanRequestResult.InvalidRequestState;
            }
            
            if (clan.Owner != playerId)
                return AcceptRejectClanRequestResult.NotOwner;

            filter = Builders<MongoClanJoinRequest>.Filter.And(
                Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.Id, requestId),
                Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.State, JoinRequestState.Pending),
                Builders<MongoClanJoinRequest>.Filter.Gt(_ => _.Expiry, DateTime.UtcNow)
            );
            var update = Builders<MongoClanJoinRequest>.Update.Set(_ => _.State, stateToSet);
            var r = await _clanJoinRequestsCollection.UpdateManyAsync(filter, update);

            if (r.ModifiedCount == 1)
            {
                if (stateToSet == JoinRequestState.Approved)
                {
                    var addResult = await AddPlayerToClan(existingRequest.ClanId, existingRequest.PlayerId, role, membersLimit);
                    if (addResult == AddPlayerToClanResult.MembersLimit)
                    {
                        filter = Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.Id, existingRequest.Id);
                        update = Builders<MongoClanJoinRequest>.Update.Set(_ => _.State, JoinRequestState.Rejected);
                        await _clanJoinRequestsCollection.UpdateOneAsync(filter, update);
                        _notifyPlayers.AskPlayerToUpdateRequests(existingRequest.PlayerId);
                        _notifyPlayers.AskPlayerToUpdateRequests(playerId);
                        return AcceptRejectClanRequestResult.MembersLimit;
                    }
                    if (addResult != AddPlayerToClanResult.Success)
                    {
                        Log.Error($"Add player {existingRequest.PlayerId} to clan {existingRequest.ClanId} failed. code={addResult}.");
                        return AcceptRejectClanRequestResult.Error;
                    }
                    _notifyPlayers.AskClanToUpdate(existingRequest.ClanId);
                }

                _notifyPlayers.AskPlayerToUpdateRequests(existingRequest.PlayerId);
                return AcceptRejectClanRequestResult.Success;
            }

            filter = Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.PlayerId, playerId);
            existingRequest = await (await _clanJoinRequestsCollection.FindAsync(filter)).SingleOrDefaultAsync();
            if (existingRequest == null)
                return AcceptRejectClanRequestResult.NoRequest;
            if (existingRequest.State != JoinRequestState.Pending)
                return AcceptRejectClanRequestResult.InvalidRequestState;
            _notifyPlayers.AskPlayerToUpdateRequests(playerId);
            return AcceptRejectClanRequestResult.Expired;
        }

        public async Task<CancelClanRequestResult> CancelClanRequest(Guid requestId, Guid playerId)
        {
            var filter = Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.Id, requestId);
            var existingRequest = await (await _clanJoinRequestsCollection.FindAsync(filter)).SingleOrDefaultAsync();
            if (existingRequest == null)
                return CancelClanRequestResult.NoRequest;

            var clan = await GetClan(existingRequest.ClanId);

            if (existingRequest.PlayerId != playerId)
            {
                // только создавший заявку может ее отменить
                return CancelClanRequestResult.NotAuthorized;
            }
            
            // у клан лидера должен быть актуальный список входящих заявок
            var notifyOwner = existingRequest.State == JoinRequestState.Pending;

            var r = await RemoveClanRequest(requestId);
            if (!r)
                return CancelClanRequestResult.NoRequest;
            if(clan != null)
                _notifyPlayers.AskPlayerToUpdateRequests(clan.Owner);

            return CancelClanRequestResult.Success;
        }

        public async Task<CancelClanRequestResult[]> CancelClanRequests(Guid[] requestIds, Guid playerId)
        {
            var result = new CancelClanRequestResult[requestIds.Length];
            for (int i = 0; i < requestIds.Length; i++)
            {
                result[i] = await CancelClanRequest(requestIds[i], playerId);
            }
            return result;
        }

        public async Task<TransferOwnershipResult> TransferOwnership(Guid clanId, Guid fromPlayerId, Guid toPlayerId)
        {
            var filter = Builders<ClanRecord>.Filter.And(
                Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId),
                Builders<ClanRecord>.Filter.Eq(_ => _.Owner, fromPlayerId),
                Builders<ClanRecord>.Filter.ElemMatch(_ => _.Members, player => player.Id == toPlayerId)
            );
            var update = Builders<ClanRecord>.Update.Set(_ => _.Owner, toPlayerId);

            var r = await _clansCollection.UpdateManyAsync(filter, update);
            if (r.ModifiedCount == 1)
            {
                _clanRecordCache.Invalidate(clanId);
                _notifyPlayers.AskClanToUpdate(clanId);
                _notifyPlayers.AskPlayerToUpdateRequests(toPlayerId);
                return TransferOwnershipResult.Success;
            }

            var clan = await GetClan(clanId);
            if (clan == null)
                return TransferOwnershipResult.NoClan;

            if (clan.Owner == toPlayerId)
                return TransferOwnershipResult.AlreadyOwner;

            if (clan.Owner != fromPlayerId)
                return TransferOwnershipResult.NotOwner;

            return TransferOwnershipResult.NotClanMember;
        }

        private async Task RemoveClanRequirements(Guid clanId)
        {
            var filter = Builders<MongoClanRequirement>.Filter.Eq(_ => _.ClanId, clanId);
            await _clanRequirementsCollection.DeleteManyAsync(filter);
        }

        private async Task<bool> RemoveIfEmpty(Guid clanId)
        {
            var filter = Builders<ClanRecord>.Filter.And(
                Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId),
                Builders<ClanRecord>.Filter.Size(_ => _.Members, 0));

            var delete = await _clansCollection.FindOneAndDeleteAsync(filter);
            if(delete == null)
                return false;
            if (!_config.AllowDuplicatesForClanName)
                await _clanNameChecker.RemoveBinding(delete.Desc.Name, clanId);
            if (!_config.AllowDuplicatesForClanTag)
                await _clanTagChecker.RemoveBinding(delete.Desc.Tag, clanId);
            await RemoveClanRequirements(clanId);
            await RemoveClanRequests(clanId);
            await _treasuryAuditDb.ClanDeleted(clanId);
            return true;
        }

        public async Task<LeaveClanResult> LeaveClan(Guid clanId, Guid playerId)
        {
            var filter = Builders<ClanRecord>.Filter.And(
                Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId),
                Builders<ClanRecord>.Filter.ElemMatch(_ => _.Members, player => player.Id == playerId),
                Builders<ClanRecord>.Filter.Or(
                    Builders<ClanRecord>.Filter.Ne(_ => _.Owner, playerId),
                        Builders<ClanRecord>.Filter.Size(_ => _.Members, 1)
                    )
            );
            var update = Builders<ClanRecord>.Update.PullFilter(_ => _.Members, player => player.Id == playerId);

            var r = await _clansCollection.UpdateManyAsync(filter, update);
            if (r.ModifiedCount == 1)
            {
                _clanRecordCache.Invalidate(clanId);
                await UnbindPlayerFromClan(playerId, clanId);
                await RemoveIfEmpty(clanId);
                await _treasuryAuditDb.UserHasLeftTheClan(playerId, clanId);
                _notifyPlayers.AskPlayerToUpdateHisClan(playerId);
                _notifyPlayers.AskClanToUpdate(clanId);
                return LeaveClanResult.Success;
            }

            var clan = await GetClan(clanId);
            if (clan == null)
                return LeaveClanResult.NoClan;
            if (clan.Owner == playerId)
                return LeaveClanResult.OwnerCantLeave;

            return LeaveClanResult.NotMember;
        }

        private async Task RemoveClanRequests(Guid clanId)
        {
            var filter = Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.ClanId, clanId);
            var count = 0L;
            do
            {
                var requests = await (await _clanJoinRequestsCollection.FindAsync(filter)).ToListAsync();
                var removeFilter = Builders<MongoClanJoinRequest>.Filter.In(_ => _.Id, requests.Select(_ => _.Id));
                var removeResult = await _clanJoinRequestsCollection.DeleteManyAsync(removeFilter);
                foreach (var request in requests)
                {
                    _notifyPlayers.AskPlayerToUpdateRequests(request.PlayerId);
                }
                count = await _clanJoinRequestsCollection.CountDocumentsAsync(filter);
            } while (count > 0);
        }

        private async Task RemovePlayerRequests(Guid playerId)
        {
            var filter = Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.PlayerId, playerId);
            var requests = await (await _clanJoinRequestsCollection.FindAsync(filter)).ToListAsync();
            var removeResult = await _clanJoinRequestsCollection.DeleteManyAsync(filter);
            foreach (var request in requests)
            {
                var clan = await GetClan(request.ClanId);
                _notifyPlayers.AskPlayerToUpdateRequests(clan.Owner);
            }
        }

        public async Task<LeaveClanResult> KickClan(Guid clanId, Guid callerId, Guid playerIdToKick)
        {
            if (callerId == playerIdToKick)
                return await LeaveClan(clanId, playerIdToKick);

            var filter = Builders<ClanRecord>.Filter.And(
                Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId),
                Builders<ClanRecord>.Filter.ElemMatch(_ => _.Members, player => player.Id == playerIdToKick),
                Builders<ClanRecord>.Filter.Eq(_ => _.Owner, callerId)
                );
            var update = Builders<ClanRecord>.Update.PullFilter(_ => _.Members, player => player.Id == playerIdToKick);

            var r = await _clansCollection.UpdateManyAsync(filter, update);
            if (r.ModifiedCount == 1)
            {
                _clanRecordCache.Invalidate(clanId);
                await UnbindPlayerFromClan(playerIdToKick, clanId);
                _notifyPlayers.AskPlayerToUpdateHisClan(playerIdToKick);
                _notifyPlayers.AskClanToUpdate(clanId);
                return LeaveClanResult.Success;
            }

            var clan = await GetClan(clanId);
            if (clan == null)
                return LeaveClanResult.NoClan;
            if (clan.Owner != callerId)
                return LeaveClanResult.NotOwner;

            return LeaveClanResult.NotMember;
        }

        private async Task<bool> RemoveClanRequest(Guid requestId)
        {
            var filter = Builders<MongoClanJoinRequest>.Filter.And(Builders<MongoClanJoinRequest>.Filter.Eq(_ => _.Id, requestId));
            var r = await _clanJoinRequestsCollection.DeleteOneAsync(filter);
            return r.DeletedCount == 1;
        }

        public async Task<bool> SetLevel(Guid clanId, Guid callerId, int currLevel, int newLevel, int cost)
        {
            var filter = Builders<ClanRecord>.Filter.And(
                Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId),
                Builders<ClanRecord>.Filter.Eq(_ => _.Owner, callerId),
                Builders<ClanRecord>.Filter.Eq(_ => _.Level, currLevel),
                Builders<ClanRecord>.Filter.Gte(_ => _.TreasuryCurrent, cost)
                );
            var update = Builders<ClanRecord>.Update.Combine(
                Builders<ClanRecord>.Update.Set(_ => _.Level, newLevel),
                Builders<ClanRecord>.Update.Inc(_ => _.TreasuryCurrent, -cost)
                );
            var r = await _clansCollection.UpdateOneAsync(filter, update);
            if (r.ModifiedCount != 1)
                if (r.MatchedCount == 1)
                    return true;
                else
                    return false;
            _clanRecordCache.Invalidate(clanId);
            if (cost > 0)
            {
                var clan = await GetClan(clanId);
                await _treasuryAuditDb.Audit(DateTime.UtcNow, clanId, clan?.Owner ?? Guid.Empty, -cost, (int)AuditReasons.LevelUpCost);
            }
            _notifyPlayers.AskClanToUpdate(clanId);
            return true;
        }

        public async Task<SetClanRatingResult> SetRating(Guid clanId, Guid callerId, int currRating, int newRating)
        {
            var filters = new List<FilterDefinition<ClanRecord>>();
            filters.Add(Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId));
            filters.Add(Builders<ClanRecord>.Filter.Eq(_ => _.Rating, currRating));
            if(!_config.AllMembersCanSetClanRating)
                filters.Add(Builders<ClanRecord>.Filter.Eq(_ => _.Owner, callerId));
            else
                filters.Add(Builders<ClanRecord>.Filter.ElemMatch(_ => _.Members, player => player.Id == callerId));

            var filter = Builders<ClanRecord>.Filter.And(filters);
            var update = Builders<ClanRecord>.Update.Set(_ => _.Rating, newRating);
            var r = await _clansCollection.UpdateOneAsync(filter, update);
            _notifyPlayers.AskClanToUpdate(clanId);
            var ok = r.MatchedCount == 1 && r.ModifiedCount == 1;
            if (ok)
            {
                _clanRecordCache.Invalidate(clanId);
                return SetClanRatingResult.Success;
            }

            if (r.MatchedCount == 1 && r.ModifiedCount == 0)
                    return SetClanRatingResult.OldCurrentValue;
            var clan = await GetClan(clanId);
            if (clan == null)
                return SetClanRatingResult.NoClan;
            if (clan.Members.All(_ => _.Id != callerId))
                return SetClanRatingResult.NotMember;

            return SetClanRatingResult.Error;
        }

        public async Task<bool> SetPlayerRating(Guid clanId, Guid playerId, int currentRating, int newRating)
        {
            var filter = Builders<ClanRecord>.Filter.And(
                Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId),
                Builders<ClanRecord>.Filter.ElemMatch(_ => _.Members, player => player.Id == playerId && player.Rating == currentRating)
            );
            var update = Builders<ClanRecord>.Update.Set(_ => _.Members[-1].Rating, newRating);
            var r = await _clansCollection.FindOneAndUpdateAsync(filter, update);
            _notifyPlayers.AskClanToUpdate(clanId);
            var ok = r != null;
            if(ok)
                _clanRecordCache.Invalidate(clanId);
            return ok;
        }

        public async Task<ActivateBoosterResult> ActivateBooster(Guid clanId, Guid callerId, Guid boosterId, TimeSpan ttl, int price)
        {
            if (price < 1)
            {
                return ActivateBoosterResult.InvalidPrice;
            }

            return await ActivateBooster(clanId, callerId, boosterId, ttl, new ClanCost() {TreasuryCost = price, ConsumableCost = new ClanCostInConsumable[] { }});
        }

        public async Task<ActivateBoosterResult> ActivateBooster(Guid clanId, Guid callerId, Guid boosterId, TimeSpan ttl, ClanCost cost)
        {
            if (cost.TreasuryCost < 0)
            {
                return ActivateBoosterResult.InvalidPrice;
            }

            if (cost.TreasuryCost == 0 && (cost.ConsumableCost?.Length ?? 0) == 0)
            {
                return ActivateBoosterResult.InvalidPrice;
            }

            var activateTime = DateTime.UtcNow;
            var expiryTime = activateTime + ttl;
            using var rollbackContext = new RollbackOnDispose();

            cost.ConsumableCost ??= new ClanCostInConsumable[] { };
            foreach (var consumable in cost.ConsumableCost)
            {
                if (consumable.Amount < 1)
                {
                    return ActivateBoosterResult.InvalidPrice;
                }

                var consFilter = Builders<ClanConsumables>.Filter.And(
                    Builders<ClanConsumables>.Filter.Eq(_ => _.ClanId, clanId),
                    Builders<ClanConsumables>.Filter.Eq(_ => _.ConsumableId, consumable.ConsumableId),
                    Builders<ClanConsumables>.Filter.Gte(_ => _.Balance, consumable.Amount)
                );
                var consUpdate = Builders<ClanConsumables>.Update.Inc(_ => _.Balance, -consumable.Amount);
                var updateResult = _clanConsumablesCollection.FindOneAndUpdateAsync(consFilter, consUpdate);
                if (updateResult != null)
                {
                    rollbackContext.AddRollback(() =>
                    {
                        var _ = Builders<ClanConsumables>.Update.Inc(_ => _.Balance, consumable.Amount);
                        _clanConsumablesCollection.UpdateOne(consFilter, _);
                    });
                }
                else
                {
                    return ActivateBoosterResult.NoMoney;
                }
            }

            var filter = Builders<ClanRecord>.Filter.And(
                Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId),
                Builders<ClanRecord>.Filter.Eq(_ => _.Owner, callerId),
                Builders<ClanRecord>.Filter.Gte(_ => _.TreasuryCurrent, cost.TreasuryCost),
                Builders<ClanRecord>.Filter.ElemMatch(_ => _.Boosters, _ => _.Id == boosterId && !_.Activated)
            );
            var update = Builders<ClanRecord>.Update.Combine(
                Builders<ClanRecord>.Update.Set(_ => _.Boosters[-1].ActivateTime, activateTime),
                Builders<ClanRecord>.Update.Set(_ => _.Boosters[-1].ExpiryTime, expiryTime),
                Builders<ClanRecord>.Update.Set(_ => _.Boosters[-1].Activated, true),
                Builders<ClanRecord>.Update.Inc(_ => _.TreasuryCurrent, -cost.TreasuryCost)
            );

            var r = await _clansCollection.UpdateManyAsync(filter, update);
            if (r.ModifiedCount == 1)
            {
                _clanRecordCache.Invalidate(clanId);
                _notifyPlayers.AskClanToUpdateBoosters(clanId);
                if(cost.ConsumableCost.Length > 0)
                    _notifyPlayers.AskClanToUpdateConsumables(clanId);
                rollbackContext.Clear();
                return ActivateBoosterResult.Success;
            }

            var clan = await GetClan(clanId);
            if (clan == null)
                return ActivateBoosterResult.NoClan;
            if (clan.Owner != callerId)
                return ActivateBoosterResult.NotOwner;
            var booster = clan.Boosters.FirstOrDefault(_ => _.Id == boosterId);
            if (booster.Id == default)
                return ActivateBoosterResult.NoBooster;
            if (booster.Activated)
            {
                if (booster.ExpiryTime < activateTime)
                    return ActivateBoosterResult.Expiried;
                return ActivateBoosterResult.AlreadyActive;
            }

            if (clan.TreasuryCurrent < cost.TreasuryCost)
                return ActivateBoosterResult.NoMoney;

            throw new Exception("Booster added after activate request.");
        }

        public async Task<bool> AddBooster(Guid clanId, Guid callerId, ClanBooster booster)
        {
            var filter = Builders<ClanRecord>.Filter.And(
                Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId),
                Builders<ClanRecord>.Filter.Eq(_ => _.Owner, callerId)
            );
            var update = Builders<ClanRecord>.Update.Push(_ => _.Boosters, booster);
            var r = await _clansCollection.UpdateOneAsync(filter, update);
            var ok = r != null;
            if (ok)
            {
                _clanRecordCache.Invalidate(clanId);
                _notifyPlayers.AskClanToUpdateBoosters(clanId);
            }

            return ok;
        }

        public async Task<bool> AddBoosters(Guid clanId, Guid callerId, ClanBooster[] boosters)
        {
            var filter = Builders<ClanRecord>.Filter.And(
                Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId),
                Builders<ClanRecord>.Filter.Eq(_ => _.Owner, callerId)
                );
            var update = Builders<ClanRecord>.Update.PushEach(_ => _.Boosters, boosters);
            var r = await _clansCollection.UpdateOneAsync(filter, update);
            var ok = r != null;
            if (ok)
            {
                _clanRecordCache.Invalidate(clanId);
                _notifyPlayers.AskClanToUpdateBoosters(clanId);
            }

            return ok;
        }

        public async Task<bool> Donate(Guid clanId, Guid playerId, int amount, int reason)
        {
            var max = int.MaxValue - amount;
            var filter = Builders<ClanRecord>.Filter.And(
                Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId),
                Builders<ClanRecord>.Filter.Lte(_ => _.TreasuryCurrent, max),
                Builders<ClanRecord>.Filter.Lte(_ => _.TreasuryTotal, max),
            Builders<ClanRecord>.Filter.ElemMatch(_ => _.Members, player => player.Id == playerId)
            );
            var update = Builders<ClanRecord>.Update.Combine(
                Builders<ClanRecord>.Update.Inc(_ => _.TreasuryCurrent, amount),
                Builders<ClanRecord>.Update.Inc(_ => _.TreasuryTotal, amount));

            var r = await _clansCollection.UpdateManyAsync(filter, update);

            if (r.ModifiedCount != 1)
                return false;

            filter = Builders<ClanRecord>.Filter.And(
                Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId),
                Builders<ClanRecord>.Filter.ElemMatch(_ => _.Members, player => player.Id == playerId && player.Donated <= max)
            );
            update = Builders<ClanRecord>.Update.Inc(_ => _.Members[-1].Donated, amount);
            await _clansCollection.UpdateManyAsync(filter, update);

            _clanRecordCache.Invalidate(clanId);
            await _treasuryAuditDb.Audit(DateTime.UtcNow, clanId, playerId, amount, reason);
            _notifyPlayers.AskClanToUpdate(clanId);

            return true;
        }

        private async Task<bool> SetClanJoinRequirements(Guid clanId, ClanRequirement[] requirements)
        {
            var filter = Builders<MongoClanRequirement>.Filter.Eq(_ => _.ClanId, clanId);
            var existingReqs = (await _clanRequirementsCollection.FindAsync(filter)).ToList().ToDictionary(_ => _.Name);

            List<WriteModel<MongoClanRequirement>> requests = new List<WriteModel<MongoClanRequirement>>();
            foreach (var requirement in requirements)
            {
                if (existingReqs.ContainsKey(requirement.Name))
                {
                    var reqFilter = Builders<MongoClanRequirement>.Filter.And(
                        Builders<MongoClanRequirement>.Filter.Eq(_ => _.ClanId, clanId),
                        Builders<MongoClanRequirement>.Filter.Eq(_ => _.Name, requirement.Name));
                    var reqUpdate = Builders<MongoClanRequirement>.Update.Set(_ => _.Value, requirement.Value);
                    requests.Add(new UpdateOneModel<MongoClanRequirement>(reqFilter, reqUpdate));
                    existingReqs.Remove(requirement.Name);
                }
                else
                {
                    requests.Add(new InsertOneModel<MongoClanRequirement>(new MongoClanRequirement()
                    {
                        ClanId = clanId,
                        Name = requirement.Name,
                        Value = requirement.Value
                    }));
                }
            }

            foreach (var reqsValue in existingReqs.Values)
            {
                var delFilter = Builders<MongoClanRequirement>.Filter.And(
                    Builders<MongoClanRequirement>.Filter.Eq(_ => _.ClanId, clanId),
                    Builders<MongoClanRequirement>.Filter.Eq(_ => _.Name, reqsValue.Name)
                    );
                requests.Add(new DeleteOneModel<MongoClanRequirement>(filter));
            }

            var result = await _clanRequirementsCollection.BulkWriteAsync(requests);

            var clanFilter = Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId);
            var clanUpdate = Builders<ClanRecord>.Update.Set(_ => _.Desc.Requirements, requirements.ToList());

            await _clansCollection.FindOneAndUpdateAsync(clanFilter, clanUpdate);
            _clanRecordCache.Invalidate(clanId);

            return true;
        }

        public async Task<SetClanDescriptionResult> SetClanDescription(Guid clanId, Guid callerId, string name, string tag, string description,
            string emblem, bool? joinOpen, bool? joinAfterApprove, ClanRequirement[] requirements)
        {
            var reqsStr = "[]";
            if (requirements != null && requirements.Length > 0)
                reqsStr = "[" + string.Join(',', requirements.Select(_ => $"{_.Name}: {_.Value}")) + "]";
            var argsStr =
                $"SetClanDescription(clanId: '{clanId}', callerId: '{callerId}', name: '{name}', tag: '{tag}', description: '{description}', emblem: '{emblem}', joinOpen: '{joinOpen}', joinAfterApprove: '{joinAfterApprove}', requirements: '{reqsStr}')";
            var result = new SetClanDescriptionResult();
            var clan = await GetClan(clanId);
            var filter = Builders<ClanRecord>.Filter.And(
                Builders<ClanRecord>.Filter.Eq(_ => _.Owner, callerId),
                Builders<ClanRecord>.Filter.Eq(_ => _.Id, clanId)
            );
            var changesCount = 0;

            // замена имени клана
            if (!string.IsNullOrEmpty(name) && name != clan?.Desc.Name)
            {
                if (!_config.AllowChangeClanName)
                {
                    Log.Warn("Change clan name not allowed. See config.");
                    result.SetNameResult = SetNameTagCode.NotAllowed;
                }
                else
                {
                    if (clan == null)
                        result.SetNameResult = SetNameTagCode.NoClan;
                    else if (clan.Owner != callerId)
                        result.SetNameResult = SetNameTagCode.NotOwner;
                    else
                    {
                        if (!_config.AllowDuplicatesForClanName)
                        {
                            var isNameUnique = await _clanNameChecker.BindUniqueName(name, clanId);
                            if (!isNameUnique)
                                result.SetNameResult = SetNameTagCode.AlreadyTaken;
                            else
                            {
                                await _clanNameChecker.RemoveBinding(clan.Desc.Name, clanId);
                            }
                        }

                        if (result.SetNameResult == default)
                        {
                            var update = Builders<ClanRecord>.Update.Set(_ => _.Desc.Name, name);
                            var updateResult = await _clansCollection.FindOneAndUpdateAsync(filter, update);
                            if (updateResult == null)
                                Log.Error($"Set name error (clan not found). {argsStr}.");
                            ++changesCount;
                            result.SetNameResult = SetNameTagCode.Success;
                        }
                    }
                }
            }

            // замена тэга клана
            if (!string.IsNullOrEmpty(tag) && tag != clan?.Desc.Tag)
            {
                if (!_config.AllowChangeClanTag)
                {
                    Log.Warn("Change clan tag not allowed. See config.");
                    result.SetTagResult = SetNameTagCode.NotAllowed;
                }
                else
                {
                    if (clan == null)
                        result.SetTagResult = SetNameTagCode.NoClan;
                    else if (clan.Owner != callerId)
                        result.SetTagResult = SetNameTagCode.NotOwner;
                    else
                    {
                        if (!_config.AllowDuplicatesForClanTag)
                        {
                            var isNameUnique = await _clanTagChecker.BindUniqueName(name, clanId);
                            if (!isNameUnique)
                                result.SetTagResult = SetNameTagCode.AlreadyTaken;
                            else
                            {
                                await _clanTagChecker.RemoveBinding(clan.Desc.Name, clanId);
                            }
                        }

                        if (result.SetTagResult == default)
                        {
                            var update = Builders<ClanRecord>.Update.Set(_ => _.Desc.Tag, tag);
                            var updateResult = await _clansCollection.FindOneAndUpdateAsync(filter, update);
                            if (updateResult == null)
                                Log.Error($"Set tag error (clan not found). {argsStr}.");
                            ++changesCount;
                            result.SetTagResult = SetNameTagCode.Success;
                        }
                    }
                }
            }

            // замена описания клана
            if (!string.IsNullOrEmpty(description) && description != clan?.Desc.Description)
            {
                if (clan == null)
                    result.SetDescriptionResult = SetClanDescriptionCode.NoClan;
                else if (clan.Owner != callerId)
                    result.SetDescriptionResult = SetClanDescriptionCode.NotOwner;
                else
                {
                    var update = Builders<ClanRecord>.Update.Set(_ => _.Desc.Description, description);
                    var updateResult = await _clansCollection.FindOneAndUpdateAsync(filter, update);
                    if (updateResult == null)
                        Log.Error($"Set description error (clan not found). {argsStr}.");
                    ++changesCount;
                    result.SetDescriptionResult = SetClanDescriptionCode.Success;
                }
            }

            // замена эмблемы клана
            if (!string.IsNullOrEmpty(emblem) && emblem != clan?.Desc.Emblem)
            {
                if (clan == null)
                    result.SetEmblemResult = SetClanDescriptionCode.NoClan;
                else if (clan.Owner != callerId)
                    result.SetEmblemResult = SetClanDescriptionCode.NotOwner;
                else
                {
                    var update = Builders<ClanRecord>.Update.Set(_ => _.Desc.Emblem, emblem);
                    var updateResult = await _clansCollection.FindOneAndUpdateAsync(filter, update);
                    if (updateResult == null)
                        Log.Error($"Set emblem error (clan not found). {argsStr}.");
                    ++changesCount;
                    result.SetEmblemResult = SetClanDescriptionCode.Success;
                }
            }

            // замена порядка вступления в клан
            if (joinOpen != null || joinAfterApprove != null)
            {
                if (clan == null)
                    result.SetJoinTypeResult = SetClanDescriptionCode.NoClan;
                else if (clan.Owner != callerId)
                    result.SetJoinTypeResult = SetClanDescriptionCode.NotOwner;
                else
                {
                    bool? checkEligibility = requirements?.Length > 0;
                    var list = new List<UpdateDefinition<ClanRecord>>();
                    if(joinOpen != null)
                        list.Add(Builders<ClanRecord>.Update.Set(_ => _.Desc.JoinOpen, joinOpen.Value));
                    if(joinAfterApprove != null)
                        list.Add(Builders<ClanRecord>.Update.Set(_ => _.Desc.JoinAfterApprove, joinAfterApprove.Value));
                    if(checkEligibility != null)
                        list.Add(Builders<ClanRecord>.Update.Set(_ => _.Desc.JoinCheckEligibility, checkEligibility.Value));

                    var update = Builders<ClanRecord>.Update.Combine(list.ToArray());
                    var updateResult = await _clansCollection.FindOneAndUpdateAsync(filter, update);
                    if (updateResult == null)
                        Log.Error($"Set join error (clan not found). {argsStr}.");
                    ++changesCount;
                    result.SetJoinTypeResult = SetClanDescriptionCode.Success;
                }
            }


            // замена минимальных требований к вступающим в клан
            if (requirements != null)
            {
                if (clan == null)
                    result.SetRequirementsResult = SetClanDescriptionCode.NoClan;
                else if (clan.Owner != callerId)
                    result.SetRequirementsResult = SetClanDescriptionCode.NotOwner;
                else
                {
                    var rB = await SetClanJoinRequirements(clanId, requirements);
                    if (rB)
                    {
                        result.SetRequirementsResult = SetClanDescriptionCode.Success;
                        ++changesCount;
                    }
                    else
                    {
                        result.SetRequirementsResult = SetClanDescriptionCode.NoClan;
                        return result;
                    }
                }
            }

            if (changesCount > 0)
            {
                _notifyPlayers.AskClanToUpdate(clanId);
                _clanRecordCache.Invalidate(clanId);
            }

            return result;
        }

        public async Task<PeriodicClanDonationByPlayer> GetDonationsForPeriod(Guid clanId, Guid callerId, TimeSpan period)
        {
            var result = await _treasuryAuditDb.GetClanDonationsByPeriod(period, clanId);
            return result;
        }

        public void Dispose()
        {
            _periodicWorker?.Dispose();
        }

        private async Task<bool> UnbindPlayerFromClan(Guid playerId, Guid clanId)
        {
            var filter = Builders<MongoPlayerToClanMapEntry>.Filter.And(
                Builders<MongoPlayerToClanMapEntry>.Filter.Eq(_ => _.PlayerId, playerId),
                Builders<MongoPlayerToClanMapEntry>.Filter.Eq(_ => _.ClanId, clanId)
            );

            var r = await _playerToClanMap.FindOneAndDeleteAsync(filter);
            return r != null;
        }

        private async Task<Guid> GetPlayerClanId(Guid playerId)
        {
            var filter = Builders<MongoPlayerToClanMapEntry>.Filter.Eq(_ => _.PlayerId, playerId);
            var r = await _playerToClanMap.FindAsync(filter);
            return r.SingleOrDefault()?.ClanId ?? Guid.Empty;
        }

        private async Task<bool> BindPlayerToClan(Guid playerId, Guid clanId)
        {
            var filter = Builders<MongoPlayerToClanMapEntry>.Filter.Eq(_ => _.PlayerId, playerId);
            var update = Builders<MongoPlayerToClanMapEntry>.Update.SetOnInsert(_ => _.ClanId, clanId);
            var opts = new FindOneAndUpdateOptions<MongoPlayerToClanMapEntry>();
            opts.IsUpsert = true;
            opts.ReturnDocument = ReturnDocument.After;
            var r = await _playerToClanMap.FindOneAndUpdateAsync(filter, update, opts);
            if (r.ClanId == clanId)
                return true;
            return false;
        }

        private async Task<bool> IsInClan(Guid playerId)
        {
            var filter = Builders<MongoPlayerToClanMapEntry>.Filter.Eq(_ => _.PlayerId, playerId);
            return await _playerToClanMap.CountDocumentsAsync(filter) > 0;
        }

        public async Task UserDeletedPrivate(Guid userId)
        {
            Log.Debug($"User {userId} deleted on backend.");
            await _treasuryAuditDb.UserDeleted(userId);
            await RemovePlayerRequests(userId);

            var clan = await GetClanByPlayer(userId);
            if (clan == null)
                return;

            if (clan.Members.Length > 1 && clan.Owner == userId)
            {
                await TransferOwnership(clan.Id, clan.Owner, clan.Members.First(_ => _.Id != clan.Owner).Id);
            }
            await LeaveClan(clan.Id, userId);
        }

        public async Task UserBannedPrivate(Guid userId)
        {
            Log.Debug($"User {userId} banned on backend.");
            var clan = await GetClanByPlayer(userId);
            if(clan == null || clan.Owner != userId)
                return;
            var transferToPlayer = clan.Members.Where(_ => _.Id != userId).OrderByDescending(_ => _.Rating)
                .FirstOrDefault();
            if (transferToPlayer == null)
            {
                Log.Debug($"Banned user {userId} is the only clan member.");
            }
            else
            {
                var transferResult = await TransferOwnership(clan.Id, userId, transferToPlayer.Id);
                if (transferResult != TransferOwnershipResult.Success)
                {
                    Log.Error($"Transfer ownership for clan {clan.Id} failed. owner={userId}, candidate={transferToPlayer.Id}.");
                }
            }
        }

        private IMongoCollection<ClanConsumables> _clanConsumablesCollection;
        private IMongoCollection<ClanRecord> _clansCollection;
        private IMongoCollection<MongoClanRequirement> _clanRequirementsCollection;
        private IMongoCollection<MongoPlayerToClanMapEntry> _playerToClanMap;
        private IMongoCollection<MongoClanJoinRequest> _clanJoinRequestsCollection;
        private TreasuryAuditDb _treasuryAuditDb;
        private MongoUniqueName _clanNameChecker;
        private MongoUniqueName _clanTagChecker;
        private ClansConfig _config;
        private PeriodicWorker _periodicWorker;
        private MongoClanBoosterCleaner _boosterCleaner;
        private MemoryCache _topClansCache;
        private SemaphoreSlim _topClansSemaphore;
        private static readonly ILog Log = LogManager.GetLogger(typeof(MongoClanDb));
        private object topClansCacheKey = new object();
        private PestelStatsClient _pestelStatsClient;

        [UnityDI.Dependency] private ClanRecordCache _clanRecordCache;
        [UnityDI.Dependency] private INotifyPlayers _notifyPlayers;
    }
}
