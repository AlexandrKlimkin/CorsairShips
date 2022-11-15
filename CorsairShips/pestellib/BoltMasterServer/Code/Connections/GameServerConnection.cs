using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BoltTransport;
using log4net;
using MasterServerProtocol;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("BoltMasterServerTest")]
namespace BoltMasterServer.Connections
{
    /// <summary>
    /// Обработка входящих соединений от игровых серверов осуществляется инстансами этого класса.
    /// 
    /// Т.е. первоначальные данные мастер передает игровому серверу через аргументы командной строки
    /// (см. GameServerLauncher), а после этого коммуникация осуществляется через
    /// GameServerConnection.
    /// </summary>
    internal class GameServerConnection : Connection
    {
        private static ILog _log = LogManager.GetLogger(typeof(GameServerConnection));

        private Settings settings;
        private GameServersCollection gameServersCollection;
        
        public GameServerConnection()
        {
            throw new NotImplementedException();
        }

        public GameServerConnection(GameServersCollection gameServers, Settings settings)
        {
            this.gameServersCollection = gameServers;
            this.settings = settings;
        }

        /// <summary>
        /// При подключении к MasterServer GameServer всегда первым делом отправляет репорт. Самое важное
        /// в нём - ProcessID, он позволяет связать MasterServer'у соединения с игровыми серверами и их
        /// ProcessID, которые необходимы для остановки ненужных инстансов.
        /// </summary>
        protected override async Task OnConnectionEstablished(Stream stream)
        {
            //Log("connection established");
            var report = (GameServerStateReport) await stream.ReadMessageAsync();

            var gameServer = new GameServer(this, report, settings);

            if (!gameServersCollection.TryAdd(report.GameServerId, gameServer))
            {
                _log.Error("ERROR: connected game server with duplicated ProcessID: " + report.GameServerId);
            }
        }

        /// <summary>
        /// Игровой сервер каждые несколько секунд отправлят свой статус мастер серверу. Мастер отдаёт
        /// это состояние в сводном репорте в лоадбалансинг, который потом использует эти данные для
        /// матчмейкинга.
        /// 
        /// При получении отчета мы его просто обновляем в dictionary, в котором лежат все отчеты игровых
        /// серверов.
        /// </summary>
        ///
        /// <param name="r">    отчет игрового сервера. </param>
        [MessageHandler]
        public void GameServerStateReport(GameServerStateReport r)
        {
            //_log.Info($"received GameServerStateReport from game server with process{r.ProcessID}");
            gameServersCollection.AddOrUpdate(r.GameServerId, new GameServer(this, r, settings), (key, existingServer) => {
                existingServer.State = r;
                return existingServer;
            });
        }

        [MessageHandler]
        public void RemoveGameServerFromMaster(RemoveGameServerFromMaster cmd)
        {
            gameServersCollection.TryRemove(cmd.GameServerId, out var removed);
            _log.Debug($"Server with id {cmd.GameServerId} was removed by command RemoveGameServerFromMaster");
        }
    }
}
