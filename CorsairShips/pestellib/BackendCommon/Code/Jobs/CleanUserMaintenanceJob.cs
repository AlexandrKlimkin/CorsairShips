using System;
using System.Threading.Tasks;
using log4net;
using PestelLib.ServerCommon.Db;

namespace BackendCommon.Code.Jobs
{
    public class CleanUserMaintenanceJob : JobWithServiceProvider
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CleanUserMaintenanceJob));

        public Task Execute(IUserMaintenancePrivate userMaintenancePrivate)
        {
            try
            {
                var count = userMaintenancePrivate.RemoveBeforeDate(DateTime.UtcNow);
                if (count > 0)
                    log.Debug($"User maintenance removed: {count}.");
            }
            catch (Exception e)
            {
                log.Warn("Can't clean maintenance list: " + e.Message);
            }
            return Task.CompletedTask;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(LeaguesJob));
    }
}
