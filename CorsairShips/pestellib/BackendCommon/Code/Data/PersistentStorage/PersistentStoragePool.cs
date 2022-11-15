using System.Collections.Concurrent;
using log4net;
using MongoDB.Driver;

namespace BackendCommon.Code.Data.PersistentStorage
{
    public class PersistentStoragePool
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PersistentStoragePool));
        private readonly object _sync = new object();
        private readonly ConcurrentDictionary<int, IPersistentStorage> _persistentStorages = new ConcurrentDictionary<int, IPersistentStorage>();

        public IPersistentStorage GetStorage(string connectionString, string dataCollectionName)
        {
            var dicKey = (connectionString + dataCollectionName).GetHashCode();
            if (_persistentStorages.TryGetValue(dicKey, out var result))
                return result;

            lock (_sync)
            {
                if (_persistentStorages.TryGetValue(dicKey, out result))
                    return result;

                try
                {
                    var mongoUrl = new MongoUrl(connectionString);
                    result = new MongoStorage(mongoUrl, dataCollectionName);
                }
                catch (MongoConfigurationException)
                {
                }

                if (result != null)
                    _persistentStorages[dicKey] = result;
            }
            return result;
        }
    }
}
