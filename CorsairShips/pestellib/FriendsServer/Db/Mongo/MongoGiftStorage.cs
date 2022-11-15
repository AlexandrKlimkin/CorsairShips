using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using S;
using PestelLib.ServerCommon.Db;

namespace FriendsServer.Db.Mongo
{

    class MongoGiftStorage : IGiftStorage
    {
        private IMongoCollection<Gift> _gifts;
        private NamedCounter _giftId;

        public MongoGiftStorage(string connectionString)
        {
            var url = new MongoUrl(connectionString);
            var c = url.GetServer();
            var db = c.GetDatabase(url.DatabaseName);
            _gifts = db.GetCollection<Gift>("gifts");
            _giftId = new NamedCounter(db, "gift_id");
            _gifts.Indexes.CreateOneAsync(Builders<Gift>.IndexKeys.Ascending(_ => _.From));
            _gifts.Indexes.CreateOneAsync(Builders<Gift>.IndexKeys.Ascending(_ => _.To));
            _gifts.Indexes.CreateOneAsync(Builders<Gift>.IndexKeys.Ascending(_ => _.CreateTime));
        }

        public async Task<Gift> Create(MadId @from, MadId to, int giftId)
        {
            var id = await _giftId.NextId();
            var result = new Gift()
            {
                Id = id,
                CreateTime = DateTime.UtcNow,
                From = @from,
                To = to,
                GameSpecificId = giftId,
                ClaimTime = DateTime.MinValue,
            };
            await _gifts.InsertOneAsync(result);
            return result;
        }

        public async Task<Gift[]> Get(MadId id)
        {
            var filter = Builders<Gift>.Filter.And(Builders<Gift>.Filter.Or(
                Builders<Gift>.Filter.Eq(_ => _.From, id),
                Builders<Gift>.Filter.Eq(_ => _.To, id)),
                Builders<Gift>.Filter.Eq(_ => _.IsClaimed, false));
            var c = await _gifts.FindAsync(filter);
            var result = new List<Gift>();
            while (await c.MoveNextAsync())
            {
                result.AddRange(c.Current);
            }

            return result.ToArray();
        }

        public async Task<List<Gift>> GetById(params long[] giftIds)
        {
            var filter = Builders<Gift>.Filter.In(_ => _.Id, giftIds);
            var c = await _gifts.FindAsync(filter);
            return c.ToList();
        }

        public async Task<Gift[]> GetAll(MadId id)
        {
            var filter = Builders<Gift>.Filter.Or(
                    Builders<Gift>.Filter.Eq(_ => _.From, id),
                    Builders<Gift>.Filter.Eq(_ => _.To, id));
            var c = await _gifts.FindAsync(filter);
            var result = new List<Gift>();
            while (await c.MoveNextAsync())
            {
                result.AddRange(c.Current);
            }

            return result.ToArray();
        }

        public async Task<long> CountGifts(MadId from, MadId to, bool includeClaimed)
        {
            var conditions = new List<FilterDefinition<Gift>>()
            {
                Builders<Gift>.Filter.Eq(_ => _.To, to),
                Builders<Gift>.Filter.Eq(_ => _.From, from)
            };
            if(!includeClaimed)
                conditions.Add(Builders<Gift>.Filter.Eq(_ => _.IsClaimed, false));
            var filter = Builders<Gift>.Filter.And(conditions.ToArray());
            return await _gifts.CountAsync(filter);
        }

        public async Task<Gift> GetLastGift(MadId from, MadId to)
        {
            var filter = Builders<Gift>.Filter.And(
                Builders<Gift>.Filter.Eq(_ => _.To, to),
                Builders<Gift>.Filter.Eq(_ => _.From, from)
            );
            var opt = new FindOptions<Gift>();
            opt.Limit = 1;
            opt.Sort = Builders<Gift>.Sort.Descending(_ => _.CreateTime);
            var r = await _gifts.FindAsync(filter, opt);
            if (!await r.MoveNextAsync() || r.Current == null || r.Current.Count() == 0)
                return null;
            return r.Current.First();
        }

        public async Task<Gift> ClaimGift(long giftId, MadId clamingId)
        {
            var filter = Builders<Gift>.Filter.And(
                Builders<Gift>.Filter.Eq(_ => _.Id, giftId),
                Builders<Gift>.Filter.Eq(_ => _.To, clamingId),
                Builders<Gift>.Filter.Eq(_ => _.IsClaimed, false));
            var update = Builders<Gift>.Update.Combine(
                Builders<Gift>.Update.Set(_ => _.IsClaimed, true),
                Builders<Gift>.Update.Set(_ => _.ClaimTime, DateTime.UtcNow)
                );
            var r = await _gifts.FindOneAndUpdateAsync(filter, update);
            return r;
        }

        static MongoGiftStorage()
        {
            BsonClassMap.RegisterClassMap<Gift>(cm =>
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
