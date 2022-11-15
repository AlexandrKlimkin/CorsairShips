using System.Collections.Generic;
using log4net;
using log4net.Core;
using PestelLib.ServerShared;
using ServerExtension;

namespace ServerLib.Modules
{
    public class ServerExtensions : IModule
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServerExtensions));

        private readonly Dictionary<string, IExtension> _extensions = new Dictionary<string, IExtension>();

        public void RegisterExtension(IExtension extension)
        {
            _extensions[extension.GetType().Name] = extension;
        }

        public ServerResponse Process(string extensionType, byte[] requestData)
        {
            return _extensions[extensionType].ProcessRequest(requestData);
        }

        public ServerResponse ProcessCommand(ServerRequest request)
        {
            var extensionRequest = request.Request.ExtensionModuleRequest;
            if (!_extensions.ContainsKey(extensionRequest.ModuleType))
            {
                Log.ErrorFormat("Cant find extension module: " + extensionRequest.ModuleType);
            }
            var module = _extensions[extensionRequest.ModuleType];
            return module.ProcessRequest(extensionRequest.Request);
        }
    }
}