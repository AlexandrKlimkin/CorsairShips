using System.Collections.Concurrent;
using MongoDB.Driver;

namespace PestelLib.ServerCommon.Db
{
    public class MongoDb
    {
        public string Name { get; private set; }
        public IMongoDatabase Db { get; private set; }

        public MongoDb(string name, IMongoDatabase db)
        {
            Name = name;
            Db = db;
        }

        public IMongoCollection<T> GetCappedCollection<T>(string name, long? maxDocs = null, long? maxSize = null)
        {
            return Collections.GetOrAdd(name, (s) =>
            {
                if(!Db.CollectionExists(s))
                    Db.CreateCollection(s, new CreateCollectionOptions()
                    {
                        Capped = true,
                        MaxDocuments = maxDocs,
                        MaxSize = maxSize
                    });
                return Db.GetCollection<T>(name);
            }) as IMongoCollection<T>;
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return Collections.GetOrAdd(name, (s) => Db.GetCollection<T>(name)) as IMongoCollection<T>;
        }
        
        private ConcurrentDictionary<string, object> Collections = new ConcurrentDictionary<string, object>();
    }

    public class MongoServer
    {
        public string ConnectionString { get; private set; }
        public MongoClient Client { get; private set; }
        public MongoServer(string connectionString, string initialDb)
        {
            ConnectionString = connectionString;
            Client = new MongoClient(connectionString);
            if (!string.IsNullOrEmpty(initialDb))
            {
                _ = GetDatabase(initialDb);
            }
        }

        public MongoDb GetDatabase(string name)
        {
            return Databases.GetOrAdd(name, _ => new MongoDb(_, Client.GetDatabase(_)));
        }

        private ConcurrentDictionary<string, MongoDb> Databases = new ConcurrentDictionary<string, MongoDb>();
    }

    public static class MongoConnection
    {
        const string scheme = "mongodb://";
        private static ConcurrentDictionary<string, MongoServer> _servers = new ConcurrentDictionary<string, MongoServer>();
        public static MongoServer GetServer(this MongoUrl url)
        {
            string dbName = string.Empty;
            var s = url.ToString();
            var dbNameStart = s.IndexOf('/', scheme.Length);
            var dbNameEnd = s.IndexOf('?', scheme.Length);
            if (dbNameStart > -1)
            {
                ++dbNameStart;
                if (dbNameEnd < 0)
                {
                    dbName = s.Substring(dbNameStart);
                    s = s.Substring(0, dbNameStart);
                }
                else
                {
                    dbName = s.Substring(dbNameStart, dbNameEnd - dbNameStart);
                    s = s.Substring(0, dbNameStart) + s.Substring(dbNameEnd);
                }
                url = MongoUrl.Create(s);
            }
            return _servers.GetOrAdd(s, _ => new MongoServer(s, dbName));
        }

        
    }
}
