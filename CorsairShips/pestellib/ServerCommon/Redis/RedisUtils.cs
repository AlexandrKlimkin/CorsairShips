using System;
using System.Collections.Generic;
using StackExchange.Redis;
using System.Linq;
using System.Threading.Tasks;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;

namespace ServerLib
{
    public static class RedisUtils
    {
        private static ConfigurationOptions _connectionOptions;
        private static Lazy<RedLockFactory> _redlockFactory;

        static RedisUtils()
        {
            _redlockFactory= new Lazy<RedLockFactory>(() => RedLockFactory.Create(new List<RedLockMultiplexer>() {lazyConnection.Value}));
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            //return ConnectionMultiplexer.Connect("localhost,abortConnect=false,ssl=false");
            var result = ConnectionMultiplexer.Connect(AppSettings.Default.RedisConnectionString);
            _connectionOptions = ConfigurationOptions.Parse(result.Configuration);
#if DEPLOY_IOS
            _connectionOptions.DefaultDatabase = 1;
#else
            _connectionOptions.DefaultDatabase = 0;
#endif
            return result;
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        public static IDatabase Cache
        {
#if DEPLOY_IOS
            get { return Connection.GetDatabase(1); }
#else
            get { return Connection.GetDatabase(0); }
#endif
        }

        public static TimeSpan DefaultExpirationTime = new TimeSpan(3000, 0, 0, 0);

        public static TimeSpan IdleTime(TimeSpan expirationTimespan)
        {
            return DefaultExpirationTime - expirationTimespan;
        }

        public static string[] GetKeys(string pattern)
        {
            var s = Connection.GetServer(_connectionOptions.EndPoints.First());
            var dbIndex = _connectionOptions.DefaultDatabase ?? 0;
            return s.Keys(pattern: pattern, database: dbIndex)
                .Select(k => k.ToString())
                .ToArray();
        }

        public static TimeSpan? KeyIdleTime(this IDatabase cache, RedisKey key)
        {
            var expirationTimespan = cache.KeyTimeToLive(key);
            if (expirationTimespan.HasValue)
            {
                return IdleTime(expirationTimespan.Value);
            }
            return null;
        }

        /// <summary>
        /// usage https://github.com/samcook/RedLock.net
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="ttl"></param>
        /// <returns></returns>
        public static IRedLock LockResource(string resourceName, TimeSpan ttl)
        {
            return
                _redlockFactory.Value.CreateLock(resourceName, ttl);
        }

        /// <summary>
        /// usage https://github.com/samcook/RedLock.net
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="ttl"></param>
        /// <returns></returns>
        public static Task<IRedLock> LockResourceAsync(string resourceName, TimeSpan ttl)
        {
            return
                _redlockFactory.Value.CreateLockAsync(resourceName, ttl);
        }
    }
}