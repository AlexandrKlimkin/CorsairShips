using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PestelLib.ServerCommon.Db;

namespace FriendsServer.Db.Mongo
{
#pragma warning disable 649
    class NamedCounterItem
    {
        [BsonId]
        public string Name;
        public long Count;
    }
#pragma warning restore 649

    class NamedCounter
    {
        private readonly string _counterName;
        private IMongoCollection<NamedCounterItem> _counters;
        private const string CollectionName = "named_counters";
        private readonly FilterDefinition<NamedCounterItem> _filter;
        private readonly UpdateDefinition<NamedCounterItem> _update;

        private NamedCounter(string counterName)
        {
            _counterName = counterName;
            _filter = Builders<NamedCounterItem>.Filter.Eq(_ => _.Name, _counterName);
            _update = Builders<NamedCounterItem>.Update.Inc(_ => _.Count, 1);
        }

        public NamedCounter(MongoDb db, string counterName)
        :this(counterName)
        {
            _counters = db.GetCollection<NamedCounterItem>(CollectionName);
        }

        public NamedCounter(string connectionString, string counterName)
            : this(counterName)
        {
            var url = new MongoUrl(connectionString);
            var c = url.GetServer();
            var db = c.GetDatabase(url.DatabaseName);
            _counters = db.GetCollection<NamedCounterItem>(CollectionName);
        }

        public async Task<long> NextId()
        {
            var opts = new FindOneAndUpdateOptions<NamedCounterItem>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };
            var r = await _counters.FindOneAndUpdateAsync(_filter, _update, opts);

            return r.Count;
        }
    }
}
