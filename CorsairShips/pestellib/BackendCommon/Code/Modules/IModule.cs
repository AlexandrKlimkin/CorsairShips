using System.Threading.Tasks;
using PestelLib.ServerShared;

namespace ServerLib.Modules
{
    public interface IModule
    {
        ServerResponse ProcessCommand(ServerRequest request);
    }

    public interface IModuleAsync : IModule
    {
        Task<ServerResponse> ProcessCommandAsync(ServerRequest request);
    }

    public abstract class ConfiguredModule : IModule
    {
        protected readonly AppSettings _appSettings;

        protected ConfiguredModule(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public abstract ServerResponse ProcessCommand(ServerRequest request);
    }

    public abstract class ModuleAsyncBase : IModuleAsync
    {
        public ServerResponse ProcessCommand(ServerRequest request)
        {
            return Task.Run(() => ProcessCommandAsync(request)).Result;
        }

        public abstract Task<ServerResponse> ProcessCommandAsync(ServerRequest request);
    }
}