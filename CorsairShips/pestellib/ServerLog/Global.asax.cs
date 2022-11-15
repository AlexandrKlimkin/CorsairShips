using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PestelLib.ServerLogProtocol;
using ServerLog.MongoDocs;
using PestelLib.ServerCommon.Db;

namespace ServerLog
{
    public class Global : System.Web.HttpApplication
    {
        public static IMongoCollection<LogMessage> MongoLogCollection;

        public static IMongoCollection<LogErrorCounter> MongoLogErrorCounterCollection;
        private readonly long CapSize = 100L * 1024 * 1024 * 1024; // 100Gb

        protected void Application_Start(object sender, EventArgs e)
        {
            BsonClassMap.RegisterClassMap<LogErrorCounter>(cm =>
            {
                cm.AutoMap();
                cm.MapIdField(_ => _.Message);
            });

            BsonClassMap.RegisterClassMap<LogMessage>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(c => c.Time).SetSerializer(new DateTimeSerializer(DateTimeKind.Utc));
            });

            var connectionString = new MongoUrl("mongodb://localhost:27017");
            var client = connectionString.GetServer();
            var mongo = client.GetDatabase("logs");

            // if sizeInbytes is lower than size of existing collection when old DATA will be LOST
            MongoLogCollection = mongo.Db.GetCappedCollection<LogMessage>("messages", CapSize, null, true);
            MongoLogCollection.Indexes.CreateOneAsync(Builders<LogMessage>.IndexKeys.Ascending(_ => _.PlayerId));
            MongoLogCollection.Indexes.CreateOneAsync(Builders<LogMessage>.IndexKeys.Ascending(_ => _.BuildVersion));
            MongoLogCollection.Indexes.CreateOneAsync(Builders<LogMessage>.IndexKeys.Ascending(_ => _.Game));
            MongoLogCollection.Indexes.CreateOneAsync(Builders<LogMessage>.IndexKeys.Ascending(_ => _.Message));
            MongoLogCollection.Indexes.CreateOneAsync(Builders<LogMessage>.IndexKeys.Ascending(_ => _.Platform));
            MongoLogCollection.Indexes.CreateOneAsync(Builders<LogMessage>.IndexKeys.Ascending(_ => _.Tag));
            MongoLogCollection.Indexes.CreateOneAsync(Builders<LogMessage>.IndexKeys.Ascending(_ => _.Type));
            MongoLogCollection.Indexes.CreateOneAsync(Builders<LogMessage>.IndexKeys.Ascending(_ => _.PlayerIdString));

            MongoLogErrorCounterCollection = mongo.GetCollection<LogErrorCounter>("errors");
            MongoLogErrorCounterCollection.Indexes.CreateOneAsync(Builders<LogErrorCounter>.IndexKeys.Combine(
                Builders<LogErrorCounter>.IndexKeys.Ascending(_ => _.Message),
                Builders<LogErrorCounter>.IndexKeys.Ascending(_ => _.GameName))
            );
        }
    }
}