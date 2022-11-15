using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoltLoadBalancing.MasterServer;
using BoltLoadBalancing.MatchMaking;
using log4net;
using MasterServerProtocol;
using Newtonsoft.Json;

namespace BoltLoadBalancing.Logic
{
    public interface ICreateOrJoinGameHelper
    {
        Task<JoinInfo> GetJoinInfo(List<MatchMakingGame> possibleMatches, RequestServer requestServer);
    }

    /// <summary>   Логика подключения к одному из существующих матчей, либо создания нового </summary>
    public class CreateOrJoinGameHelper : ICreateOrJoinGameHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CreateOrJoinGameHelper));

        private readonly MasterServersCollection _masterServersCollection;

        public CreateOrJoinGameHelper(MasterServersCollection masterServersCollection)
        {
            _masterServersCollection = masterServersCollection;
        }

        public async Task<JoinInfo> GetJoinInfo(List<MatchMakingGame> possibleMatches, RequestServer requestServer)
        {
            //если среди существующих матчей нет ни одного подходящего, создаём новый
            if (!possibleMatches.Any())
            {
                log.Info("Can't find any matches, creating new");
                return await CreateGame(requestServer);
            }

            //отдаём предпочтение серверам с низкой загрузкой, в первую очередь пытаемся подключиться к ним
            var orderedMatches = possibleMatches.OrderBy(x => x.MasterServer.CPUUsage);
            var joinInfo2 = await TryToJoinExistingGame(orderedMatches);
            if (joinInfo2 != null)
            {
                log.Info($"Join info isn't null, returning to client {JsonConvert.SerializeObject(joinInfo2)}");
                return joinInfo2;
            }

            log.Info("Can't reserve slot in existing matches, creating new");
            //нашли подходящие матчи, но ни в одном из них не удалось зарезервировать слот
            //поэтому запрашиваем у мастер сервера создание новой игры
            return await CreateGame(requestServer);
        }

        private async Task<JoinInfo> CreateGame(RequestServer requestServer)
        {
            log.Info("CreateGame begin");

            var masters = _masterServersCollection.Values.Where(x => x.GameInfo.Equals(requestServer.GameInfo));
            var masterWithLowestCpuUsage = masters.OrderBy(x => x.CPUUsage).FirstOrDefault();
            var createGameResponse = await masterWithLowestCpuUsage.CreateGame(new CreateGameRequest
            {
                MatchmakingData = requestServer.MatchmakingData
            });
            if (createGameResponse is CreateGameResponse response)
            {
                log.Info("CreateGame finish with success");
                return response.JoinInfo;
            }
            else if (createGameResponse is Error error && error.Code == ErrorCode.SERVERS_ARE_FULL)
            {
                log.Error("CreateGame finish with fail : ServersAreFullException");
                throw new ServersAreFullException();
            }
            else
            {
                log.Error("CreateGame finish with fail : Unknown response from CreateGameRequest");
                throw new Exception("Unknown response from CreateGameRequest");
            }
        }

        private async Task<JoinInfo> TryToJoinExistingGame(IEnumerable<MatchMakingGame> possibleMatches)
        {
            //отдаём предпочтение серверам с низкой загрузкой, в первую очередь пытаемся подключиться к ним
            var orderedMatches = possibleMatches.OrderBy(x => x.MasterServer.CPUUsage);

            foreach (var game in orderedMatches)
            {
                //проходим по все серверам, и на каждом из них пытаемся зарезервировать слот, пока
                //это не увенчается успехом                    
                var reserveSlotResponse = await game.MasterServer.ReserveSlot(new ReserveSlotRequest
                {
                    GameServerId = game.GameServerStateReport.GameServerId
                });

                if (reserveSlotResponse.Succeed)
                {
                    return reserveSlotResponse.JoinInfo;
                }
            }

            return null;
        }
    }
}