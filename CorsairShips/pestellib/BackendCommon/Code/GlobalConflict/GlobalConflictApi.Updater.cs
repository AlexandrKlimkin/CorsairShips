using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Backend.Code.Statistics;
using BackendCommon.Code.GlobalConflict.Server.Stages;
using log4net;
using PestelLib.ServerCommon.Extensions;
using ServerLib;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict
{
    abstract partial class GlobalConflictPrivateApi
    {
        private int queueSize;
        private DateTime _lastUpdate = DateTime.MinValue;
        protected static readonly ILog Log = LogManager.GetLogger(typeof(GlobalConflictPrivateApi));
        [Dependency]
        protected DefaultStatisticsClient _statsClient;
        public const string _statCategory = "gloc";

        protected GlobalConflictPrivateApi()
        {
            ContainerHolder.Container.BuildUp(this);
            ContainerHolder.Container.RegisterSingleton<StageFactory>();
        }

        public async Task Update()
        {
            var conflictState = await ConflictsSchedulePrivateApi.GetCurrentConflictAsync().ConfigureAwait(false);
            if (conflictState == null)
                return;

            if(queueSize > 0)
                return;

            var sw = Stopwatch.StartNew();
            var d = DateTime.UtcNow - _lastUpdate;
            if (d < AppSettings.Default.GlobalConflictSettings.UpdateCooldown)
            {
                var r = Interlocked.Increment(ref queueSize);
                try
                {
                    if(r > 1)
                        return;
                    await Task.Delay(AppSettings.Default.GlobalConflictSettings.UpdateCooldown - d).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Decrement(ref queueSize);
                }
            }

            var conflict = new Conflict(conflictState);
            await conflict.Update().ReportOnFail().ConfigureAwait(false);

            // отправим время апдейта в стату
            _statsClient?.SendAsync(_statCategory, "step_time", sw.ElapsedMilliseconds);
        }
    }
}