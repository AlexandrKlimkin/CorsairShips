using System;
using System.Threading.Tasks;
using MessagePack;
using PestelLib.ServerShared;
using S;

namespace ServerLib.Modules.Leagues
{
    public class LeaguePlayerGlobalRankModule : LeaguesModuleBase
    {
        public LeaguePlayerGlobalRankModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<ServerResponse> ProcessCommandAsync(ServerRequest request)
        {
            var result = Validate(nameof(LeaguePlayerGlobalRankModule));
            if (result != null)
                return result;
            var req = request.Request.LeaguePlayerGlobalRank;
            var resp = await LeagueServer.PlayerGlobalRank(req.PlayerId);
            return new ServerResponse()
            {
                ResponseCode = ResponseCode.OK,
                PlayerId = req.PlayerId,
                Data = MessagePackSerializer.Serialize(resp)
            };
        }
    }
}
