using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClansClientLib;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PestelLib.ServerCommon.Db;
using UnityDI;

namespace ClansServerLib.Mongo
{
    [BsonIgnoreExtraElements]
    class MongoExpiredBooster
    {
        [BsonId]
        public Object Id;
        public Guid ClanId;
        public ClanBooster Booster;
    }

    class MongoClanBoosterCleaner
    {
        public MongoClanBoosterCleaner(MongoDb db, IMongoCollection<ClanRecord> clanCollection)
        {
            ContainerHolder.Container.BuildUp(this);
            _config = ClansConfigCache.Get();
            _clanCollection = clanCollection;
            _expiredBoosters = new Dictionary<Guid, HashSet<ClanBooster>>();
            if(_config.EnableBoosterHistory)
                _expiredBoostersCollection = db.GetCappedCollection<MongoExpiredBooster>("ExpiredBoosters", maxSize: 0x40000000);
        }

        public void CheckExpiredBoosts(ClanRecord record)
        {
            if (record?.Boosters.Length > 0)
            {
                var dt = DateTime.UtcNow + _config.BoosterCleanupDelay;
                var expiried = record.Boosters.Where(_ => _.Activated && _.ExpiryTime > dt).ToArray();
                lock (_expiredBoosters)
                {
                    if (!_expiredBoosters.TryGetValue(record.Id, out var list))
                        _expiredBoosters[record.Id] = new HashSet<ClanBooster>(expiried);
                    else
                    {
                        for (int j = 0; j < expiried.Length; j++)
                        {
                            list.Add(expiried[j]);
                        }
                    }
                        
                }
            }
        }

        public async Task Run()
        {
            var insertList = new List<InsertOneModel<MongoExpiredBooster>>();
            var updateList = new List<UpdateOneModel<ClanRecord>>();
            Dictionary<Guid, HashSet<ClanBooster>> copy;
            lock (_expiredBoosters)
            {
                copy = new Dictionary<Guid, HashSet<ClanBooster>>(_expiredBoosters);
                _expiredBoosters.Clear();
            }
            if(copy.Count < 1)
                return;
            foreach (var kv in copy)
            {
                foreach (var clanBooster in kv.Value)
                {
                    if(_config.EnableBoosterHistory)
                        insertList.Add(new InsertOneModel<MongoExpiredBooster>(new MongoExpiredBooster()
                        {
                            Id = ObjectId.GenerateNewId(),
                            Booster = clanBooster,
                            ClanId = kv.Key
                        }));
                    var remList = kv.Value.Select(_ => _.Id).ToArray();
                    var filter = Builders<ClanRecord>.Filter.Eq(_ => _.Id, kv.Key);
                    var boosterFilter = Builders<ClanBooster>.Filter.In(_ => _.Id, remList);
                    var update = Builders<ClanRecord>.Update.PullFilter(_ => _.Boosters, boosterFilter);
                    updateList.Add(new UpdateOneModel<ClanRecord>(filter, update));
                }
            }

            var tasks = new List<Task>();

            if (updateList.Count > 0)
            {
                tasks.Add(_clanCollection.BulkWriteAsync(updateList));
            }

            if(insertList.Count > 0)
            {
                tasks.Add(_expiredBoostersCollection?.BulkWriteAsync(insertList) ?? Task.CompletedTask);
            }

            if(tasks.Count > 0)
                await Task.WhenAll(tasks.ToArray());

            foreach (var kv in copy)
            {
                _clanRecordCache.Invalidate(kv.Key);
                _notifyPlayers.AskClanToUpdateBoosters(kv.Key);
            }
        }

        private ClansConfig _config;
        private IMongoCollection<ClanRecord> _clanCollection;
        private IMongoCollection<MongoExpiredBooster> _expiredBoostersCollection;
        private Dictionary<Guid, HashSet<ClanBooster>> _expiredBoosters;
        [Dependency] private ClanRecordCache _clanRecordCache;
        [Dependency] private INotifyPlayers _notifyPlayers;
    }
}
