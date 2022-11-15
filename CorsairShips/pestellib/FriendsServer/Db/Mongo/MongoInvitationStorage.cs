using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FriendsClient.FriendList;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using S;
using PestelLib.ServerCommon.Db;

namespace FriendsServer.Db.Mongo
{
    class MongoInvitationStorage : IInvitationStorage
    {
        private IMongoCollection<Invitation> _invitations;
        private NamedCounter _invitationIdCounter;
        private bool _cachedCount;
        private long _count;

        public MongoInvitationStorage(string connectionString)
        {
            var url = new MongoUrl(connectionString);
            var c = url.GetServer();
            var db = c.GetDatabase(url.DatabaseName);
            _invitations = db.GetCollection<Invitation>("invitations");
            _invitationIdCounter = new NamedCounter(db, "invite_id");

            _invitations.Indexes.CreateOneAsync(Builders<Invitation>.IndexKeys.Ascending(_ => _.From));
            _invitations.Indexes.CreateOneAsync(Builders<Invitation>.IndexKeys.Ascending(_ => _.To));
            _invitations.Indexes.CreateOneAsync(Builders<Invitation>.IndexKeys.Ascending(_ => _.Expiry));
        }

        public async Task<Invitation> Create(MadId @from, MadId to, TimeSpan ttl)
        {
            var id = await _invitationIdCounter.NextId();
            var result = new Invitation();
            result.Id = id;
            result.Expiry = DateTime.UtcNow + ttl;
            result.From = @from;
            result.To = to;
            result.Status = InviteStatus.Pending;
            await _invitations.InsertOneAsync(result);
            Interlocked.Increment(ref _count);
            return result;
        }

        public async Task<Invitation> Get(long inviteId)
        {
            var filter = Builders<Invitation>.Filter.Eq(_ => _.Id, inviteId);
            var c = await _invitations.FindAsync(filter);
            if (!await c.MoveNextAsync() || c.Current.Count() == 0)
                return null;
            return c.Current.First();
        }

        public async Task<Invitation[]> GetIncoming(MadId id)
        {
            var filter = Builders<Invitation>.Filter.And(
                Builders<Invitation>.Filter.Eq(_ => _.To, id),
                Builders<Invitation>.Filter.Gt(_ => _.Expiry, DateTime.UtcNow));
            var result = new List<Invitation>();
            var c = await _invitations.FindAsync(filter);
            while (await c.MoveNextAsync())
            {
                result.AddRange(c.Current);
            }

            return result.ToArray();
        }

        public async Task<Invitation[]> GetOutgoing(MadId id)
        {
            var filter = Builders<Invitation>.Filter.And(
                Builders<Invitation>.Filter.Eq(_ => _.From, id), 
                Builders<Invitation>.Filter.Gt(_ => _.Expiry, DateTime.UtcNow));
            var result = new List<Invitation>();
            var c = await _invitations.FindAsync(filter);
            while (await c.MoveNextAsync())
            {
                result.AddRange(c.Current);
            }

            return result.ToArray();
        }

        public async Task<long> CountIncoming(MadId id)
        {
            var filter = Builders<Invitation>.Filter.Eq(_ => _.To, id);
            return await _invitations.CountAsync(filter);
        }

        public async Task<Invitation> HasInvite(MadId f, MadId t)
        {
            var opts = new FindOptions<Invitation>();
            opts.Limit = 1;
            var filter = Builders<Invitation>.Filter.And(
                Builders<Invitation>.Filter.Eq(_ => _.From, f),
                Builders<Invitation>.Filter.Eq(_ => _.To, t)
            );
            var r = await _invitations.FindAsync(filter, opts);
            if (!await r.MoveNextAsync() || r.Current.Count() == 0)
                return null;
            return r.Current.First();
        }

        public async Task<Invitation> GetCloseToExpirInvitation()
        {
            var opts = new FindOptions<Invitation>();
            opts.Limit = 1;
            opts.Sort = Builders<Invitation>.Sort.Ascending(_ => _.Expiry);
            var filter = Builders<Invitation>.Filter.Empty;
            var r = await _invitations.FindAsync(filter, opts);
            if (!await r.MoveNextAsync() || r.Current.Count() == 0)
                return null;

            return r.Current.First();
        }

        public async Task<Invitation[]> GetExpired(int amount)
        {
            if(_cachedCount && _count == 0) return new Invitation[] {};
            var dt = DateTime.UtcNow;
            var opts = new FindOptions<Invitation>();
            opts.Limit = amount;
            opts.Sort = Builders<Invitation>.Sort.Ascending(_ => _.Expiry);
            var filter = Builders<Invitation>.Filter.Lt(_ => _.Expiry, dt);
            var r = await _invitations.FindAsync(filter, opts);
            var result = new List<Invitation>();
            while (await r.MoveNextAsync())
            {
                result.AddRange(r.Current);
            }

            return result.ToArray();
        }

        public async Task<long> CleanExpiried()
        {
            var filter = Builders<Invitation>.Filter.Lt(_ => _.Expiry, DateTime.UtcNow);
            var r = await _invitations.DeleteManyAsync(filter);
            Interlocked.Add(ref _count, -r.DeletedCount);
            return r.DeletedCount;
        }

        public async Task<bool> Remove(long id)
        {
            var filter = Builders<Invitation>.Filter.Eq(_ => _.Id, id);
            var r = await _invitations.DeleteOneAsync(filter);
            Interlocked.Add(ref _count, -r.DeletedCount);
            return r.DeletedCount > 0;
        }

        public async Task<long> RemoveMany(params long[] ids)
        {
            var filter = Builders<Invitation>.Filter.In(_ => _.Id, ids);
            var r = await _invitations.DeleteManyAsync(filter);
            Interlocked.Add(ref _count, -r.DeletedCount);
            return r.DeletedCount;
        }

        public async Task<long> Count()
        {
            if (_cachedCount)
                return _count;
            var count = await _invitations.CountAsync(Builders<Invitation>.Filter.Empty);
            _cachedCount = true;
            return Interlocked.Add(ref _count, count);
        }

        static MongoInvitationStorage()
        {
            BsonClassMap.RegisterClassMap<Invitation>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.Id);
                cm.MapField(_ => _.From).SetSerializer(MadIdSerializer.Instance);
                cm.MapField(_ => _.To).SetSerializer(MadIdSerializer.Instance);
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}
