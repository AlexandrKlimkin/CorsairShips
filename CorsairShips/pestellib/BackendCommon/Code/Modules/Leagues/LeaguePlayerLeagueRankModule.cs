using System;
using System.Threading.Tasks;
using MessagePack;
using PestelLib.ServerShared;
using S;

namespace ServerLib.Modules.Leagues
{
    public class LeaguePlayerLeagueRankModule : LeaguesModuleBase
    {
        public LeaguePlayerLeagueRankModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<ServerResponse> ProcessCommandAsync(ServerRequest request)
        {
            var result = Validate(nameof(LeaguePlayerLeagueRankModule));
            if (result != null)
                return result;
            var req = request.Request.LeaguePlayerLeagueRank;
            var resp = await LeagueServer.PlayerLeagueRank(req.PlayerId);
            return new ServerResponse()
            {
                ResponseCode = ResponseCode.OK,
                PlayerId = req.PlayerId,
                Data = MessagePackSerializer.Serialize(resp)
            };
        }
    }
}
