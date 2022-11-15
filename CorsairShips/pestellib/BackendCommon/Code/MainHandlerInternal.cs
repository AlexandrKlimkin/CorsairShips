using System;
using System.Threading.Tasks;
using BackendCommon.Services;
using log4net;
using MessagePack;
using PestelLib.ServerShared;
using S;
using ServerLib.Modules;

namespace BackendCommon.Code
{
    public class MainHandlerInternal : IRequestHandler
    {
        private static ILog Log = LogManager.GetLogger(typeof(MainHandlerInternal));

        public async Task<byte[]> Process(byte[] data, RequestContext ctx)
        {
            ServerResponse resp = new ServerResponse();
            var hive = MainHandlerBase.ServiceProvider.GetService(typeof(IBackendHive)) as IBackendHive;
            if (hive?.SelfService?.Internal == false)
            {
                resp.ResponseCode = ResponseCode.UNSUPPORTED_COMMANDS;
                resp.DebugInfo = "Access denied";
            }
            else
            {
                var serverRequest = MessagePackSerializer.Deserialize<ServerRequest>(data);
                var processingModule = MainHandlerBase.GetProcessingModule(serverRequest.Request);

                try
                {
                    Log.Debug($"MainHandler internal call. processingModule={processingModule.GetType()}.");
                    if (processingModule is IModuleAsync asyncModule)
                    {
                        resp = await asyncModule.ProcessCommandAsync(serverRequest);
                    }
                    else
                    {
                        resp = processingModule.ProcessCommand(serverRequest);
                    }
                }
                catch (ResponseException e)
                {
                    resp.ResponseCode = e.ResponseCode;
                    resp.DebugInfo = e.DebugMessage;
                    Log.Error(e.ResponseCode + " " + e + " " + e.DebugMessage);
                }
                catch (Exception e)
                {
                    resp.ResponseCode = ResponseCode.SERVER_EXCEPTION;
                    resp.DebugInfo = e.Message + e.StackTrace;
                }
            }

            var respBytes = MessagePackSerializer.Serialize(resp);
            return respBytes;
        }
    }
}
