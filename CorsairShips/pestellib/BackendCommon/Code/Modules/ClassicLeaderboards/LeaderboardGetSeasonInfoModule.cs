using PestelLib.ServerShared;
using PestelLib.UniversalSerializer;
using S;
using ServerLib.Modules;

namespace BackendCommon.Code.Modules.ClassicLeaderboards
{
    public class LeaderboardGetSeasonInfoModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest request)
        {
            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK,
                Data = Serializer.Serialize(new LeaderboardGetSeasonInfoResponse
                {
                    SeasonId = LeaderboardUtils.CurrentSeasonId,
                    SeasonIndex = LeaderboardUtils.CurrentSeasonIndex
                })
            };
        }
    }
}