using System;
using MongoDB.Driver;
using ServerLib;
using log4net;

namespace PestelLib.ServerCommon.Db.Auth
{
    public static class TokenStorageFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TokenStorageFactory));
        private static object _sync = new object();
        private static volatile MongoTokenStorage _storage;

        public static ITokenStore GetStorage(string connectionString)
        {
            if (_storage != null)
                return _storage;
            try
            {
                lock (_sync)
                {
                    if (_storage != null)
                        return _storage;
                    var mongoUrl = new MongoUrl(connectionString);
                    _storage = new MongoTokenStorage(mongoUrl);
                }

            }
            catch (Exception e)
            {
                Log.Error($"Parse mongo url '{connectionString}' failed.", e);
                return null;
            }

            return _storage;
        }

        public static ITokenStore GetStorage(AppSettings settings)
        {
            if (_storage != null)
                return _storage;
            if (string.IsNullOrEmpty(settings.PersistentStorageSettings
                .StorageConnectionString))
                return null;
            lock (_sync)
            {
                if (_storage != null)
                    return _storage;
                try
                {
                    _storage = new MongoTokenStorage(new MongoUrl(settings.PersistentStorageSettings
                        .StorageConnectionString));
                }
                catch(Exception e)
                {
                    Log.Warn($"Initialize token storage failed. connString={settings.PersistentStorageSettings.StorageConnectionString}.", e);
                }
            }

            return _storage;
        }

        public static ITokenStoreWriter GetStoreWriter(AppSettings settings)
        {
            if (_storage == null)
                GetStorage(settings);
            return _storage;
        }
    }
}
