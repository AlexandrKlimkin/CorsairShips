using System;
using System.Threading.Tasks;
using log4net;
using PestelLib.ServerShared;
using S;
using ServerLib.Modules;
using ServerLib.PlayerProfile;
using UnityDI;

namespace Backend.Code.Modules.PlayerProfile
{
    public class PlayerProfileApiModule : ModuleAsyncBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PlayerProfileApiModule));
        [Dependency]
        private IProfileStorage _storage;
        [Dependency]
        private PlayerProfileApiCallHandler _handler;

        public PlayerProfileApiModule()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        public override async Task<ServerResponse> ProcessCommandAsync(ServerRequest request)
        {
            _storage = ContainerHolder.Container.Resolve<IProfileStorage>();
            _handler = ContainerHolder.Container.Resolve<PlayerProfileApiCallHandler>();

            if(_storage == null)
                throw new InvalidOperationException("ProfileStorage not found");
            if(_handler == null)
                throw new InvalidOperationException("PlayerProfileApiCallHandler not found");

            var apiCall = request.Request.PlayerProfile;
            byte[] r;

            try
            {
                r = await _handler.Process(apiCall, request).ConfigureAwait(false);
            }
            catch (AggregateException e)
            {
                Log.Error(e.Flatten());
                throw;
            }

            return new ServerResponse()
            {
                ResponseCode = ResponseCode.OK,
                Data = r
            };
        }
    }
}