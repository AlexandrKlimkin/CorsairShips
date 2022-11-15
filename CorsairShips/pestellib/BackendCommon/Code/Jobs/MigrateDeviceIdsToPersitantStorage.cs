using System;
using System.Threading.Tasks;
using BackendCommon.Code.Data;
using log4net;
using Quartz;
using ServerLib;

namespace BackendCommon.Code.Jobs
{
    [DisallowConcurrentExecution]
    public class MigrateDeviceIdsToPersitantStorage : IJob 
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MigrateDeviceIdsToPersitantStorage));
        private static readonly UserStorage _storage = new UserStorage();

        public Task Execute(IJobExecutionContext context)
        {
            var deviceIds = RedisUtils.Cache.HashScan(UserStorage.DeviceIdToUserId);

            var counter = 0;

            foreach (var hashEntry in deviceIds)
            {
                try
                {
                    _storage.SaveDeviceId(hashEntry.Name, Guid.Parse((string) hashEntry.Value));
                    RedisUtils.Cache.HashDelete(UserStorage.DeviceIdToUserId, hashEntry.Name);
                }
                catch (Exception e)
                {
                    log.Error("Can't move device id to persistent storage: " + e.Message + " " + e.StackTrace);
                }

                if (++counter % 1000 == 0)
                {
                    log.InfoFormat("Moved {0} device ids to persistent database", counter);
                }
            }

            if (counter > 0)
            {
                log.Info("All device ids moved to persistent storage");
            }
            return Task.CompletedTask;
        }
    }
}