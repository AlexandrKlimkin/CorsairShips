using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PestelLib.ServerCommon.Db;
using PestelLib.ServerCommon.Utils;

namespace Backend.Code.Utils
{
    class IpTableItem
    {
        [BsonId]
        public Guid PlayerId;
        public string Ip;
    }

    public class MongoPlayerIpTable : IPlayerIpResolver
    {
        private const string CollName = "PlayerIpTable";

        public MongoPlayerIpTable(MongoUrl mongoUrl)
        {
            var server = mongoUrl.GetServer();
            var db = server.GetDatabase(mongoUrl.DatabaseName);
            _ipTable = db.GetCollection<IpTableItem>(CollName);
        }

        public void Set(Guid playerId, string ip)
        {
            var filter = Builders<IpTableItem>.Filter.Eq(_ => _.PlayerId, playerId);
            var update = Builders<IpTableItem>.Update.Set(_ => _.Ip, ip);
            _ipTable.FindOneAndUpdate(filter, update);
        }

        public string GetPlayerIp(Guid playerId)
        {
            var filter = Builders<IpTableItem>.Filter.Eq(_ => _.PlayerId, playerId);
            var r = _ipTable.FindSync(filter).SingleOrDefault();
            if (r == null)
                return null;
            return r.Ip;
        }

        private IMongoCollection<IpTableItem> _ipTable;
    }
}