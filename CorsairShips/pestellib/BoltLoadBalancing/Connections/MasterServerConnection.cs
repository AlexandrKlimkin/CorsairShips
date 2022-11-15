using BoltTransport;
using MasterServerProtocol;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using BoltLoadBalancing.MasterServer;
using Newtonsoft.Json;
using log4net;

namespace BoltLoadBalancing
{
    /// <summary>   Обработка входящих соединений от мастер-серверов </summary>
    public class MasterServerConnection : Connection
    {
        private static ILog _log = LogManager.GetLogger(typeof(MasterServerConnection));

        private readonly MasterServersCollection _masterServers;
        
        //https://blog.cdemi.io/async-waiting-inside-c-sharp-locks/
        private readonly SemaphoreSlim _masterServerConfigSemaphore;
        
        private IMasterServer _masterServer;

        public MasterServerConnection()
        {
            throw new NotImplementedException();
        }
        
        public MasterServerConnection(MasterServersCollection masterServers, SemaphoreSlim masterServerConfigSemaphore)
        {
            _masterServers = masterServers;
            _masterServerConfigSemaphore = masterServerConfigSemaphore;
        }

        /// <summary>
        /// При потере соединения нужно удалить мастер сервер из списка всех доступных мастер серверов.
        /// </summary>
        protected override Task OnConnectionLost(Exception reason)
        {
            if (_masterServer != null)
            {
                _log.Info($"Disconnected master server: {_masterServer.InstanceId} due to: {reason.Message}");
                _masterServers.TryRemove(_masterServer.InstanceId, out var master);
            } 
            else
            {
                _log.Info($"Disconnected unknown master server");
            }
            return base.OnConnectionLost(reason);
        }

        /// <summary>
        /// Обновляем данные по мастер-серверу в соответствии с пришедшим от него репортом.
        /// </summary>
        ///
        /// <param name="r">    A MasterServerReport to process. </param>
        [MessageHandler]
        public void RegisterReport(MasterServerReport r)
        {
            _masterServer = _masterServers.AddOrUpdate(r.InstanceId, new MasterServer.MasterServer(this, r), (key, existingServer) => {
                existingServer.UpdateState(r);
                return existingServer;
            });
        }

        /// <summary>
        /// Возвращаем мастер-серверу его конфиг: уникальный порт для него самого, и диапазон портов для
        /// игровых серверов.
        /// </summary>
        [MessageHandler]
        public async Task<MasterConfigurationResponse> MasterConfigurationRequest(
            MasterConfigurationRequest request)
        {
            await _masterServerConfigSemaphore.WaitAsync();
            try {
                static int GetFreeMasterServerPort(int[] usedPorts)
                {
                    if (!usedPorts.Any())
                    {
                        return Settings.FirstMasterServerPort;
                    }

                    for (var i = Settings.FirstMasterServerPort; i < Settings.LastMasterServerPort; i++)
                    {
                        if (!usedPorts.Contains(i)) return i;
                    }
                    var errorMsg = "Can't find free port for master server!";
                    _log.Error(errorMsg);
                    throw new Exception(errorMsg);
                }

                static int GetFirstGameServerPortFromMasterServerPort(int masterServerPort)
                {
                    var offset = Settings.FirstGameServerPort;
                    var rangePerMaster = Settings.MaxGameServersPerMaster;

                    var masterServerIndex = masterServerPort - Settings.FirstMasterServerPort;
                    return Settings.FirstGameServerPort + masterServerIndex * rangePerMaster;
                }

                var servers = _masterServers.Values;

                var usedPorts = servers.Where(x => x.RemoteIP.Equals(RemoteIP))
                    .OrderBy(x => x.MasterListenerPort)
                    .Select(x => x.MasterListenerPort)
                    .ToArray();

                var masterListenerPort = GetFreeMasterServerPort(usedPorts);
                var firstGameServerPort = GetFirstGameServerPortFromMasterServerPort(masterListenerPort);

                _log.Info(
                    $"Configured master: masterListenerPort: {masterListenerPort} " +
                    $" firstGameServerPort: {firstGameServerPort} " +
                    $" usedPorts: {JsonConvert.SerializeObject(usedPorts)}");

                request.Report.MasterListenerPort = masterListenerPort;
                RegisterReport(request.Report);
                
                return new MasterConfigurationResponse
                {
                    MasterListenerPort = masterListenerPort,
                    FirstGameServerPort = firstGameServerPort,
                    MessageId = request.MessageId
                };
            }
            finally
            {
                _masterServerConfigSemaphore.Release();
            }
        }
    }
}
