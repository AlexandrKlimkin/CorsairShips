using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using PestelLib.ServerCommon.Extensions;
using PestelLib.ServerCommon.Messaging;
using PestelLib.ServerCommon.Db;
using S;

namespace ServerLib.Modules.Messages
{
    

    [BsonIgnoreExtraElements]
    public class BroadcastMessage
    {
        [BsonId]
        public Guid Id;
        public long SerialId;
        public ServerMessage Message;
        public DateTime Time;
        public DateTime ExpireTime;
        public string[] SharedLogicVersionFilter;
        public string[] AbTestingGroupFilter;
        public string[] GeoFilterFilter;
        public string[] SystemLanguageFilter;
        public string[] PlayerIds;
        public int ReadCount;
        public bool Removed;
        public bool WelcomeLetter; // all user will get this message
    }

    public class BroadcastMessagePlayerCache
    {
        [BsonId]
        public string PlayerId;
        public long[] LastInbox;
    }

    public class BroadcastMessageStorage
    {
        private IMongoCollection<BroadcastMessage> _messages;
        private IMongoCollection<BroadcastMessagePlayerCache> _inboxCache;

        public BroadcastMessageStorage()
        {
            var mongoUrl = new MongoUrl(AppSettings.Default.PersistentStorageSettings.StorageConnectionString);
            var c = mongoUrl.GetServer();
            var db = c.GetDatabase(mongoUrl.DatabaseName);
            _messages = db.GetCollection<BroadcastMessage>(AppSettings.Default.PersistentStorageSettings.BroadcastMessageCollectionName);
            _inboxCache = db.GetCollection<BroadcastMessagePlayerCache>(AppSettings.Default.PersistentStorageSettings.BroadcastMessagePlayerChacheName);

            _messages.Indexes.CreateOneAsync(Builders<BroadcastMessage>.IndexKeys.Descending(_ => _.Time)).ReportOnFail();
            _messages.Indexes.CreateOneAsync(Builders<BroadcastMessage>.IndexKeys.Ascending(_ => _.SerialId)).ReportOnFail();
            _messages.Indexes.CreateOneAsync(Builders<BroadcastMessage>.IndexKeys.Ascending(_ => _.Removed)).ReportOnFail();
        }

        public BroadcastMessage[] GetLastMessages(int count)
        {
            var result = new List<BroadcastMessage>();
            var filter = Builders<BroadcastMessage>.Filter.Empty;
            var sort = Builders<BroadcastMessage>.Sort.Descending(_ => _.SerialId);
            var opts = new FindOptions<BroadcastMessage>();
            opts.Limit = count;
            opts.Sort = sort;
            var cur = _messages.FindSync(filter, opts);
            while (cur.MoveNext() && cur.Current != null)
            {
                result.AddRange(cur.Current);
            }
            return result.ToArray();
        }

        private long[] GetLastInbox(string playerId)
        {
            var cur = _inboxCache.FindSync(Builders<BroadcastMessagePlayerCache>.Filter.Eq(_ => _.PlayerId, playerId));
            if (!cur.MoveNext())
                return new long[] { };
            if (cur.Current == null)
                return new long[] { };
            var cache = cur.Current.FirstOrDefault();
            return cache?.LastInbox ?? new long[] { };
        }

        private void ResetCache(string playerId, long[] inbox)
        {
            var filter = Builders<BroadcastMessagePlayerCache>.Filter.Eq(_ => _.PlayerId, playerId);
            var update = Builders<BroadcastMessagePlayerCache>.Update.Set(_ => _.LastInbox, inbox);
            var opts = new FindOneAndUpdateOptions<BroadcastMessagePlayerCache>();
            opts.IsUpsert = true;
            _inboxCache.FindOneAndUpdate(filter, update, opts);
        }

        private void AppendCache(string playerId, long[] inbox)
        {
            var filter = Builders<BroadcastMessagePlayerCache>.Filter.Eq(_ => _.PlayerId, playerId);
            var update = Builders<BroadcastMessagePlayerCache>.Update.PushEach(_ => _.LastInbox, inbox);
            var opts = new FindOneAndUpdateOptions<BroadcastMessagePlayerCache>();
            opts.IsUpsert = true;
            _inboxCache.FindOneAndUpdate(filter, update, opts);
        }

        private BroadcastMessage GetWelcomeLetter(long stateBirthday, long[] lastInbox)
        {
            var welcomeFilter = Builders<BroadcastMessage>.Filter.And(
                Builders<BroadcastMessage>.Filter.Eq(_ => _.WelcomeLetter, true),
                Builders<BroadcastMessage>.Filter.Eq(_ => _.Removed, false)
                
            );
            // if already got welcome message left it as is except it was deleted
            if (lastInbox.Length > 0)
            {
                welcomeFilter = Builders<BroadcastMessage>.Filter.And(
                    welcomeFilter,
                    Builders<BroadcastMessage>.Filter.Or(
                        Builders<BroadcastMessage>.Filter.In(_ => _.SerialId, lastInbox),
                        Builders<BroadcastMessage>.Filter.Lte(_ => _.SerialId, stateBirthday)
                        )
                    );
            }
            // players with empty inbox will get most recent welcome message created before player state creation time
            else
            {
                welcomeFilter = Builders<BroadcastMessage>.Filter.And(
                    welcomeFilter,
                    Builders<BroadcastMessage>.Filter.Lte(_ => _.SerialId, stateBirthday)
                );
            }

            var opts = new FindOptions<BroadcastMessage>();
            opts.Sort = Builders<BroadcastMessage>.Sort.Descending(_ => _.SerialId);
            opts.Limit = 1;
            var cursor = _messages.FindSync(welcomeFilter, opts);
            if (!cursor.MoveNext() || !cursor.Current.Any())
                return null;
            return cursor.Current.First();
        }

        public ServerMessage[] GetMessages(long lastSeenSerialId, string playerId, Dictionary<string, string> userFilter, bool syncInboxAdditive, long stateBirthday)
        {
            var lastInbox = GetLastInbox(playerId);
            var filterDefs = new List<FilterDefinition<BroadcastMessage>>();
            filterDefs.Add(Builders<BroadcastMessage>.Filter.Gt(_ => _.SerialId, lastSeenSerialId));
            filterDefs.Add(Builders<BroadcastMessage>.Filter.Eq(_ => _.WelcomeLetter, false));
            string filterValue;
            var dt = DateTime.UtcNow;
            filterDefs.Add(Builders<BroadcastMessage>.Filter.Lt(_ => _.Time, dt));
            if (userFilter.TryGetValue(SharedLogicVersion.Name, out filterValue))
            {
                var groupping = Builders<BroadcastMessage>.Filter.Or(
                    Builders<BroadcastMessage>.Filter.Not(Builders<BroadcastMessage>.Filter.Exists(_ => _.SharedLogicVersionFilter)),
                    Builders<BroadcastMessage>.Filter.Eq(_ => _.SharedLogicVersionFilter, null),
                    Builders<BroadcastMessage>.Filter.AnyEq(_ => _.SharedLogicVersionFilter, filterValue));
                filterDefs.Add(groupping);
            }

            if (userFilter.TryGetValue(AbTestingGroupFilter.Name, out filterValue))
            {
                var groupping = Builders<BroadcastMessage>.Filter.Or(
                    Builders<BroadcastMessage>.Filter.Not(Builders<BroadcastMessage>.Filter.Exists(_ => _.AbTestingGroupFilter)),
                    Builders<BroadcastMessage>.Filter.Eq(_ => _.AbTestingGroupFilter, null),
                    Builders<BroadcastMessage>.Filter.AnyEq(_ => _.AbTestingGroupFilter, filterValue));
                filterDefs.Add(groupping);
            }

            if (userFilter.TryGetValue(GeoFilter.Name, out filterValue))
            {
                var groupping = Builders<BroadcastMessage>.Filter.Or(
                    Builders<BroadcastMessage>.Filter.Not(Builders<BroadcastMessage>.Filter.Exists(_ => _.GeoFilterFilter)),
                    Builders<BroadcastMessage>.Filter.Eq(_ => _.GeoFilterFilter, null),
                    Builders<BroadcastMessage>.Filter.AnyEq(_ => _.GeoFilterFilter, filterValue));
                filterDefs.Add(groupping);
            }

            if (userFilter.TryGetValue(SystemLanguageFilter.Name, out filterValue))
            {
                var groupping = Builders<BroadcastMessage>.Filter.Or(
                    Builders<BroadcastMessage>.Filter.Not(Builders<BroadcastMessage>.Filter.Exists(_ => _.SystemLanguageFilter)),
                    Builders<BroadcastMessage>.Filter.Eq(_ => _.SystemLanguageFilter, null),
                    Builders<BroadcastMessage>.Filter.AnyEq(_ => _.SystemLanguageFilter, filterValue));
                filterDefs.Add(groupping);
            }

            {
                var groupping = Builders<BroadcastMessage>.Filter.Or(
                    Builders<BroadcastMessage>.Filter.Not(Builders<BroadcastMessage>.Filter.Exists(_ => _.PlayerIds)),
                    Builders<BroadcastMessage>.Filter.Eq(_ => _.PlayerIds, null),
                    Builders<BroadcastMessage>.Filter.AnyEq(_ => _.PlayerIds, playerId));
                filterDefs.Add(groupping);
            }

            filterDefs.Add(Builders<BroadcastMessage>.Filter.Eq(_ =>_.Removed, false));
            if (lastInbox != null && lastInbox.Length > 0)
                filterDefs.Add(Builders<BroadcastMessage>.Filter.Or(
                    Builders<BroadcastMessage>.Filter.Gt(_ => _.ExpireTime, dt),
                    Builders<BroadcastMessage>.Filter.In(_ => _.SerialId, lastInbox)));
            else
                filterDefs.Add(Builders<BroadcastMessage>.Filter.Gt(_ => _.ExpireTime, dt));

            var result = new List<ServerMessage>();
            var serialz = new List<long>();
            var ids = new List<Guid>();
            if (!syncInboxAdditive)
            {
                var welcomeLetter = GetWelcomeLetter(stateBirthday, lastInbox);
                if (welcomeLetter != null)
                {
                    ids.Add(welcomeLetter.Id);
                    serialz.Add(welcomeLetter.SerialId);
                    result.Add(welcomeLetter.Message);
                }
            }
            var filter = Builders<BroadcastMessage>.Filter.And(filterDefs.ToArray());
            var cursor = _messages.FindSync(filter);
            while (cursor.MoveNext())
            {
                if(cursor.Current == null)
                    return new ServerMessage[] {};

                foreach (var message in cursor.Current)
                {
                    ids.Add(message.Id);
                    serialz.Add(message.SerialId);
                    result.Add(message.Message);
                }
            }

            if (ids.Count > 0)
            {
                filter = Builders<BroadcastMessage>.Filter.In(_ => _.Id, ids);
                var update = Builders<BroadcastMessage>.Update.Inc(_ => _.ReadCount, 1);
                _messages.UpdateManyAsync(filter, update).ReportOnFail();
            }
            if(syncInboxAdditive)
                AppendCache(playerId, serialz.ToArray());
            else
                ResetCache(playerId, serialz.ToArray());

            return result.ToArray();
        }

        public void PushMessage(long serialId, DateTime deliveryTime, DateTime expireDate, ServerMessage message, BroadcastMessageFilter filter = null, bool welcomeLetter = false)
        {
            var bm = new BroadcastMessage()
            {
                Id = Guid.NewGuid(),
                Time = deliveryTime,
                ExpireTime = expireDate,
                Message = message,
                SerialId = serialId,
                WelcomeLetter = welcomeLetter
            };
            if (filter != null)
            {
                string[] filterVals;
                if (filter.TryGetValue(SharedLogicVersion.Name, out filterVals))
                {
                    bm.SharedLogicVersionFilter = filterVals;
                }

                if (filter.TryGetValue(AbTestingGroupFilter.Name, out filterVals))
                {
                    bm.AbTestingGroupFilter = filterVals;
                }

                if (filter.TryGetValue(GeoFilter.Name, out filterVals))
                {
                    bm.GeoFilterFilter = filterVals;
                }

                if (filter.TryGetValue(SystemLanguageFilter.Name, out filterVals))
                {
                    bm.SystemLanguageFilter = filterVals;
                }

                if (filter.TryGetValue(PlayerIdsFilter.Name, out filterVals))
                {
                    bm.PlayerIds = filterVals;
                }
            }

            _messages.InsertOne(bm);
        }

        public async Task RemoveMessage(Guid messageId)
        {
            var filter = Builders<BroadcastMessage>.Filter.Eq(_ => _.Id, messageId);
            var update = Builders<BroadcastMessage>.Update.Set(_ => _.Removed, true);
            await _messages.UpdateOneAsync(filter, update);
        }

        public async Task<bool> ChangePlayerFilter(long serialId, string[] values)
        {
            var filter = Builders<BroadcastMessage>.Filter.Eq(_ => _.SerialId, serialId);
            var update = Builders<BroadcastMessage>.Update.Set(_ => _.PlayerIds, values);
            var r = await _messages.UpdateOneAsync(filter, update);
            return r.IsAcknowledged && r.ModifiedCount > 0;
        }
    }
}