using System.Threading.Tasks;
using Quartz;

namespace BackendCommon.Code.Jobs
{
    public class JobsControl : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            if (QuartzConfig.ShouldExecuteJobs())
                BackgroundProcesses.ResumeByGroup(BackgroundProcesses.DefaultGroup);
            else
                BackgroundProcesses.PauseByGroup(BackgroundProcesses.DefaultGroup);
            return Task.CompletedTask;
        }
    }
}