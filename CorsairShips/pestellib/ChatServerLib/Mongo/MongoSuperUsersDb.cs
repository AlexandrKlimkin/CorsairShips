using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PestelLib.ChatCommon;
using PestelLib.ServerCommon.Db;
using PestelLib.ServerShared;
using MongoException = MongoDB.Driver.MongoException;

namespace ChatServer.Mongo
{
#pragma warning disable 649
    [BsonIgnoreExtraElements]
    class SuperUserInfoMongo
    {
        [BsonId]
        public string AuthData;
        public DateTime CreateTime;
    }
#pragma warning restore 649

    internal class MongoSuperUsersDb : ISuperUsersDb
    {
        public MongoSuperUsersDb(MongoUrl url)
        {
            var c = url.GetServer();
            var db = c.GetDatabase(url.DatabaseName);
            m_superByAuthData = db.GetCollection<SuperUserInfoMongo>("super_by_auth");
        }

        public bool IsSuper(byte[] authData)
        {
            var authStr = StringUtils.ArrayToHex(authData);
            var filter = Builders<SuperUserInfoMongo>.Filter.Eq(_ => _.AuthData, authStr);
            return m_superByAuthData.Count(filter) > 0;
        }

        public bool AddSuper(byte[] authData)
        {
            var doc = new SuperUserInfoMongo()
            {
                AuthData = StringUtils.ArrayToHex(authData),
                CreateTime = DateTime.UtcNow
            };
            try
            {
                m_superByAuthData.InsertOne(doc);
            }
            catch (MongoWriteException e)
            {
                return false;
            }

            return true;
        }

        public bool RemoveSuper(byte[] authData)
        {
            var authStr = StringUtils.ArrayToHex(authData);
            var filter = Builders<SuperUserInfoMongo>.Filter.Eq(_ => _.AuthData, authStr);
            var r = m_superByAuthData.DeleteOne(filter);
            return r.DeletedCount > 0;
        }

        private IMongoCollection<SuperUserInfoMongo> m_superByAuthData;
    }
}
