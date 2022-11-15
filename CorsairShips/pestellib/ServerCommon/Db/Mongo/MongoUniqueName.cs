using System;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace PestelLib.ServerCommon.Db.Mongo
{
    public class MongoNameToId
    {
        [BsonId]
        public string Name;
        public Guid Id;
    }
    public class MongoUniqueName
    {
        public static MongoUniqueName Create(MongoDb db, string name, bool caseInsensitive) => new MongoUniqueName(db.GetCollection<MongoNameToId>(name), caseInsensitive);

        public MongoUniqueName(IMongoCollection<MongoNameToId> collection, bool caseInsensitive)
        {
            _caseInsensitive = caseInsensitive;
            _collection = collection;
        }

        public Task<bool> BindUniqueName(string name) => BindUniqueName(name, Guid.NewGuid());

        public async Task<bool> BindUniqueName(string name, Guid bindId)
        {
            if (_caseInsensitive)
                name = name.ToLower();
            var filter = Builders<MongoNameToId>.Filter.Eq(_ => _.Name, name);
            var update = Builders<MongoNameToId>.Update.SetOnInsert(_ => _.Id, bindId); // только если нет такого имени создаем биндинг к id
            var opts = new FindOneAndUpdateOptions<MongoNameToId>();
            opts.IsUpsert = true;
            opts.ReturnDocument = ReturnDocument.After;

            var checkClanName = await _collection.FindOneAndUpdateAsync(filter, update, opts);
            if (checkClanName.Id == bindId)
                return true;
            return false;
        }

        public async Task<Guid> GetIdByName(string name)
        {
            if (_caseInsensitive)
                name = name.ToLower();
            var filter = Builders<MongoNameToId>.Filter.Eq(_ => _.Name, name);
            var r = (await _collection.FindAsync(filter)).SingleOrDefault();
            if(r == null)
                return Guid.Empty;
            return r.Id;
        }

        public async Task<bool> RemoveBinding(string name, Guid id)
        {
            if (_caseInsensitive)
                name = name.ToLower();
            var filter = Builders<MongoNameToId>.Filter.And(
                Builders<MongoNameToId>.Filter.Eq(_ => _.Id, id),
                Builders<MongoNameToId>.Filter.Eq(_ => _.Name, name));

            var r = await _collection.FindOneAndDeleteAsync(filter);
            return r != null;
        }

        private bool _caseInsensitive;
        private IMongoCollection<MongoNameToId> _collection;
    }
}
