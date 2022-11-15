using System;
using BackendCommon.Code.Leagues;
using PestelLib.ServerShared;
using S;
using log4net;

namespace ServerLib.Modules.Leagues
{
    public abstract class LeaguesModuleBase : ModuleAsyncBase
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(LeaguesModuleBase));
        protected readonly IServiceProvider ServiceProvider;
        protected readonly LeagueServer LeagueServer;

        protected LeaguesModuleBase(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            LeagueServer = ServiceProvider.GetService(typeof(LeagueServer)) as LeagueServer;
        }

        protected ServerResponse Validate(string moduleName)
        {
            if (LeagueServer == null)
            {
                Log.Error($"{moduleName}: LeagueServer not found");
                return new ServerResponse()
                {
                    ResponseCode = ResponseCode.SERVER_EXCEPTION,
                };
            }

            return null;
        }

        protected void SecurityStrip(LeaguePlayerInfo[] playerInfos)
        {
            for (var i = 0; i < playerInfos.Length; i++)
            {
                playerInfos[i].PlayerId = Guid.Empty;
            }
        }
    }
}
