using System;
using System.Threading.Tasks;
using MessagePack;
using PestelLib.ServerShared;
using S;

namespace ServerLib.Modules.Leagues
{
    public class LeagueDivisionRanksModule : LeaguesModuleBase
    {
        public LeagueDivisionRanksModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<ServerResponse> ProcessCommandAsync(ServerRequest request)
        {
            var result = Validate(nameof(LeagueDivisionRanksModule));
            if (result != null)
                return result;
            var req = request.Request.LeagueDivisionRanks;
            var resp = await LeagueServer.DivisionPlayersRanks(req.PlayerId);
            return new ServerResponse()
            {
                ResponseCode = ResponseCode.OK,
                PlayerId = req.PlayerId,
                Data = MessagePackSerializer.Serialize(resp)
            };
        }
    }
}
