using System;
using System.Threading.Tasks;
using BackendCommon.Services;
using log4net;
using Quartz;

namespace BackendCommon.Code.Jobs
{
    class HiveHealthCheckerJob : IJob
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HiveHealthCheckerJob));

        public async Task Execute(IJobExecutionContext context)
        {
            var serviceProvider = context.MergedJobDataMap[nameof(IServiceProvider)] as IServiceProvider;
            if (serviceProvider == null)
            {
                Log.Debug("ServiceProvider not found");
                return;
            }

            if (!(serviceProvider.GetService(typeof(IBackendHivePrivate)) is IBackendHivePrivate hive))
            {
                Log.Debug("IBackendHivePrivate not found");
                return;
            }

            foreach (var service in hive.AllServices())
            {
                var online = service.IsOnline();
                Log.Debug($"Backend service '{service}' is online: {online}.");
                await hive.SetOnlineStatus(service, online);
            }
        }
    }
}
