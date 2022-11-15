using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using PestelLib.ServerShared;
using ServerExtension;
using ServerLib.Modules;

namespace BackendCommon.Code.Modules
{
    public class ServerExtensionsAsync : IModuleAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServerExtensionsAsync));

        private readonly Dictionary<string, IAsyncExtension> _extensions = new Dictionary<string, IAsyncExtension>();

        public void RegisterExtension(IAsyncExtension extension)
        {
            _extensions[extension.GetType().Name] = extension;
        }

        public Task<ServerResponse> Process(string extensionType, byte[] requestData)
        {
            return _extensions[extensionType].ProcessRequestAsync(requestData);
        }
        
        public async Task<ServerResponse> ProcessCommandAsync(ServerRequest request)
        {
            var extensionRequest = request.Request.ExtensionModuleAsyncRequest;
            if (!_extensions.ContainsKey(extensionRequest.ModuleType))
            {
                Log.ErrorFormat("Cant find extension module: " + extensionRequest.ModuleType);
            }
            var module = _extensions[extensionRequest.ModuleType];
            return await module.ProcessRequestAsync(extensionRequest.Request);
        }

        public ServerResponse ProcessCommand(ServerRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}