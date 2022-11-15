using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BoltTransport;
using log4net;
using MasterServerProtocol;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("BoltMasterServerTest")]
namespace BoltMasterServer.Connections
{
    /// <summary>
    /// Исходящее соединение с лоадбалансингом
    /// 
    /// Лоад балансинг:
    /// 1) Периодически получает отчет от каждого мастера о состоянии мастера и его игровых серверов.
    /// 2) Может просить зарезервировать слот в существующей игре.  
    /// 3) Может просить создать новую игру.
    /// </summary>
    class LoadBalancingConnection : Connection
    {
        private static bool _logEnabled = false;
        private static readonly ILog log = LogManager.GetLogger(typeof(LoadBalancingConnection));

        /// <summary>
        /// Лоадбалансингу нужно отличать как-то несколько мастер серверов, потэтому каждый мастер сервер
        /// назначает на запуске сам себе уникальный идентификатор.
        /// </summary>
        private static Guid MyIdentifier = Guid.NewGuid();

        private Settings settings;
        private IGameServerLauncher gameServerLauncher;
        private GameServersCollection gameServersCollection;

        public LoadBalancingConnection()
        {
            throw new NotImplementedException();
        }

        public LoadBalancingConnection(Settings settings, IGameServerLauncher gameServerLauncher, GameServersCollection gameServersCollection)
        {
            this.gameServersCollection = gameServersCollection;
            this.gameServerLauncher = gameServerLauncher;
            this.settings = settings;
        }

        /// <summary>
        /// Получение полного отчета о данном мастер сервере и всех игровых серверах, которыми он
        /// управляет.
        /// </summary>
        ///
        /// <returns>   The report. </returns>
        public MasterServerReport GetReport()
        {
            var gameServers = gameServersCollection.Values.Select(x => x.State).ToArray();
            return new MasterServerReport
            {
                GameInfo = new GameInfo
                {
                    GameName = settings.GameName,
                    GameVersion = settings.GameVersion
                },
                InstanceId = MyIdentifier,
                CPUUsage = gameServers.Length, //TODO: implement CPU usage meter
                GameServers = gameServers,
                MasterListenerPort = settings.MasterListenerPort
            };
        }

        /// <summary>
        /// Зацикленная отправка репорта о своём состоянии мастер серверу каждую секунду.
        /// </summary>
        ///
        /// <returns>   An asynchronous result. </returns>
        public async Task ReportToLoadBalancing()
        {
            while (true)
            {
                SendMessage(GetReport());
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// Когда игрок хочет подключиться к существующей игре, нам нужно зарезервировать слот для него:
        /// 1) Резервацию слота можно сделать заранее, до длительного процесса подключения игрока и.  
        /// загрузки им игрового уровня. Благодаря этому в один матч не зайдет больше, чем максимальное
        /// число игроков в комнате. Это снижает вероятность того, что игрока выкинет в процессе
        /// подключения.
        /// 1) Статусы игровых серверов обновляются не мгновенно, если в течение короткого промежутка.  
        /// времени 3 игрока попытаются зайти на сервер, где 1 свободный слот, то без резервации слотов
        /// этому не будет препятствий.
        /// Если зарезервировать слот не получится, лоадбалансинг попробует зарезервировать в других 
        /// подходящих матчах, если нигде не выйдет - отправит мастеру запрос на создание новой игры
        /// </summary>
        ///
        /// <param name="reserveSlotRequest">   The reserve slot request. </param>
        ///
        /// <returns>   An asynchronous result that yields a ReserveSlotResponse. </returns>
        [MessageHandler]
        public async Task<ReserveSlotResponse> ReserveSlotRequest(ReserveSlotRequest reserveSlotRequest)
        {
            Log($"ReserveSlotRequest received {reserveSlotRequest.MessageId}");
            if (gameServersCollection.TryGetValue(reserveSlotRequest.GameServerId, out var gameServer))
            {
                Log($"reserving slot started, request = {reserveSlotRequest.MessageId}");
                var response = await gameServer.ReserveSlot(reserveSlotRequest);
                Log($"reserving slot finished succeed: {response.Succeed}, join info = {JsonConvert.SerializeObject(response.JoinInfo, Formatting.Indented )} request = {reserveSlotRequest.MessageId}");
                return response;
            }

            Log($"Can't find game server with id = {reserveSlotRequest.GameServerId}, request = {reserveSlotRequest.MessageId}");
            return new ReserveSlotResponse() { Succeed = false };
        }

        /// <summary>
        /// Запрос на создание новой игры.
        /// </summary>
        ///
        /// <param name="createGameRequest">    The create game request. </param>
        ///
        /// <returns>   An asynchronous result that yields the new game request. </returns>
        [MessageHandler]
        public async Task<Message> CreateGameRequest(CreateGameRequest createGameRequest)
        {
            Log($"CreateGameRequest received {createGameRequest.MessageId}");

            if (gameServersCollection.Count >= settings.MaxServers)
            {
                Log($"CreateGameRequest error: all servers are full {createGameRequest.MessageId}");
                return new Error() { Code = ErrorCode.SERVERS_ARE_FULL };
            }
            
            var joinInfo = await gameServerLauncher.StartNewServerInstance(createGameRequest.MatchmakingData);
            Log($"CreateGameRequest sending join info: { JsonConvert.SerializeObject(joinInfo, Formatting.Indented) } {createGameRequest.MessageId}");

            return new CreateGameResponse() { 
                JoinInfo = joinInfo,
                MessageId = createGameRequest.MessageId
            };
        }

        private void Log(object message)
        {
            if (_logEnabled)
            {
                log.Info(message);
            }
        }
    }
}
