using System;
using BackendCommon.Code.Data;
using MessagePack;
using PestelLib.ServerShared;
using S;
using ServerLib.Modules;

namespace BackendCommon.Code.Modules
{
    public class GetProfileModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest serverRequest)
        {
            var request = serverRequest.Request;
            var concreteRequest = request.GetProfileByFacebookIdRequest;
            var facebookId = concreteRequest.FacebookId;

            var userId = (concreteRequest.PlayerId != null) ? new Guid(concreteRequest.PlayerId) : Guid.Empty;

            if (!string.IsNullOrEmpty(facebookId))
            {
                userId = StateLoader.Storage.GetPlayerIdByFacebookId(facebookId);

                if (userId == Guid.Empty)
                {
                    //return facebook id not found
                    return MakeResponse(facebookId, new byte[0], GetProfileByFacebookIdCode.FACEBOOK_ID_NOT_FOUND);
                }

                if (!StateLoader.Storage.UserExist(userId))
                {
                    //return player data not found
                    return MakeResponse(facebookId, new byte[0], GetProfileByFacebookIdCode.USER_PROGRESS_NOT_FOUND);
                }
            }

            var profile = StateLoader.LoadBytes(
                MainHandlerBase.ConcreteGame,
                userId,
                null,
                (int)request.NetworkId,
                out userId,
                facebookId
            );

            return MakeResponse(facebookId, profile, GetProfileByFacebookIdCode.OK);
        }

        private ServerResponse MakeResponse(string facebookId, byte[] profile, GetProfileByFacebookIdCode resultCode)
        {
            var response = new GetProfileByFacebookIdResponse
            {
                FacebookId = facebookId,
                Profile = profile,
                ResultCode = resultCode
            };

            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK,
                Data = MessagePackSerializer.Serialize(response)
            };
        }
    }
}
