using System;
using System.Collections.Generic;
using ClassicLeaderboards;
using MessagePack;
using PestelLib.ServerShared;
using S;
using ServerLib;
using ServerLib.Modules;
using StackExchange.Redis;
using UnityDI;

namespace BackendCommon.Code.Modules.ClassicLeaderboards
{
    public class LeaderboardGetRankModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest request)
        {
            var leaderboards = ContainerHolder.Container.Resolve<ILeaderboards>();
            var cmd = request.Request.LeaderboardGetRank;

            LeaderboardUtils.CheckLeaderboardName(cmd.Type);

            var userId = new Guid(request.Request.UserId);

            var item = leaderboards.GetPlayer(cmd.Type, userId);
            var response = new LeaderboardGetRankTopResponse();
            response.Records.Add(item);

            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK,
                Data = MessagePackSerializer.Serialize(response)
            };
        }
    }
}