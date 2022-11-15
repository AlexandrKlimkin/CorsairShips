using System;
using PestelLib.ServerShared;
using S;

namespace ServerLib.Modules
{
    class SyncTimeModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest request)
        {
            return new ServerResponse
            {
                PlayerId = new Guid(request.Request.UserId),
                ResponseCode = ResponseCode.OK
            };
        }
    }
}
