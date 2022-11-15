using System;
using System.Diagnostics;
using BackendCommon.Code.Data;
using PestelLib.ServerShared;
using S;

namespace ServerLib.Modules
{
    public class ResetDataModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest serverRequest)
        {
            var request = serverRequest.Request;

            StateLoader.Storage.Delete(new Guid(request.UserId));
            Debug.WriteLine("Reset user " + request.UserId);

            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK
            };
        }
    }
}
