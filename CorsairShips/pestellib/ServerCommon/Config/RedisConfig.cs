
using System;
using System.Threading;
using log4net;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace ServerLib.Config
{
    /// <summary>
    /// Читает конфиг из JSON-строки в редисе.
    /// Перечитывает конфиг по заданному интервалу.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RedisConfig<T> where T: new()
    {
        public T Config { get; private set; }
        public TimeSpan ReloadPeriod
        {
            get { return _reloadPeriod;}
            set { SetReload(value);}
        }

        public RedisConfig(string redisKey, TimeSpan reloadPeriod)
        {
            _redisKey = redisKey;

            var dict = default(RedisValue);

            for (var i = 0; i <= 10; i++)
            {
                try
                {
                    dict = RedisUtils.Cache.StringGet(_redisKey);
                    break;
                }
                catch (Exception e)
                {
                    Log.Error("Redis exception: " + e);
                }

                if (i == 10)
                {
                    Log.Error("Can't initialize RedisConfig!");
                    return;
                }

                Thread.Sleep(100);
            }

            SetReload(reloadPeriod);
            Config = dict.HasValue ? JsonConvert.DeserializeObject<T>(dict) : new T();

            Log.Info("Config content: " + dict);
        }

        private void SetReload(TimeSpan reloadPeriod)
        {
            _reloadPeriod = reloadPeriod;
            if (reloadPeriod == TimeSpan.Zero)
            {
                if (_reloadTimer != null)
                {
                    _reloadTimer.Dispose();
                    _reloadTimer = null;
                }
                return;
            }
            if (_reloadTimer != null)
                _reloadTimer.Change(reloadPeriod, reloadPeriod);
            else
                _reloadTimer = new Timer(ReloadEvent, null, reloadPeriod, reloadPeriod);
        }

        private void ReloadEvent(object state)
        {
            Reload();
        }

        public void Reload()
        {
            try
            {
                var dict = RedisUtils.Cache.StringGet(_redisKey);
                Config = JsonConvert.DeserializeObject<T>(dict);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private string _redisKey;
        private TimeSpan _reloadPeriod;
        private Timer _reloadTimer;
        private static readonly ILog Log = LogManager.GetLogger(typeof(RedisConfig<T>));
    }
}
