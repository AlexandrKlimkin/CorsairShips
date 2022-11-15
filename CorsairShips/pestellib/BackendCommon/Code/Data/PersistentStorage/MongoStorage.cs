using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PestelLib.ServerCommon.Extensions;
using PestelLib.ServerCommon.Db;
using log4net;

namespace BackendCommon.Code.Data.PersistentStorage
{
    [BsonIgnoreExtraElements]
    class RawData
    {
        [BsonId]
        public string Id;
        [BsonElement("data")]
        public byte[] Data;
        [BsonElement("time")]
        public DateTime Time;
        [BsonElement("size")]
        public int Size;
    }

    public class MongoStorage : IPersistentStorage
    {
        private static readonly BsonDocument _empty = new BsonDocument();
        private readonly IMongoCollection<RawData> _rawDataCollection;

        public MongoStorage(MongoUrl connectionString, string dataCollection)
        {
            var c = connectionString.GetServer();
            var db = c.GetDatabase(connectionString.DatabaseName);
            _rawDataCollection = db.GetCollection<RawData>(dataCollection);

            _rawDataCollection.Indexes.CreateOneAsync(Builders<RawData>.IndexKeys.Ascending(_ => _.Time)).ReportOnFail();
        }

        private BsonDocument CreateUpdateDocument(string field, object value)
        {
            return new BsonDocument { { "$set", new BsonDocument { { field, BsonValue.Create(value) } } } };
        }

        public bool SaveRawData(string key, byte[] data)
        {
            var doc = new RawData() {Id = key, Data = data, Time = DateTime.UtcNow, Size = data.Length};
            var filter = Builders<RawData>.Filter.Eq(_ => _.Id, key);
            var opts = new UpdateOptions() {IsUpsert = true};
            var result = _rawDataCollection.ReplaceOne(filter, doc, opts);
            return true;
        }

        public byte[] LoadRawData(string key)
        {
            var result = _rawDataCollection
                .FindSync(rd => rd.Id == key)
                .FirstOrDefault();
            return result?.Data;
        }

        public bool Remove(string key)
        {
            var result = _rawDataCollection.DeleteOne(rd => rd.Id == key);
            return result.DeletedCount > 0;
        }

        public IEnumerable<string> GetKeys(int skip, int count, DateTime lastLogin)
        {
            var o = new FindOptions<RawData, RawData>
            {
                Projection = new ProjectionDefinitionBuilder<RawData>().Exclude(rd => rd.Data),
                Skip = skip,
                Limit = count
            };
            FilterDefinition<RawData> filter;
            if (lastLogin != default(DateTime))
                filter = Builders<RawData>.Filter.Gte(_ => _.Time, lastLogin);
            else
                filter = _empty;
            using (var rawResult = _rawDataCollection.FindSync(filter, o))
            {
                while (rawResult.MoveNext())
                {
                    foreach (var rawData in rawResult.Current)
                    {
                        yield return rawData.Id;
                    }
                }
            }
        }

        public bool Exists(string key)
        {
            try
            {
                return _rawDataCollection.FindSync(rd => rd.Id == key).Any();
            }
            catch (TimeoutException)
            {
                throw new UserStorageException(UserStorageException.ErrorEnum.STORAGE_NOT_AVAILABLE, null);
            }
        }

        public long Count()
        {
            return _rawDataCollection.Count(_empty);
        }

        public long CountBySaveTime(DateTime dt)
        {
            return _rawDataCollection.Count(Builders<RawData>.Filter.Gt(_ => _.Time, dt));
        }

        public bool HasKey(string key)
        {
            var o = new FindOptions<RawData, RawData>
            {
                Projection = new ProjectionDefinitionBuilder<RawData>().Exclude(rd => rd.Data)
            };
            return _rawDataCollection
                .FindSync(new BsonDocument(new BsonElement("_id", key)), o)
                .Any();
        }

        public string[] GetKeys(TimeSpan maxAge, int minSize, int count)
        {
            var result = new List<string>(count);
            var timeFilter = DateTime.UtcNow - maxAge;
            var filter = Builders<RawData>.Filter.And(
                Builders<RawData>.Filter.Gte(_ => _.Time, timeFilter),
                Builders<RawData>.Filter.Gte(_ => _.Size, minSize));
            var opts = new FindOptions<RawData>();
            opts.Projection = Builders<RawData>.Projection.Include(_ => _.Id);

            var cur = _rawDataCollection.FindSync(filter, opts);
            while (cur.MoveNext())
            {
                result.AddRange(Enumerable.Select<RawData, string>(cur.Current, _ => _.Id));
                if(result.Count >= count)
                    break;
            }
            return result.Take(count).ToArray();
        }

        public DateTime GetTime(string key)
        {
            var filter = Builders<RawData>.Filter.Eq(_ => _.Id, key);
            var opts = new FindOptions<RawData>();
            opts.Projection = Builders<RawData>.Projection.Include(_ => _.Time);
            opts.Limit = 1;

            var cur = _rawDataCollection.FindSync(filter, opts);
            if(!cur.MoveNext())
                return DateTime.MinValue;

            var item = Enumerable.FirstOrDefault<RawData>(cur.Current);
            if(item == null)
                return DateTime.MinValue;

            return item.Time;
        }

        public async Task<bool> SaveRawDataAsync(string key, byte[] data)
        {
            var doc = new RawData() { Id = key, Data = data };
            var filter = Builders<RawData>.Filter.Eq(_ => _.Id, key);
            var opts = new UpdateOptions() { IsUpsert = true };
            await _rawDataCollection.ReplaceOneAsync(filter, doc, opts);
            return true;
        }

        public async Task<byte[]> LoadRawDataAsync(string key)
        {
            var result = (await _rawDataCollection
                .FindAsync(rd => rd.Id == key))
                .FirstOrDefault();
            return result?.Data;
        }

        public async Task<bool> RemoveAsync(string key)
        {
            var result = await _rawDataCollection.DeleteOneAsync(rd => rd.Id == key);
            return result.DeletedCount > 0;
        }
    }
}
