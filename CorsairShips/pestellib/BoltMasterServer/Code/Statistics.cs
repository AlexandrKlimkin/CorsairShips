using System.Threading.Tasks;
using System.Linq;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using log4net;
using ServerShared;

namespace BoltMasterServer
{
    /// <summary>
    /// Вывод в консоль основной статистики по мастер серверу и игровым серверам: сколько игровых
    /// серверов запущено, сколько на каждом из них игроков и т.п.
    /// </summary>
    class Statistics
    {
        private static bool _logFullStats = false;
        private static ILog _log = LogManager.GetLogger(typeof(Statistics));

        private GameServersCollection gameServersCollection;
        private Settings settings;
        private GraphiteClient graphiteClient;

        public Statistics(Settings settings, GameServersCollection gameServersCollection)
        {
	        graphiteClient = new GraphiteClient(settings.GraphiteHost, settings.GraphitePort);

            this.gameServersCollection = gameServersCollection;
            this.settings = settings;
            Task.Run(() => PrintStats());
        }

        async Task PrintStats()
        {
            while (true)
            {
                try
                {
                    var servers = gameServersCollection.Values.Where(x => x != null);

                    var ccu = servers.Sum(x => x.Players);
                    var totalServerCount = servers.Count();
                    var blockedServers = servers.Count(x => x.IsServerGoingToClose());
                    var worstTime = servers.Any() ? servers.Max(x => x.TimeSinceUpdate) : TimeSpan.Zero;
                    var warningPrefix = worstTime > TimeSpan.FromSeconds(10) ? "!!!" : string.Empty;

                    _log.Info($"{warningPrefix}CCU: {ccu} Total servers: {totalServerCount} Closing servers: {blockedServers} Max servers: {settings.MaxServers} The worst update time: {worstTime}");
                    
                    if (_logFullStats)
                    {
                        foreach (var server in gameServersCollection)
                        {
                            _log.Info($"PID: {server.Key}\tPlayers: {server.Value.Players}\tReserved: {server.Value.Reserved}\tTime since update: {server.Value.TimeSinceUpdate}");
                        }
                    }

                    var serverId = settings.AgonesFleetName + "_" + settings.LoadBalancingIp.Replace('.', '_');
                    var prefix = $"pestel.{serverId}.masterserver";
                    graphiteClient.SendStat($"{prefix}.ccu", ccu);
                    graphiteClient.SendStat($"{prefix}.totalServerCount", totalServerCount);
                    graphiteClient.SendStat($"{prefix}.blockedServers", blockedServers);
                    graphiteClient.SendStat($"{prefix}.worstTime", worstTime.TotalSeconds);
                } 
                catch (Exception e)
                {
                    _log.Error("Can't print statistic: " + e.Message + " " + e.StackTrace + " " + e.InnerException);
                }

                await Task.Delay(10000);
            }
        }

        /*
         * Нужно вместо этого на линуксе смотреть через командную строку, например вот так:
         * https://stackoverflow.com/a/9229580/9936606
         * т.к. интересует загрузка CPU всеми процессами, а не только дочерними процессами данного мастер сервера
         */
        private async Task<double> GetCpuUsageForProcess()
        {
            return 0;
            /*
            //заменил идентификацию игровых серверов с ProcessID на GameServerId, поэтому сейчас не 
            //получится использовать реализацию ниже
            var startTime = DateTime.UtcNow;

            var processIds = gameServersCollection.Values.Select(x => x.ProcessID).ToArray();
            var startCpuUsage = new List<TimeSpan>();
            
            for (var i = 0; i < processIds.Length; i++)
            {
                startCpuUsage.Add(Process.GetProcessById(processIds[i]).TotalProcessorTime);
            }
            
            await Task.Delay(500);

            var endTime = DateTime.UtcNow;

            var usage = 0D;

            for (var i = 0; i < processIds.Length; i++)
            {
                var startCpu = startCpuUsage[i];
                var endCpuUsage = Process.GetProcessById(processIds[i]).TotalProcessorTime;
                usage += (endCpuUsage - startCpu).TotalMilliseconds;
            }

            //var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            //var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = usage / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
            */
        }
    }
}
