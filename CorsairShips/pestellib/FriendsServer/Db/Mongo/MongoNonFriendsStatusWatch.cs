using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using PestelLib.ServerCommon.Db;

namespace FriendsServer.Db.Mongo
{
    [BsonIgnoreExtraElements]
    class NonFriendsStatusWatch
    {
        [BsonId]
        public Guid Watcher;
        public Guid[] Observables;
    }

    class NonFriendsStatusObservables
    {
        [BsonId]
        public Guid Observable;
        public Guid[] Watchers;
    }

    class MongoNonFriendsStatusWatch : INonFriendsStatusWatch
    {
        public MongoNonFriendsStatusWatch(string connectionString)
        {
            var url = new MongoUrl(connectionString);
            var c = url.GetServer();
            var db = c.GetDatabase(url.DatabaseName);
            _watchersCollection = db.GetCollection<NonFriendsStatusWatch>("NonFriendWatchers");
            _observablesCollection = db.GetCollection<NonFriendsStatusObservables>("NonFriendStatusObservables");
        }

        public async Task SetWatch(Guid watcher, Guid[] observables)
        {
            Log.Debug($"SetWatch({watcher}, {JsonConvert.SerializeObject(observables)})");
            var wFilter = Builders<NonFriendsStatusWatch>.Filter.Eq(_ => _.Watcher, watcher);
            var r = await (await _watchersCollection.FindAsync(wFilter)).SingleOrDefaultAsync();
            var hash = new HashSet<Guid>(observables);
            var bulkList = new List<WriteModel<NonFriendsStatusObservables>>();
            if (r != null)
            {
                for (int i = 0; i < r.Observables.Length; ++i)
                {
                    var observable = r.Observables[i];
                    if (hash.Contains(observable))
                    {
                        var filter = Builders<NonFriendsStatusObservables>.Filter.Eq(_ => _.Observable, observable);
                        var update = Builders<NonFriendsStatusObservables>.Update.AddToSet(_ => _.Watchers, watcher);
                        bulkList.Add(new UpdateOneModel<NonFriendsStatusObservables>(filter, update)
                        {
                            IsUpsert = true
                        });
                    }
                    else
                    { 

                        Log.Debug($"Player {watcher} stops watch {observable}.");

                        var filter = Builders<NonFriendsStatusObservables>.Filter.Eq(_ => _.Observable, observable);
                        var update = Builders<NonFriendsStatusObservables>.Update.PullFilter(_ => _.Watchers, guid => guid == watcher);
                        bulkList.Add(new UpdateOneModel<NonFriendsStatusObservables>(filter, update));
                    }
                }
            }
            else
            {
                for (int i = 0; i < observables.Length; ++i)
                {
                    Log.Debug($"Player {watcher} starts watch {observables[i]}.");
                    var filter = Builders<NonFriendsStatusObservables>.Filter.Eq(_ => _.Observable, observables[i]);
                    var update = Builders<NonFriendsStatusObservables>.Update.Push(_ => _.Watchers, watcher);
                    bulkList.Add(new UpdateOneModel<NonFriendsStatusObservables>(filter, update)
                    {
                        IsUpsert = true
                    });
                }
            }

            if(bulkList.Count > 0)
                await _observablesCollection.BulkWriteAsync(bulkList);

            var wUpdate = Builders<NonFriendsStatusWatch>.Update.Set(_ => _.Observables, observables);
            var opt = new UpdateOptions()
            {
                IsUpsert = true
            };
            Log.Debug($"Player {watcher} watch-list {string.Join(" ", observables.Select(_ => _.ToString()))}.");
            await _watchersCollection.UpdateOneAsync(wFilter, wUpdate, opt);
        }

        public async Task<Guid[]> GetWatchersOfObservable(Guid observable)
        {
            var filter = Builders<NonFriendsStatusObservables>.Filter.Eq(_ => _.Observable, observable);
            var r = await (await _observablesCollection.FindAsync(filter)).SingleOrDefaultAsync();
            if (r == null)
                return new Guid[] { };
            return r.Watchers;
        }

        private IMongoCollection<NonFriendsStatusWatch> _watchersCollection;
        private IMongoCollection<NonFriendsStatusObservables> _observablesCollection;
        private static readonly ILog Log = LogManager.GetLogger(typeof(MongoNonFriendsStatusWatch));
    }
}
