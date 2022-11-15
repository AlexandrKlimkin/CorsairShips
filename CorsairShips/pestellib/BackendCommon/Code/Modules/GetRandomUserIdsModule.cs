using System;
using BackendCommon.Code.Data;
using MessagePack;
using PestelLib.ServerShared;
using S;
using ServerLib.Modules;

namespace BackendCommon.Code.Modules
{
    public class GetRandomUserIdsModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest serverRequest)
        {
            var request = serverRequest.Request;
            var concreteRequest = request.GetRandomUserIds;
            
            var uids = StateLoader.GetRandomUserIds(
                new Guid(request.UserId),
                (int)request.NetworkId,
                concreteRequest.IgnoreFacebookIds,
                concreteRequest.IgnorePlayerIds.ConvertAll(x => new Guid(x))
            );

            var resp = new GetRandomUserIdsResponse();
            resp.UserIds.AddRange(uids);

            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK,
                Data = MessagePackSerializer.Serialize(resp)
            };
        }
    }
}
