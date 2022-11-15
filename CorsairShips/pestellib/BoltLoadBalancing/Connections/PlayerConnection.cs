using BoltTransport;
using MasterServerProtocol;
using System.Threading.Tasks;
using System.Linq;
using System;
using BoltLoadBalancing.Logic;
using BoltLoadBalancing.MasterServer;
using log4net;
using Newtonsoft.Json;

namespace BoltLoadBalancing
{
    /// <summary>   Обработка входящих соединений от игроков </summary>
    public class PlayerConnection : Connection
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PlayerConnection));

        private readonly IMatchmaking _matchmaking;
        private readonly MasterServersCollection _masterServersCollection;
        private readonly ICreateOrJoinGameHelper _createOrJoinGameHelper;

        public PlayerConnection()
        {
            throw new NotImplementedException();
        }
        public PlayerConnection(MasterServersCollection masterServersCollection, IMatchmaking matchmaking, ICreateOrJoinGameHelper createOrJoinGameHelper)
        {
            _matchmaking = matchmaking;
            _masterServersCollection = masterServersCollection;
            _createOrJoinGameHelper = createOrJoinGameHelper;
        }

        protected override Task OnConnectionLost(Exception reason)
        {
            log.Info("Player disconnected: " + reason.Message + " " + reason.StackTrace);
            return base.OnConnectionLost(reason);
        }

        /// <summary>
        /// Запрос подходящего игрового сервера.
        /// 
        /// Вначале отбираются сервера по имени и версии игры, затем по строке матчмейкинга. Если
        /// подходящих игр нет - отправляется запрос на создание новой игры на мастер сервер, и
        /// возвращаются данные для подключения к ней.
        /// </summary>
        [MessageHandler]
        public async Task<Message> RequestServer(RequestServer requestServer)
        {
            var masters = _masterServersCollection.Values.Where(x => x.GameInfo.Equals(requestServer.GameInfo));

            log.Info($"Player requested server, request id = {requestServer.MessageId}");

            //проверяем, есть ли хоть один мастер сервер с подходящим приложением и версией
            if (!masters.Any())
            {
                log.Error($"Can't find any compatible master for {requestServer.GameInfo.GameName}: {requestServer.GameInfo.GameVersion}");
                return new Error { Code = ErrorCode.COMPATIBLE_MASTER_NOT_FOUND };
            }

            var possibleMatches = _matchmaking.GetPossibleMatches(requestServer.MatchmakingData, masters);
            log.Info($"Found {possibleMatches.Count} compatible matches, request id = {requestServer.MessageId}");

            try
            {
                log.Info($"Requesting join info, request id = {requestServer.MessageId}");
                var joinInfo = await _createOrJoinGameHelper.GetJoinInfo(possibleMatches, requestServer);

                log.Info($"returned join info: {JsonConvert.SerializeObject(joinInfo, Formatting.Indented)}, request id = {requestServer.MessageId}");

                return new RequestServerResponse
                {
                    JoinInfo = joinInfo,
                    MessageId = requestServer.MessageId
                };
            }
            catch (ServersAreFullException)
            {
                log.Error("Servers are full, request id = {requestServer.MessageId}");
                return new Error { Code = ErrorCode.SERVERS_ARE_FULL };
            }
        }
    }
}
