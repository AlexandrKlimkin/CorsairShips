using System;
using BackendCommon.Code.Data;
using PestelLib.ServerShared;
using ServerLib.Modules;
using S;

namespace Backend.Code.Modules
{
    public class ValidateSessionModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest serverRequest)
        {
            var request = serverRequest.Request;
            var userId = new Guid(request.UserId);
            var lastUsedDeviceId = StateLoader.GetLastUsedDeviceId(userId);
            if (!string.IsNullOrEmpty(lastUsedDeviceId) && lastUsedDeviceId != request.DeviceUniqueId)
            {
                return new ServerResponse
                {
                    ResponseCode = ResponseCode.WRONG_SESSION,
                    PlayerId = userId
                };
            }

            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK
            };
        }
    }
}