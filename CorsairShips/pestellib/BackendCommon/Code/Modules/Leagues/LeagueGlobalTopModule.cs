using System;
using System.Threading.Tasks;
using MessagePack;
using PestelLib.ServerShared;
using S;

namespace ServerLib.Modules.Leagues
{
    public class LeagueGlobalTopModule : LeaguesModuleBase
    {
        public LeagueGlobalTopModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<ServerResponse> ProcessCommandAsync(ServerRequest request)
        {
            var result = Validate(nameof(LeagueGlobalTopModule));
            if (result != null)
                return result;
            var req = request.Request.LeagueGlobalTop;
            var resp = await LeagueServer.GlobalTop(req.PlayerId, req.Amount);
            return new ServerResponse()
            {
                ResponseCode = ResponseCode.OK,
                PlayerId = req.PlayerId,
                Data = MessagePackSerializer.Serialize(resp)
            };
        }
    }
}
