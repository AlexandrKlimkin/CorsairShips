using System;
using System.Threading.Tasks;
using MessagePack;
using PestelLib.ServerShared;
using S;

namespace ServerLib.Modules.Leagues
{
    public class LeagueTopModule : LeaguesModuleBase
    {
        public LeagueTopModule(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override async Task<ServerResponse> ProcessCommandAsync(ServerRequest request)
        {
            var result = Validate(nameof(LeagueTopModule));
            if (result != null)
                return result;
            var req = request.Request.LeagueTop;
            var resp = await LeagueServer.LeagueTop(req.PlayerId, req.Amount);
            return new ServerResponse()
            {
                ResponseCode = ResponseCode.OK,
                PlayerId = req.PlayerId,
                Data = MessagePackSerializer.Serialize(resp)
            };
        }
    }
}
