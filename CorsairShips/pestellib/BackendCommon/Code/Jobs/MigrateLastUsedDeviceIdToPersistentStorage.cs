using System;
using System.Threading.Tasks;
using BackendCommon.Code.Data;
using log4net;
using Quartz;
using ServerLib;
using StackExchange.Redis;

namespace BackendCommon.Code.Jobs
{
    [DisallowConcurrentExecution]
    public class MigrateLastUsedDeviceIdToPersistentStorage : IJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MigrateLastUsedDeviceIdToPersistentStorage));
        private static readonly UserStorage _storage = new UserStorage();

        public Task Execute(IJobExecutionContext context)
        {
            var scan = RedisUtils.Cache.HashScan(UserStorage.LastUsedDeviceIdKey, default(RedisValue), 100);

            var counter = 0;

            foreach (var hashEntry in scan)
            {
                try
                {
                    Guid playerId;
                    if (Guid.TryParse((string) hashEntry.Name, out playerId))
                    {
                        _storage.SetLastUsedDeviceId(playerId, hashEntry.Value);
                        RedisUtils.Cache.HashDelete(UserStorage.LastUsedDeviceIdKey, hashEntry.Name);
                    }
                }
                catch (Exception e)
                {
                    log.Error("Can't move last used device id to persistent storage: " + e.Message + " " + e.StackTrace);
                }

                if (++counter % 1000 == 0)
                {
                    log.InfoFormat("Moved {0} last used device ids to persistent database", counter);
                }
            }

            if (counter > 0)
            {
                log.Info("All last used device ids were moved to persistent storage");
            }
            return Task.CompletedTask;
        }
    }
}