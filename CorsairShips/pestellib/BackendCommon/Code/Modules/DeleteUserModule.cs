using System;
using BackendCommon.Code.Data;
using PestelLib.ServerShared;
using S;
using ServerLib.Modules;

namespace BackendCommon.Code.Modules
{
    public class DeleteUserModule : IModule
    {
        public ServerResponse ProcessCommand(ServerRequest request)
        {
            var uid = new Guid(request.Request.UserId);
            
            if (StateLoader.Storage.UserExist(uid))
            {
                StateLoader.Storage.Delete(uid);
            }

            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK
            };
        }
    }
}