using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FriendsClient.Sources;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using S;
using PestelLib.ServerCommon.Db;

namespace FriendsServer.Db.Mongo
{

    class FriendMongo : Friend
    {
        public DateTime RegTime;
        public DateTime LastLoad;
    }

    class MongoFriendsStorage : IFriendStorage
    {
        private IMongoCollection<FriendMongo> _friends;
        private NamedCounter _friendIdCounter;

        public MongoFriendsStorage(string connectionString)
        {
            var url = new MongoUrl(connectionString);
            var c = url.GetServer();
            var db = c.GetDatabase(url.DatabaseName);
            _friends = db.GetCollection<FriendMongo>("friends");
            _friendIdCounter = new NamedCounter(db, "friend_id");

            _friends.Indexes.CreateOneAsync(Builders<FriendMongo>.IndexKeys.Ascending(_ => _.PlayerId), new CreateIndexOptions() {Unique = true});
            _friends.Indexes.CreateOneAsync(Builders<FriendMongo>.IndexKeys.Descending(_ => _.RegTime));
            _friends.Indexes.CreateOneAsync(Builders<FriendMongo>.IndexKeys.Descending(_ => _.LastLoad));
        }

        public async Task<Friend> GetOrCreate(Guid playerId)
        {
            var filter = Builders<FriendMongo>.Filter.Eq(_ => _.PlayerId, playerId);
            var update = Builders<FriendMongo>.Update.Set(_ => _.LastLoad, DateTime.UtcNow);
            var r = await _friends.FindOneAndUpdateAsync(filter, update);
            if (r != null)
                return r;
            
            var id = await _friendIdCounter.NextId();
            var dt = DateTime.UtcNow;
            if (id > MadId.MaxValue) throw new OverflowException("Cant generate MadId.");
            r = new FriendMongo()
            {
                Id = new MadId((uint)id),
                LastLoad = dt,
                RegTime = dt,
                PlayerId = playerId,
                Status = FriendStatus.Online,
                LastStatus = dt,
                Friends = new MadId[] {},
            };
            try
            {
                await _friends.InsertOneAsync(r);
                return r;
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError.Code != 11000)
                {
                    var c = await _friends.FindAsync(filter);
                    await c.MoveNextAsync();
                    return c.Current.First();
                }
                throw;
            }
        }

        public async Task<Friend> Get(MadId id)
        {
            var filter = Builders<FriendMongo>.Filter.Eq(_ => _.Id, id);
            var c = await _friends.FindAsync(filter);
            if (!await c.MoveNextAsync() || c.Current.Count() == 0)
                return null;
            return c.Current.First();
        }

        public async Task<Friend[]> GetMany(params Guid[] playerIds)
        {
            var filter = Builders<FriendMongo>.Filter.In(_ => _.PlayerId, playerIds);
            var c = await _friends.FindAsync(filter);
            var result = new List<Friend>();
            while (await c.MoveNextAsync())
            {
                result.AddRange(c.Current);
            }
            return result.ToArray();
        }

        public async Task<Friend[]> GetMany(params MadId[] ids)
        {
            if (ids.Length == 0)
                return new Friend[] { };
            var filter = Builders<FriendMongo>.Filter.In(_ => _.Id, ids);
            var c = await _friends.FindAsync(filter);
            List<Friend> result = new List<Friend>();
            while (await c.MoveNextAsync())
            {
                result.AddRange(c.Current);
            }

            return result.ToArray();
        }

        public async Task<bool> ChangeStatus(MadId id, int status)
        {
            var filter = Builders<FriendMongo>.Filter.Eq(_ => _.Id, id);
            var update = Builders<FriendMongo>.Update.Combine(
                Builders<FriendMongo>.Update.Set(_ => _.Status, status),
                Builders<FriendMongo>.Update.Set(_ => _.LastStatus, DateTime.UtcNow)
            );
            var r = await _friends.FindOneAndUpdateAsync(filter, update);
            return r != null;
        }

        public async Task<bool> CheckChangeStatus(MadId id, int fromStatus, int finalStatus)
        {
            var filter = Builders<FriendMongo>.Filter.And(
                Builders<FriendMongo>.Filter.Eq(_ => _.Id, id),
                Builders<FriendMongo>.Filter.Eq(_ => _.Status, fromStatus));
            var update = Builders<FriendMongo>.Update.Combine(
                Builders<FriendMongo>.Update.Set(_ => _.Status, finalStatus),
                Builders<FriendMongo>.Update.Set(_ => _.LastStatus, DateTime.UtcNow)
            );
            var r = await _friends.FindOneAndUpdateAsync(filter, update);
            return r != null;
        }

        public async Task<bool> AddFriend(MadId friendList, MadId friend)
        {
            var filter = Builders<FriendMongo>.Filter.And(
                Builders<FriendMongo>.Filter.Eq(_ => _.Id, friendList),
                Builders<FriendMongo>.Filter.Not(
                    Builders<FriendMongo>.Filter.AnyEq(_ => _.Friends, friend)
                    )
            );
            var update = Builders<FriendMongo>.Update.Push(_ => _.Friends, friend);
            var r = await _friends.FindOneAndUpdateAsync(filter, update);
            return r != null;
        }

        public async Task<bool> RemoveFriend(MadId friendList, MadId friend)
        {
            var filter = Builders<FriendMongo>.Filter.Eq(_ => _.Id, friendList);
            var update = Builders<FriendMongo>.Update.Pull(_ => _.Friends, friend);
            var r = await _friends.FindOneAndUpdateAsync(filter, update);
            return r != null;
        }

        public async Task<bool> Remove(MadId item)
        {
            var filter = Builders<FriendMongo>.Filter.Eq(_ => _.Id, item);
            var r = await _friends.FindOneAndDeleteAsync(filter);
            return r != null;
        }

        public async Task<bool> IsFriends(MadId first, MadId second)
        {
            var filter = Builders<FriendMongo>.Filter.And(
                Builders<FriendMongo>.Filter.Eq(_ => _.Id, first),
                Builders<FriendMongo>.Filter.AnyEq(_ => _.Friends, second));
            var r = await _friends.CountAsync(filter);
            return r > 0;
        }

        static MongoFriendsStorage()
        {
            BsonClassMap.RegisterClassMap<Friend>(cm =>
            {
                cm.AutoMap();
                cm.MapField(_ => _.Id).SetSerializer(MadIdSerializer.Instance);
                cm.MapField(_ => _.Friends).SetSerializer(MadIdArraySerializer.Instance);
                cm.SetIgnoreExtraElements(true);
            });

            BsonClassMap.RegisterClassMap<FriendMongo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}
