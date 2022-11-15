using System;
using System.Threading.Tasks;
using log4net;
using PestelLib.ServerShared;
using S;
using ServerLib;
using ServerLib.Modules;
using UnityDI;

namespace BackendCommon.Code.Modules.GlobalConflict
{
    public class GlobalConflictApiModule : ModuleAsyncBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GlobalConflictApiModule));
#pragma warning disable 0649
        [Dependency]
        private GlobalConflictApiCallHandler _handler;
#pragma warning restore 0649

        public GlobalConflictApiModule()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        public override async Task<ServerResponse> ProcessCommandAsync(ServerRequest request)
        {
            if (!AppSettings.Default.GloabalConflict)
                throw new InvalidOperationException("GloabalConflict is switched off");

            var apiCall = request.Request.GlobalConflictApiCall;
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