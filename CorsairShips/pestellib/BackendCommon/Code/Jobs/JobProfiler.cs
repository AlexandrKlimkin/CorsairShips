using System.Threading;
using System.Threading.Tasks;
using Backend.Code.Statistics;
using Quartz;
using UnityDI;

namespace BackendCommon.Code.Jobs
{
    class JobProfiler : IJobListener
    {
        private readonly string StatCat = "quartz";
#pragma warning disable 0649
        [Dependency] private DefaultStatisticsClient _statisticsClient;
#pragma warning restore 0649

        public JobProfiler()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellation)
        {
            return Task.CompletedTask;
        }

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellation)
        {
            return Task.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellation)
        {
            var name = context.JobInstance.GetType().Name;
            _statisticsClient?.SendAsync(StatCat, name, context.JobRunTime.TotalMilliseconds);
            return Task.CompletedTask;
        }

        public string Name => "PestelJobProfiler";
    }
}