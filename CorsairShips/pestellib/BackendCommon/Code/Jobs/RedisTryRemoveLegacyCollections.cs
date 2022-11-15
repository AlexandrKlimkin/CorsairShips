using System.Threading.Tasks;
using log4net;
using Quartz;
using ServerLib;

namespace BackendCommon.Code.Jobs
{
    [DisallowConcurrentExecution]
    public class RedisTryRemoveLegacyCollections : IJob
    {
        private const string SessionNumberKey = "SessionNumberKey";

        private static readonly ILog log = LogManager.GetLogger(typeof(RedisTryRemoveLegacyCollections));

        public Task Execute(IJobExecutionContext context)
        {
            RedisUtils.Cache.KeyDelete(SessionNumberKey);
            return Task.CompletedTask;
        }
    }
}