using System;
using System.Threading.Tasks;
using log4net;
using Quartz;
using ServerLib.PlayerProfile;

namespace BackendCommon.Code.Jobs
{
    public class CleanPlayerProfilesJob : IJob
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(CleanPlayerProfilesJob));

        public Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.MergedJobDataMap[nameof(IServiceProvider)] as IServiceProvider;
            if (serviceProvider == null)
            {
                _log.Error("ServiceProvider not found");
                return Task.CompletedTask;
            }

            var profileStorage = serviceProvider.GetService(typeof(IProfileStorage)) as IProfileStorage;
            if (profileStorage == null)
            {
                _log.Error("IProfileStorage not found");
                return Task.CompletedTask;
            }
            profileStorage.RemoveExpired().ContinueWith(_ =>
            {
                _log.Info($"{_.Result} expiried player profiles removed.");
            });
            return Task.CompletedTask;
        }
    }
}