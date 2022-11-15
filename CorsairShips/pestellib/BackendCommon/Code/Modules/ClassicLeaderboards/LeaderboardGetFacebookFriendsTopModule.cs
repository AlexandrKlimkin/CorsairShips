using System;
using MessagePack;
using PestelLib.ServerShared;
using S;
using ServerLib;
using ServerLib.Modules;
using UnityDI;
using ClassicLeaderboards;

namespace BackendCommon.Code.Modules.ClassicLeaderboards
{
    public class LeaderboardGetFacebookFriendsTopModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest serverRequest)
        {
            var leaderboards = ContainerHolder.Container.Resolve<ILeaderboards>();
            var request = serverRequest.Request.LeaderboardGetFacebookFriendsTop;

            var response = new LeaderboardGetRankTopResponse();

            if (request.Friends == null)
            {
                return new ServerResponse
                {
                    Data = MessagePackSerializer.Serialize(response),
                    ResponseCode = ResponseCode.OK
                };
            }

            for (int i = 0; i < request.Friends.Count; i++)
            {
                var facebookId = request.Friends[i];

                var player = leaderboards.GetPlayer(request.Type, facebookId);

                var userId = player?.UserId ?? Guid.Empty.ToByteArray();

                response.Records.Add(new LeaderboardRecord
                {
                    UserId = userId,
                    FacebookId = facebookId,
                    Score = player?.Score ?? 0
                });
            }

            return new ServerResponse
            {
                Data = MessagePackSerializer.Serialize(response),
                ResponseCode = ResponseCode.OK
            };
        }
    }
}