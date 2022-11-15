using System;
using System.Diagnostics;
using BackendCommon.Code;
using BackendCommon.Code.Data;
using log4net;
using PestelLib.ServerShared;
using S;

namespace ServerLib.Modules
{
    public class ReplaceStateModule : IModule
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ReplaceStateModule));

        private const int MaxStateSize = 60*1024;
        private const int WarnStateSize = 40*1024;

        public ServerResponse ProcessCommand(ServerRequest serverRequest)
        {
            var request = serverRequest.Request;
            var requestContent = request.ReplaceStateRequest;
            
            {
                var logic = MainHandlerBase.ConcreteGame.SharedLogic(requestContent.State, MainHandlerBase.FeaturesCollection);
                Log.Debug("Replacing user state: " + new Guid(request.UserId) + " new state: " + logic.StateInJson());
            }

            if (requestContent.State.Length > WarnStateSize)
            {
                Log.Error("User state size overflow: " + requestContent.State.Length + " user id: " + new Guid(request.UserId));
            }

            if (requestContent.State.Length > MaxStateSize)
            {
                return new ServerResponse
                {
                    ResponseCode = ResponseCode.USER_STATE_SIZE_OVERFLOW
                };
            }

            StateLoader.Save(new Guid(request.UserId), requestContent.State, request.DeviceUniqueId);

            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK,
                PlayerId = new Guid(request.UserId)
            };
        }
    }
}

