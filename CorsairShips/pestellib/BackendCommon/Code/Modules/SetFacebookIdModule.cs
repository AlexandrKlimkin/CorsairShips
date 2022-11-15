using System;
using BackendCommon.Code.Data;
using MessagePack;
using PestelLib.ServerShared;
using S;

namespace ServerLib.Modules
{
    public class SetFacebookIdModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest requestContainer)
        {
            var request = requestContainer.Request.SetFacebookIdRequest;

            if (string.IsNullOrEmpty(request.FacebookId))
            {
                throw new ResponseException(ResponseCode.EMPTY_SOCIAL_ID);
            }

            var playerIdForRequestedFacebookId = StateLoader.Storage.GetPlayerIdByFacebookId(request.FacebookId);

            //we already have user with this facebookId and userId in DB?
            if (playerIdForRequestedFacebookId != Guid.Empty && playerIdForRequestedFacebookId == new Guid(requestContainer.Request.UserId)) 
            {
                return new ServerResponse
                {
                    ResponseCode = ResponseCode.OK,
                    Data = MessagePackSerializer.Serialize(new SetFacebookIdResponse { Success = true })
                };
            } 

            //first time login, or destroy old user progression and write new one under current playerId
            if (playerIdForRequestedFacebookId == Guid.Empty || request.Forced) {
                StateLoader.Storage.SetFacebookId(new Guid(requestContainer.Request.UserId), request.FacebookId);

                return new ServerResponse
                {
                    ResponseCode = ResponseCode.OK,
                    Data = MessagePackSerializer.Serialize(new SetFacebookIdResponse {Success = true})
                };
            }

            //can't set facebook id - DB already has with id. Ask user about forced replacement
            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK,
                Data = MessagePackSerializer.Serialize(new SetFacebookIdResponse
                {
                    NewPlayerId = playerIdForRequestedFacebookId.ToByteArray(), 
                    Success = false
                })
            };
        }
    }
}
