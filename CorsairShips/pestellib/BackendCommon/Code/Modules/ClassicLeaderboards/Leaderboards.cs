using System;
using BackendCommon.Code.Data;
using ClassicLeaderboards;
using Newtonsoft.Json;
using S;
using ServerLib;
using StackExchange.Redis;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using log4net;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PestelLib.ServerCommon.Extensions;
using System.Threading;
using PestelLib.ServerCommon.Db;

namespace BackendCommon.Code.Modules.ClassicLeaderboards
{
    [BsonIgnoreExtraElements]
    class LeaderboardItem
    {
        [BsonId]
        public Guid PlayerId;
        public double Score;
        public string Name;
        public string SocialId;
    }

    // удалить после миграции
    [BsonIgnoreExtraElements]
    class LeaderboardSocialBindingItem
    {
        [BsonId]
        public string SocialId;
        public Guid PlayerId;
    }

    // удалить после миграции
    [BsonIgnoreExtraElements]
    class LeaderboardIdentityItem
    {
        [BsonId]
        public Guid PlayerId;
        public string Name;
        public string SocialId;
    }

    public class Leaderboards : ILeaderboards
    {
        public Leaderboards(MongoUrl url)
        {
            var server = url.GetServer();
            _db = server.GetDatabase(url.DatabaseName);
            _socialBindings = _db.GetCollection<LeaderboardSocialBindingItem>("TempLeaderboardSocialBindings");
            _socialBindings.Indexes.CreateOne(Builders<LeaderboardSocialBindingItem>.IndexKeys.Ascending(_ => _.PlayerId));
            _identities = _db.GetCollection<LeaderboardIdentityItem>("TempLeaderboardIdentities");

            Task.Run(() =>
            {
                var sw = Stopwatch.StartNew();
                MigrateSocialIds();
                MigrateUserIdentity();
                Log.Debug($"Migration complete in {sw.ElapsedMilliseconds}ms.");
            })
            .ReportOnFail()
            .ContinueWith(_ => _migrationEvent.Set());
        }

        public void RegisterRecord(LeaderboardRegisterRecord cmd, Guid userId)
        {
            _migrationEvent.Wait();
            var uid = userId.ToString();

            LeaderboardUtils.CheckLeaderboardName(cmd.Type);
            var top = GetTop(cmd.Type);

            if (RedisUtils.Cache.HashExists("BanList", uid))
            {
                return;
            }

            var filter = Builders<LeaderboardItem>.Filter.Eq(_ => _.PlayerId, userId);
            var updateList = new List<UpdateDefinition<LeaderboardItem>>();

            if (!string.IsNullOrEmpty(cmd.Name))
                updateList.Add(Builders<LeaderboardItem>.Update.Set(_ => _.Name, cmd.Name));
            if (!string.IsNullOrEmpty(cmd.FacebookId))
                updateList.Add(Builders<LeaderboardItem>.Update.Set(_ => _.SocialId, cmd.FacebookId));

            var opts = new FindOneAndUpdateOptions<LeaderboardItem>() { IsUpsert = true };
            if (cmd.Add)
            {
                updateList.Add(Builders<LeaderboardItem>.Update.Inc(_ => _.Score, cmd.Score));
            }
            else
                updateList.Add(Builders<LeaderboardItem>.Update.Set(_ => _.Score, cmd.Score));
            var update = Builders<LeaderboardItem>.Update.Combine(updateList);

            top.FindOneAndUpdate(filter, update, opts);
        }

        public long GetRank(string lb, Guid player)
        {
            _migrationEvent.Wait();
            var top = GetTop(lb);
            var score = GetScore(lb, player);
            var filter = Builders<LeaderboardItem>.Filter.Gte(_ => _.Score, score);
            return top.Count(filter) - 1;
        }

        public LeaderboardRecord GetPlayer(string lb, Guid player)
        {
            _migrationEvent.Wait();
            var top = GetTop(lb);
            var filter = Builders<LeaderboardItem>.Filter.Eq(_ => _.PlayerId, player);
            var item = top.FindSync(filter).SingleOrDefault();
            if (item == null)
                return null;
            var rank = GetRank(lb, player);
            return Convert(item, rank);
        }

        public LeaderboardRecord GetPlayer(string lb, string social)
        {
            _migrationEvent.Wait();
            var top = GetTop(lb);
            var filter = Builders<LeaderboardItem>.Filter.Eq(_ => _.SocialId, social);
            var item = top.FindSync(filter).FirstOrDefault();
            if (item == null)
                return null;
            // rank not needed right now
            return Convert(item, 0);
        }

        public LeaderboardRecord[] GetTop(string lb, int start, int amount)
        {
            _migrationEvent.Wait();
            var top = GetTop(lb);
            var filter = Builders<LeaderboardItem>.Filter.Empty;
            var opt = new FindOptions<LeaderboardItem>();
            opt.Skip = start;
            opt.Limit = amount;
            opt.Sort = Builders<LeaderboardItem>.Sort.Descending(_ => _.Score);
            var c = top.FindSync(filter, opt).ToList();
            return c.Select(_ => Convert(_, start++)).ToArray();
        }

        public LeaderboardRecord[] GetChunk(string lb, Guid[] players)
        {
            _migrationEvent.Wait();
            var top = GetTop(lb);
            var filter = Builders<LeaderboardItem>.Filter.In(_ => _.PlayerId, players);
            var opts = new FindOptions<LeaderboardItem>();
            opts.Sort = Builders<LeaderboardItem>.Sort.Descending(_ => _.Score);
            var c = top.FindSync(filter, opts).ToList();
            return c.Select((_, i) => Convert(_, i)).ToArray();
        }

        public Guid GetUserIdBySocialUserId(string lb, string socialId)
        {
            _migrationEvent.Wait();
            var top = GetTop(lb);
            var filter = Builders<LeaderboardItem>.Filter.Eq(_ => _.SocialId, socialId);
            var item = top.Find(filter).FirstOrDefault();
            return item?.PlayerId ?? Guid.Empty;
        }

        public double GetScore(string lb, Guid player)
        {
            _migrationEvent.Wait();
            var top = GetTop(lb);
            var filter = Builders<LeaderboardItem>.Filter.Eq(_ => _.PlayerId, player);
            var item = top.Find(filter).SingleOrDefault();
            return item?.Score ?? 0;
        }

        public int SeasonIndex => LeaderboardUtils.CurrentSeasonIndex;

        public string SeasonId => LeaderboardUtils.CurrentSeasonId;

        private LeaderboardRecord Convert(LeaderboardItem item, long rank)
        {
            return new LeaderboardRecord
            {

                UserId = item.PlayerId.ToByteArray(),
                FacebookId = item.SocialId,
                Name = item.Name,
                Rank = (int)rank,
                Score = (int)item.Score
            };
        }

        #region Migration

        private bool WaitMigration()
        {
            return _migrationEvent.Wait(TimeSpan.FromSeconds(15));
        }

        private void MigrateUserIdentity()
        {
            const string redisKey = "UserNames";
            var mcount = _identities.Count(Builders<LeaderboardIdentityItem>.Filter.Empty);
            var rcount = RedisUtils.Cache.HashLength(redisKey);

            if (mcount >= rcount)
                return;
            var sw = Stopwatch.StartNew();
            var count = 0;
            foreach (var item in RedisUtils.Cache.HashScan(redisKey))
            {
                try
                {
                    ++count;
                    if (count % 10000 == 0)
                    {
                        var left = (sw.Elapsed.TotalMilliseconds / count) * (rcount - count);
                        Log.Debug($"HSET 'UserNames' migration {count} of {rcount}. Left: {TimeSpan.FromMilliseconds(left)}.");
                    }
                    var guid = new Guid(item.Name.ToString());
                    if (_identities.Count(Builders<LeaderboardIdentityItem>.Filter.Eq(_ => _.PlayerId, guid)) > 0)
                        continue;
                    var data = JsonConvert.DeserializeObject<LeaderboardRecordIdentity>(item.Value.ToString());
                    _identities.InsertOne(new LeaderboardIdentityItem()
                    {
                        PlayerId = guid,
                        Name = data.UserName,
                        SocialId = data.FacebookId
                    });
                }
                catch (MongoWriteException e)
                {
                    if (e?.WriteError.Code != 11000)
                        throw;
                }
                catch (Exception e)
                {
                    Log.Error($"While migrating HSET 'UserNames'. record={item.Name},{item.Value}. " + e);
                }
            }
            Log.Debug($"HSET 'UserNames' migration time: {sw.ElapsedMilliseconds}ms. count={count}.");
        }

        private void MigrateSocialIds()
        {
            const string redisKey = "LeaderboardFacebookIds";
            var mcount = _socialBindings.Count(Builders<LeaderboardSocialBindingItem>.Filter.Empty);
            var rcount = RedisUtils.Cache.HashLength(redisKey);

            if (mcount >= rcount)
                return;
            var sw = Stopwatch.StartNew();
            var count = 0;
            foreach (var item in RedisUtils.Cache.HashScan(redisKey))
            {
                try
                {
                    ++count;
                    if (count % 10000 == 0)
                    {
                        var left = (sw.Elapsed.TotalMilliseconds / count) * (rcount - count);
                        Log.Debug($"HSET 'LeaderboardFacebookIds' migration {count} of {rcount}. Left: {TimeSpan.FromMilliseconds(left)}.");
                    }
                    if (_socialBindings.Count(Builders<LeaderboardSocialBindingItem>.Filter.Eq(_ => _.SocialId, item.Name.ToString())) > 0)
                        continue;
                    _socialBindings.InsertOne(new LeaderboardSocialBindingItem
                    {
                        SocialId = item.Name,
                        PlayerId = new Guid(item.Value.ToString())
                    });
                }
                catch (MongoWriteException e)
                {
                    if (e?.WriteError.Code != 11000)
                        throw;
                }
                catch (Exception e)
                {
                    Log.Error($"While migrating HSET 'LeaderboardFacebookIds'. record={item.Name},{item.Value}. " + e);
                }
            }
            Log.Debug($"HSET 'LeaderboardFacebookIds' migration time: {sw.ElapsedMilliseconds}ms. count={count}.");
        }

        private void MigrateCollection(string collName, IMongoCollection<LeaderboardItem> coll)
        {
            var sw = Stopwatch.StartNew();
            var total = RedisUtils.Cache.SortedSetLength(collName);
            var count = 0;
            foreach (var item in RedisUtils.Cache.SortedSetScan(collName))
            {
                try
                {
                    ++count;
                    if (count % 10000 == 0)
                    {
                        var left = (sw.Elapsed.TotalMilliseconds / count) * (total - count);
                        Log.Debug($"ZSET '{collName}' migration {count} of {total}. Left: {TimeSpan.FromMilliseconds(left)}.");
                    }
                    var guid = new Guid(item.Element.ToString());
                    if (coll.Count(Builders<LeaderboardItem>.Filter.Eq(_ => _.PlayerId, guid)) > 0)
                        continue;
                    var identity = _identities.Find(Builders<LeaderboardIdentityItem>.Filter.Eq(_ => _.PlayerId, guid)).SingleOrDefault();
                    var socialId = identity?.SocialId;
                    var name = identity?.Name ?? "unnamed";
                    if (socialId == null)
                    {
                        var binding = _socialBindings.Find(Builders<LeaderboardSocialBindingItem>.Filter.Eq(_ => _.PlayerId, guid)).FirstOrDefault();
                        socialId = binding?.SocialId ?? "n/a";
                    }
                    coll.InsertOne(new LeaderboardItem()
                    {
                        PlayerId = guid,
                        Score = item.Score,
                        Name = name,
                        SocialId = socialId
                    });
                }
                catch (MongoWriteException e)
                {
                    if (e?.WriteError.Code != 11000)
                        throw;
                }
                catch (Exception e)
                {
                    Log.Error($"While migrating ZSET '{collName}'. record={item.Element},{item.Score}. " + e);
                }
            }
            Log.Debug($"ZSET '{collName}' migration time: {sw.ElapsedMilliseconds}ms. count={count}.");
        }
        #endregion

        private IMongoCollection<LeaderboardItem> GetTop(string type)
        {
            if (_tops.TryGetValue(type, out var result))
                return result;
            lock (_sync)
            {
                if (_tops.TryGetValue(type, out result))
                    return result;
                var coll = _db.GetCollection<LeaderboardItem>(type);
                var mcount = coll.Count(Builders<LeaderboardItem>.Filter.Empty);
                var rcount = RedisUtils.Cache.SortedSetLength(type);
                if (rcount > mcount)
                {
                    coll.Indexes.CreateOne(Builders<LeaderboardItem>.IndexKeys.Descending(_ => _.Score));
                    coll.Indexes.CreateOne(Builders<LeaderboardItem>.IndexKeys.Ascending(_ => _.SocialId));
                    MigrateCollection(type, coll);
                }
                _tops.Add(type, coll);
                return coll;
            }
        }

        private MongoDb _db;
        private object _sync = new object();
        private IMongoCollection<LeaderboardIdentityItem> _identities;
        private IMongoCollection<LeaderboardSocialBindingItem> _socialBindings;
        private Dictionary<string, IMongoCollection<LeaderboardItem>> _tops = new Dictionary<string, IMongoCollection<LeaderboardItem>>();
        private static readonly ILog Log = LogManager.GetLogger(typeof(Leaderboards));
        private ManualResetEventSlim _migrationEvent = new ManualResetEventSlim();
    }
}