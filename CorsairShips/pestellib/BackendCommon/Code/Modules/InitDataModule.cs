using System;
using System.Threading.Tasks;
using BackendCommon.Code;
using BackendCommon.Code.Data;
using PestelLib.ServerCommon.Db.Auth;
using PestelLib.ServerShared;
using S;
using ShortPlayerId.Storage;
using UnityDI;

namespace ServerLib.Modules
{
    public class InitDataModule : IModuleAsync
    {
        public async Task<ServerResponse> ProcessCommandAsync(ServerRequest serverRequest)
        {
            var request = serverRequest.Request;

            Guid userId;
            var userStateBytes = StateLoader.LoadBytes(
                MainHandlerBase.ConcreteGame, 
                new Guid(request.UserId), 
                request.DeviceUniqueId, 
                (int)request.NetworkId, 
                out userId,
                null
            );

            if(AppSettings.Default != null && AppSettings.Default.CacheNameSizeLimit > 0)
                CacheName(userStateBytes, AppSettings.Default.CacheNameSizeLimit);

            var tokenStorage = MainHandlerBase.ServiceProvider.GetService(typeof(ITokenStoreWriter)) as ITokenStoreWriter;
            byte[] token = null;
            if (tokenStorage != null)
            {
                var authToken = tokenStorage.CreateToken(userId, TimeSpan.FromDays(1), serverRequest.HostAddr);
                token = authToken.TokenId.ToByteArray();
            }

            var shortIdStorage = ContainerHolder.Container.Resolve<ShortPlayerIdStorage>();
            var shortId = 0;
            if (shortIdStorage != null)
                shortId = await shortIdStorage.GetShortPlayerId(userId);

            return new ServerResponse
            {
                ResponseCode = ResponseCode.OK,
                Data = userStateBytes,
                PlayerId = userId,
                Token = token,
                ShortId = shortId
            };
        }

        private void CacheName(byte[] stateBytes, int maxSize)
        {
            try
            {
                var logic = MainHandlerBase.ConcreteGame.SharedLogic(stateBytes, MainHandlerBase.FeaturesCollection);
                StateLoader.Storage.BindUserIdToName(logic.PlayerId, logic.PlayerName, maxSize);
            }
            catch
            {
            }
        }

        public ServerResponse ProcessCommand(ServerRequest request)
        {
            throw new NotImplementedException();
        }
    }
}