using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackendCommon.Code.Leagues;
using log4net;
using Quartz;
using ServerLib;

namespace BackendCommon.Code.Jobs
{
    public class LeaguesJob : IJob
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LeaguesJob));
        private static int _count;
        private static TimeSpan _jobExecPeriod = TimeSpan.FromSeconds(AppSettings.Default.LeagueJobPeriod);

        public Task Execute(IJobExecutionContext context)
        {
            var c = Interlocked.Increment(ref _count);
            if (c > 1)
            {
                Log.Warn($"LeaguesJob prev job still running");
                Interlocked.Decrement(ref _count);
                return Task.CompletedTask;
            }
            var sw = Stopwatch.StartNew();
            try
            {
                ExecuteInternal(context);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            finally
            {
                Interlocked.Decrement(ref _count);
                if(sw.Elapsed > _jobExecPeriod)
                    Log.Warn($"LeaguesJob execution overtime ({sw.ElapsedMilliseconds} ms).");
            }
            return Task.CompletedTask;
        }

        private void ExecuteInternal(IJobExecutionContext context)
        {
            var serviceProvider = context.MergedJobDataMap[nameof(IServiceProvider)] as IServiceProvider;
            if (serviceProvider == null)
            {
                Log.Debug("ServiceProvider not found");
                return;
            }

            if (!(serviceProvider.GetService(typeof(LeagueServer)) is LeagueServer leaguesServer))
            {
                Log.Debug("LeagueServer not found");
                return;
            }

            Log.Debug("LeaguesJob update");
            leaguesServer.SeasonController.Update();
        }
    }
}
