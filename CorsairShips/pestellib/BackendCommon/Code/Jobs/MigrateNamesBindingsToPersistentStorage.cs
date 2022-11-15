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
    public class MigrateNamesBindingsToPersistentStorage : IJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MigrateNamesBindingsToPersistentStorage));
        private static readonly UserStorage _storage = new UserStorage();

        public Task Execute(IJobExecutionContext context)
        {
            var bindings = RedisUtils.Cache.HashScan(UserStorage.NameToId, default(RedisValue), 100);

            var counter = 0;

            foreach (var hashEntry in bindings)
            {
                try
                {
                    _storage.SaveNameToUidBindings(hashEntry.Name, hashEntry.Value);
                    RedisUtils.Cache.HashDelete(UserStorage.NameToId, hashEntry.Name);
                }
                catch (Exception e)
                {
                    log.Error("Can't move name-to-id binding to persistent storage: " + e.Message + " " + e.StackTrace);
                }

                if (++counter % 1000 == 0)
                {
                    log.InfoFormat("Moved {0} name bindings to persistent database", counter);
                }
            }

            if (counter > 0)
            {
                log.Info("all name bindings was moved to persistent storage");
            }
            return Task.CompletedTask;
        }
    }
}