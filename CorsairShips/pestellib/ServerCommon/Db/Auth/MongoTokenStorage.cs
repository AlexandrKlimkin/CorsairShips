using System;
using System.Linq;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace PestelLib.ServerCommon.Db.Auth
{
    public class MongoTokenStorage : ITokenStore, ITokenStoreWriter
    {
        private IMongoCollection<AuthToken> _tokenCollection;

        static MongoTokenStorage()
        {
            BsonClassMap.RegisterClassMap<AuthToken>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.PlayerId);
            });
        }

        public MongoTokenStorage(MongoUrl url)
        {
            var con = url.GetServer();
            var db = con.GetDatabase(url.DatabaseName);
            _tokenCollection = db.GetCollection<AuthToken>("auth_tokens");
            _tokenCollection.Indexes.CreateOne(Builders<AuthToken>.IndexKeys.Ascending(_ => _.TokenId));
        }

        public AuthToken GetByTokenId(Guid tokenId)
        {
            var filter = Builders<AuthToken>.Filter.Eq(_ => _.TokenId, tokenId);
            var cur = _tokenCollection.FindSync(filter);
            if (!cur.MoveNext())
                return null;
            return cur.Current?.FirstOrDefault();
        }

        public AuthToken GetByPlayerId(Guid playerId)
        {
            var filter = Builders<AuthToken>.Filter.Eq(_ => _.PlayerId, playerId);
            var cur = _tokenCollection.FindSync(filter);
            if (!cur.MoveNext())
                return null;
            return cur.Current?.FirstOrDefault();
        }

        public AuthToken CreateToken(Guid playerId, TimeSpan ttl, string clientAddress)
        {
            var filter = Builders<AuthToken>.Filter.Eq(_ => _.PlayerId, playerId);
            var update = Builders<AuthToken>.Update.Combine(
                Builders<AuthToken>.Update.Set(_ => _.AuthTime, DateTime.UtcNow),
                Builders<AuthToken>.Update.Set(_ => _.ExpiryTime, DateTime.UtcNow + ttl),
                Builders<AuthToken>.Update.Set(_ => _.TokenId, Guid.NewGuid()),
                Builders<AuthToken>.Update.Set(_ => _.Address, clientAddress)
            );
            var opts = new FindOneAndUpdateOptions<AuthToken>();
            opts.IsUpsert = true;
            opts.ReturnDocument = ReturnDocument.After;
            return _tokenCollection.FindOneAndUpdate(filter, update, opts);
        }
    }
}
