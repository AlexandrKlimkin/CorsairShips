using System;
using System.Threading.Tasks;
using MessagePack;
using PestelLib.ServerShared;
using S;

namespace ServerLib.Modules.Leagues
{
    public class LeagueRegisterModule : LeaguesModuleBase
    {
        public LeagueRegisterModule(IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
        }

        public override async Task<ServerResponse> ProcessCommandAsync(ServerRequest request)
        {
            var result = Validate(nameof(LeagueRegisterModule));
            if (result != null)
                return result;
            var req = request.Request.LeagueRegister;
            var resp = await LeagueServer.Register(req.PlayerId, req.Name, req.FacebookId);
            return new ServerResponse()
            {
                ResponseCode = ResponseCode.OK,
                PlayerId = req.PlayerId,
                Data = MessagePackSerializer.Serialize(resp)
            };
        }
    }
}
