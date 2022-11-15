using System;
using MessagePack;
using PestelLib.ServerShared;
using S;
using ServerLib.Modules;

namespace BackendCommon.Code.Modules
{
    public class DefsModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest request)
        {
            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK,
                Data = MessagePackSerializer.Serialize(DefsLoader.DefsData),
                PlayerId = new Guid(request.Request.UserId)
            };
        }
    }
}