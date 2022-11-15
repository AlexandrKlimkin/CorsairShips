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
    public class LeaderboardGetRankTopModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest request)
        {
            var leaderboards = ContainerHolder.Container.Resolve<ILeaderboards>();
            var cmd = request.Request.LeaderboardGetRankTop;

            LeaderboardUtils.CheckLeaderboardName(cmd.Type);

            int start = 0;
            int amount = cmd.Amount + 1;
            if (cmd.Amount == 0)
            {
                start = cmd.From;
                amount = cmd.To - cmd.From + 1;
            }
            var items = leaderboards.GetTop(cmd.Type, start, amount);

            var response = new LeaderboardGetRankTopResponse();
            response.Records.AddRange(items);

            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK,
                Data = MessagePackSerializer.Serialize(response)
            };
        }
    }
}