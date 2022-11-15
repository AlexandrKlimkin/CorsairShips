using System;
using System.Threading.Tasks;
using BackendCommon.Code.Data;
using log4net;
using Quartz;
using ServerLib;

namespace BackendCommon.Code.Jobs
{
    [DisallowConcurrentExecution]
    public class MigrateFacebookIdToPersistantStorage : IJob
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MigrateFacebookIdToPersistantStorage));
        private static readonly UserStorage _storage = new UserStorage();

        public Task Execute(IJobExecutionContext context)
        {
            var facebookIds = RedisUtils.Cache.HashScan(UserStorage.FacebookIdsKeys);

            var counter = 0;

            foreach (var hashEntry in facebookIds)
            {
                try
                {
                    _storage.SetFacebookId(Guid.Parse((string) hashEntry.Value), hashEntry.Name);
                    RedisUtils.Cache.HashDelete(UserStorage.FacebookIdsKeys, hashEntry.Name);
                }
                catch (Exception e)
                {
                    log.Error("Can't move facebook id to persistent storage: " + e.Message + " " + e.StackTrace);
                }

                if (++counter % 1000 == 0)
                {
                    log.InfoFormat("Moved {0} facebook ids to persistent database", counter);
                }
            }

            if (counter > 0)
            {
                log.Info("All facebook ids moved to persistent storage");
            }
            return Task.CompletedTask;
        }
    }
}