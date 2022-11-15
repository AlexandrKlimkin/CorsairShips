using System.Threading.Tasks;
using BackendCommon.Code.GlobalConflict;
using log4net;
using PestelLib.ServerCommon.Extensions;
using Quartz;
using UnityDI;

namespace BackendCommon.Code.Jobs
{
    public class GlobalConflictJob : IJob
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GlobalConflictJob));
#pragma warning disable 0649
        [Dependency]
        private GlobalConflictPrivateApi _api;
#pragma warning restore 0649

        public Task Execute(IJobExecutionContext context)
        {
            if (_api == null)
                ContainerHolder.Container.BuildUp(this);
            if (_api == null)
            {
                Log.Warn($"GlobalConflictPrivateApi not set");
                return Task.CompletedTask;
            }

            _api.Update().ReportOnFail().GetAwaiter().GetResult();
            return Task.CompletedTask;
        }
    }
}