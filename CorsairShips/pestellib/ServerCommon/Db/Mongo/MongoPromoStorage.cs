using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace PestelLib.ServerCommon.Db.Mongo
{
    public class MongoPromoStorage : IPromoStorage
    {
        public MongoPromoStorage(MongoUrl mongoUrl)
        {
            var server = mongoUrl.GetServer();
            var db = server.GetDatabase(mongoUrl.DatabaseName);
            _promoInfos = db.GetCollection<PromoInfo>("PromoInfos");
        }

        public bool ActivatePromo(string promoId)
        {
            var item = Get(promoId);
            if (item == null)
                return false;
            var filter = Builders<PromoInfo>.Filter.And( Builders<PromoInfo>.Filter.Eq(_ => _.Id, promoId),
                Builders<PromoInfo>.Filter.Lt(_ => _.ActivateCount, item.ActivateMax));
            var update = Builders<PromoInfo>.Update.Inc(_ => _.ActivateCount, 1);
            var r = _promoInfos.UpdateOne(filter, update);
            return r.ModifiedCount > 0;
        }

        public bool BindPlayerToPromo(string promoId, Guid playerId)
        {
            var filter = Builders<PromoInfo>.Filter.And(
                Builders<PromoInfo>.Filter.Eq(_ => _.Id, promoId),
                Builders<PromoInfo>.Filter.Not(
                    Builders<PromoInfo>.Filter.AnyEq(_ => _.ActivatedByPlayers, playerId)
                    )
                );
            var update = Builders<PromoInfo>.Update.Push(_ => _.ActivatedByPlayers, playerId);
            var r = _promoInfos.FindOneAndUpdate(filter, update);
            return r != null;
        }

        public bool Create(PromoInfo info)
        {
            try
            {
                _promoInfos.InsertOne(info);
                return true;
            }
            catch (MongoWriteException e)
            {
                if (e.WriteError != null && e.WriteError.Code == 11000)
                    return false;
                throw;
            }
        }

        public bool Create(string promoId, string func, string param, int count)
        {
            var promo = new PromoInfo()
            {
                Id = promoId,
                Function = func,
                Parameter = param,
                ActivateMax = count
            };

            return Create(promo);
        }

        public PromoInfo Get(string promoId)
        {
            var cur = _promoInfos.Find(Builders<PromoInfo>.Filter.Eq(_ => _.Id, promoId));
            return cur.SingleOrDefault();
        }

        public bool UsedByPlayer(string promoId, Guid playerId)
        {
            var count = 
            _promoInfos.Count(Builders<PromoInfo>.Filter.And(
                Builders<PromoInfo>.Filter.Eq(_ => _.Id, promoId),
                Builders<PromoInfo>.Filter.AnyEq(_ => _.ActivatedByPlayers, playerId)
                ));
            return count > 0;
        }

        static MongoPromoStorage()
        {
            BsonClassMap.RegisterClassMap<PromoInfo>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.MapIdField(_ => _.Id);
            });
        }

        private IMongoCollection<PromoInfo> _promoInfos;
    }
}
