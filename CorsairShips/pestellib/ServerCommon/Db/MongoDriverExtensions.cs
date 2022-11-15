using System;
using log4net;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PestelLib.ServerCommon.Db
{
    public static class MongoDriverExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MongoDriverExtensions));

        /// <summary>
        /// Converts existing collection to capped
        /// Can be called on already capped collection to change cap
        /// </summary>
        /// <param name="db"></param>
        /// <param name="collectionName"></param>
        /// <param name="sizeInBytes"></param>
        /// <returns></returns>
        public static bool ConvertToCapped(this IMongoDatabase db, string collectionName, long sizeInBytes)
        {
            var command = new BsonDocumentCommand<BsonDocument>(new BsonDocument { { "convertToCapped", collectionName }, { "size", sizeInBytes } });
            var result = db.RunCommand(command).ToHashtable();
            return result.ContainsKey("ok") && Math.Abs((double)result["ok"] - 1) < 0.001;
        }

        /// <summary>
        /// Checks if specified collection capped
        /// </summary>
        /// <param name="db"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public static bool IsCollectionCapped(this IMongoDatabase db, string collectionName)
        {
            var command = new BsonDocumentCommand<BsonDocument>(new BsonDocument { { "collstats", collectionName } });
            var stats = db.RunCommand(command);
            var h = stats.ToHashtable();
            if (h["capped"] is bool capped)
            {
                return capped;
            }

            return false;
        }

        /// <summary>
        /// Checks if specified collection exists
        /// </summary>
        /// <param name="db"></param>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public static bool CollectionExists(this IMongoDatabase db, string collectionName)
        {
            if (collectionName == null)
                return false;
            var cur = db.ListCollections();
            while (cur.MoveNext())
            {
                foreach (var coll in cur.Current)
                {
                    var h = coll.ToHashtable();
                    if (h["name"].ToString() == collectionName)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets or creates capped collection.
        /// If collection already exists converts it to capped (if sizeInbytes is lower than size of existing collection when old DATA will be LOST).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="name">collection name</param>
        /// <param name="sizeInbytes">max size of collection in bytes</param>
        /// <param name="maxDocuments">max size of collection in documents</param>
        /// <param name="convert">convert existing collection to capped</param>
        /// <returns></returns>
        public static IMongoCollection<T> GetCappedCollection<T>(this IMongoDatabase db, string name, long? sizeInbytes, long? maxDocuments, bool convert=false)
        {
            if (!sizeInbytes.HasValue && !maxDocuments.HasValue)
            {
                Log.Error($"Arguments sizeInbytes=null and maxDocuments=null suitable for not capped collection. User GetCollection<T>({name}) instead");
                return db.GetCollection<T>(name);
            }

            if (db.CollectionExists(name))
            {
                if (convert)
                {
                    if (maxDocuments.HasValue)
                    {
                        Log.Error(
                            $"Cant convert existing collection {name} with maxDocuments argument set. Call with maxDocuments=null");
                        return db.GetCollection<T>(name);
                    }
                    db.ConvertToCapped(name, sizeInbytes.Value);
                }
                return db.GetCollection<T>(name);
            }
            return db.CreateCappedCollection<T>(name, sizeInbytes, maxDocuments);
        }

        private static IMongoCollection<T> CreateCappedCollection<T>(this IMongoDatabase db, string name, long? sizeInbytes, long? maxDocuments)
        {
            var settings = new CreateCollectionOptions<T>();
            settings.Capped = true;
            settings.MaxSize = sizeInbytes;
            settings.MaxDocuments = maxDocuments;
            db.CreateCollection(name, settings);
            return db.GetCollection<T>(name);
        }
    }
}
