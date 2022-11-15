using System;
using log4net;
using MongoDB.Driver;
using PestelLib.ChatServer.Mongo;

namespace PestelLib.ChatServer
{
    class BanStorageFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BanStorageFactory));
        private ChatServerConfig _config;
        private string _cs;
        private IBanStorage _storage;
        private IBanRequestStorage _banRequestStorage;

        public BanStorageFactory(ChatServerConfig config)
        {
            _config = config;
        }

        public IBanStorage Get()
        {
            if (_storage != null)
                return _storage;
            Create();
            Log.Info($"BanStorage type '{_storage.GetType().Name}'");
            return _storage;
        }

        public IBanRequestStorage GetBanRequestStorage()
        {
            if (_banRequestStorage != null)
                return _banRequestStorage;
            Create();
            return _banRequestStorage;
        }

        private void Create()
        {
            _cs = _config.MongoConnectionString;

            try
            {
                if (string.IsNullOrEmpty(_config.MongoConnectionString))
                    throw null;
                var mongo = new BanStorageMongo(MongoUrl.Create(_config.MongoConnectionString));
                _storage = mongo;
                _banRequestStorage = mongo;
            }
            catch (Exception e)
            {
                Log.Error(e);
                var stub = new BanStorageStub();
                _storage = stub;
                _banRequestStorage = stub;
            }
        }
    }
}
