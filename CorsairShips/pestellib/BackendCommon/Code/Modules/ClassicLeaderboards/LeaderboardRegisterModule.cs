using System;
using ClassicLeaderboards;
using PestelLib.ServerShared;
using S;
using ServerLib.Modules;
using UnityDI;

namespace BackendCommon.Code.Modules.ClassicLeaderboards
{
    public class LeaderboardRegisterModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest request)
        {
            var cmd = request.Request.LeaderboardRegisterRecord;
            var uid = new Guid(request.Request.UserId);

            var leaderboards = ContainerHolder.Container.Resolve<ILeaderboards>();
            leaderboards.RegisterRecord(cmd, uid);

            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK
            };
        }
    }
}