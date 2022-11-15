using MasterServerProtocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using log4net;

[assembly: InternalsVisibleTo("BoltMasterServerTest")]
namespace BoltMasterServer
{
    /// <summary>
    /// Запуск и остановка процессов игровых серверов, поддержание списка запущенных процессов.
    /// </summary>
    internal class GameServerLauncher : IGameServerLauncher
    {
        private static ILog _log = LogManager.GetLogger(typeof(GameServerLauncher));

        private Settings settings;
        private GameServersCollection gameServersCollection;
        public GameServerLauncher(Settings settings, GameServersCollection gameServersCollection)
        {
            this.gameServersCollection = gameServersCollection;
            this.settings = settings;
        }

        private Dictionary<Guid, Process> processes = new Dictionary<Guid, Process>();
        internal volatile int port;

        /// <summary>
        /// Остановка всех запущенных процессов, нужна при закрытии мастер-сервера, чтобы не оставались
        /// зависшие процессы.
        /// </summary>
        public void StopAllProcesses()
        {
            _log.Info("StopAllProcesses");
            lock (processes)
            {
                foreach (var process in processes.Values)
                {
                    process.Kill(true);
                }
            }
        }

        /// <summary>   Остановка одного из запущенных ранее процессов игровых серверов. </summary>
        ///
        /// <param name="gameServerId">    идентификатор сервера </param>
        ///
        /// <returns>   Может веруть false, если передан неверный идентификатор. </returns>
        public bool KillProcess(Guid gameServerId)
        {
            lock (processes)
            {
                if (processes.TryGetValue(gameServerId, out var target))
                {
                    target.Kill(true);
                    processes.Remove(gameServerId);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Запуск нового экземпляра сервера.
        /// 
        /// Выполняется в три стадии:
        /// 1) Запуск процесса с ожидание входящего соединения от этого процесса.  
        /// 2) Сразу же выполняется резервирование слота для игрока, который через лоадбалансинг.  
        /// инициировал создание игры чтобы он гарантированно мог в неё попасть.
        /// </summary>
        ///
        /// <param name="matchmakingData">  произвольная строка, которая может использоваться в
        ///                                 дальнейшем матчмейкингом для определения совместимости
        ///                                 игроков. Для обычного матчмейкинга это сериализованный в json
        ///                                 Dictionary&lt;string,string&gt; содержащий параметры матча:
        ///                                 сцена, уровень игроков, игровой режим. </param>
        ///
        /// <returns>
        /// JoinInfo, который в конечном итоге попадает к игроку, который запросил подключение к матчу у
        /// лоад-балансинга.
        /// </returns>
        public virtual async Task<JoinInfo> StartNewServerInstance(string matchmakingData)
        {
            if (port == 0)
            {
                port = settings.FirstGameServerPort;
            }

            var server = await StartNewInstance(matchmakingData);

            var reserveSlotResponse = await server.ReserveSlot(new ReserveSlotRequest());

            return new JoinInfo
            {
                Port = server.Port,
                IpAddress = server.IPAddress,
                ReservedSlot = reserveSlotResponse.JoinInfo.ReservedSlot,
                Map = reserveSlotResponse.JoinInfo.Map
            };
        }

        private async Task<IGameServer> StartNewInstance(string matchmakingData)
        {
            var connectionData = StartGameServerInstance(matchmakingData);

            IGameServer gameServer;
            while (!gameServersCollection.TryGetValue(connectionData.GameServerId, out gameServer))
            {
                //тут очень нужен таймаут, и повторная попытка создания сервера. Либо хотя бы возврат ошибки клиенту.
                await Task.Delay(30);
            }

            gameServer.Port = connectionData.Port;
            gameServer.IPAddress = connectionData.IPAddress;
            return gameServer;
        }

        private ConnectionData StartGameServerInstance(string matchmakingData)
        {
            ++port; //TODO: добавить переиспользование портов остановленных инстансов

            var matchmakingDataBytes = Encoding.UTF8.GetBytes(matchmakingData);
            var matchmakingDataBase64 = Convert.ToBase64String(matchmakingDataBytes);

            var gameServerId = Guid.NewGuid();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = settings.BuildFilePath,
                    Arguments = $"-nolog -batchmode -nographics -serverport {port} " +
                        $"-ip 127.0.0.1 " +
                        $"-noagones " +
                        $"-master {settings.MasterServerIp} " +
                        $"-masterport {settings.MasterListenerPort} " +
                        $"-matchmakingData {matchmakingDataBase64} " +
                        $"-gameServerId {gameServerId}",
                    RedirectStandardInput = false,
                    RedirectStandardError = false,
                    RedirectStandardOutput = false
                    /*
                    UseShellExecute = false,
                    RedirectStandardOutput = true, //если включить RedirectStandardOutput, то при относительно большой загрузке (10 игровых серверов и больше)
                                                    //несколько раз в час они теряют возможность отсылать сообщения мастер серверу. 
                                                    //Мастер сервер может в том числе решить что они зависли, и закрыть их.
                                                    //Как ещё не выводить stdout из запущенного игрового инстанса, пока не нашёл
                                                    //может запускать его через bash и делать перенаправление в /dev/null ?
                    RedirectStandardError = true,
                   */
                }
            };

            lock (processes)
            {
                processes[gameServerId] = process;
            }

            process.Start();

            //https://stackoverflow.com/questions/1145969/processinfo-and-redirectstandardoutput
            //если поставить RedirectStandardOutput и не подписаться на события, то запущенный процесс в какой-то момент
            //перестаёт присылать апдейты о своем состоянии, видимо зависает.
            //можно отключить RedirectStandardOutput, но тогда будет вся консоль в сообщениях от сразу всех запущенных инстансов
            /*
            process.ErrorDataReceived += ProcessGameServerOutput;
            process.OutputDataReceived += ProcessGameServerOutput;
            process.EnableRaisingEvents = true;
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            */

            return new ConnectionData() { 
                GameServerId = gameServerId,
                IPAddress = settings.MasterServerIp,
                Port = port
            };
        }

        void ProcessGameServerOutput(object sender, DataReceivedEventArgs args)
        {
            if (settings.EnabledUnityGameServerLogsInConsole)
            {
                _log.Info(args.Data);
            }
        }
    }
}
