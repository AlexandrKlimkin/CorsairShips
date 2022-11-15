using System;
using System.Threading.Tasks;
using log4net;
using PestelLib.ServerCommon.Db;
using Quartz;
using ServerLib;

namespace BackendCommon.Code.Jobs
{
    public class CleanOldMatchInfosJob : IJob
    {
        private static ILog _log = LogManager.GetLogger(typeof(CleanOldMatchInfosJob));
        public Task Execute(IJobExecutionContext context)
        {
            var dt = DateTime.UtcNow - AppSettings.Default.MatchInfoTTL;
            var matchInfoApi = ApiFactory.GetMatchInfoApi();
            var cleaned = matchInfoApi.CleanOldMatchInfos(dt);
            _log.Info($"{cleaned} match infos older than {dt} removed");
            return Task.CompletedTask;
        }
    }
}