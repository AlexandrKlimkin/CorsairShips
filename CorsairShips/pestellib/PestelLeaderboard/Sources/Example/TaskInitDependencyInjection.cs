using PestelLib.ClientConfig;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicBase;
using PestelLib.TaskQueueLib;
using PestelLib.UI;
using UnityDI;
using PestelLib.Utils;

namespace PestelLib.Leaderboard
{
    public class TaskInitDependencyInjection : Task
    {
        private static RequestQueue _requestQueue = null;

        override public void Run()
        {
            var container = ContainerHolder.Container;

            container.RegisterUnitySingleton<UpdateProvider>(null, true);
            container.RegisterUnitySingleton<SharedTime>(null, true);
            container.RegisterUnitySingleton<LeaderboardToSocialNetworkMediator, LeaderboardToSocialNetworkMediatorMock>(null, true);
            container.RegisterUnityScriptableObject<Config>(null);

            container.RegisterUnitySingleton<Gui>(null, true);

            container.RegisterCustom(() => container.Resolve<ISharedLogic>().GetModule<LeaderboardLeagueModule>());

            container.RegisterCustom<RequestQueue>(() =>
            {
                if (_requestQueue == null)
                {
                    _requestQueue = new RequestQueue();
                    ContainerHolder.Container.BuildUp(_requestQueue);
                }
                return _requestQueue;
            });

            OnComplete(this);
        }

    }
}