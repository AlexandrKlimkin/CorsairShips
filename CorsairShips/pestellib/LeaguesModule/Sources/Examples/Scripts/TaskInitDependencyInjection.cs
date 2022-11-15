using PestelLib.ClientConfig;
using PestelLib.Localization;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicBase;
using PestelLib.SharedLogicClient;
using PestelLib.TaskQueueLib;
using PestelLib.UI;
using UnityDI;
using PestelLib.Utils;
using PestelLib.ServerClientUtils;

namespace PestelLib.Leagues
{
    public class TaskInitDependencyInjection : Task
    {
        private RequestQueue _requestQueue;

        override public void Run()
        {
            var container = new Container();
            ContainerHolder.Container = container;

            container.RegisterUnityScriptableObject<LeaguesExampleConfig>(null);
            container.RegisterSingleton<RequestQueue>();
            container.RegisterUnitySingleton<UpdateProvider>(null, true);
            container.RegisterUnitySingleton<LeaguesExampleGameInterface>(null, true);
            container.RegisterUnitySingleton<CommandProcessor>(null, true);
            container.RegisterUnitySingleton<LeaguesModuleDefinitionsContainer>(null, true);
            //container.RegisterUnitySingleton<SharedTimeHack>(null, true);
            container.RegisterUnitySingleton<UiLeagueScreenTabs>(null, true);
            container.RegisterSingleton<ILocalization, LocalizationData>(null);
            container.RegisterUnitySingleton<SpritesDatabase>(null, true);

            //container.RegisterCustom<SharedTime>(() => ContainerHolder.Container.Resolve<SharedTimeHack>());
            container.RegisterCustom<ILeaguesConcreteGameInterface>(() => ContainerHolder.Container.Resolve<LeaguesExampleGameInterface>());

            container.RegisterCustom(() => ContainerHolder.Container.Resolve<LeaguesModuleDefinitionsContainer>().SharedLogicDefs.ChestDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<LeaguesModuleDefinitionsContainer>().SharedLogicDefs.LeagueDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<LeaguesModuleDefinitionsContainer>().SharedLogicDefs.LeagueRewardDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<LeaguesModuleDefinitionsContainer>().SharedLogicDefs.RewardDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<LeaguesModuleDefinitionsContainer>().SharedLogicDefs.RewardPoolDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<LeaguesModuleDefinitionsContainer>().SharedLogicDefs.RewardPoolListDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ISharedLogic>().GetModule<LeaguesModule>());
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ISharedLogic>().GetModule<ChestModule>());

            container.RegisterUnitySingleton<Gui>(null, true);
            container.RegisterUnitySingleton<NotificationGui>(null, true);
            container.RegisterCustom<Config>(() => ContainerHolder.Container.Resolve<LeaguesExampleConfig>());

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