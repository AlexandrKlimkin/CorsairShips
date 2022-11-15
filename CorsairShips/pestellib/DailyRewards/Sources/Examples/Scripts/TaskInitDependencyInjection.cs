using PestelLib.Chests;
using PestelLib.Localization;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicBase;
using PestelLib.SharedLogicClient;
using PestelLib.TaskQueueLib;
using PestelLib.UI;
using UnityDI;
using PestelLib.Utils;
using PestelLib.ServerClientUtils;

namespace PestelLib.DailyRewards
{
    public class TaskInitDependencyInjection : Task
    {
        override public void Run()
        {
            var container = new Container();
            ContainerHolder.Container = container;

            container.RegisterUnitySingleton<UpdateProvider>(null, true);
            container.RegisterUnitySingleton<DailyRewardsExampleGame>(null, true);
            container.RegisterUnitySingleton<CommandProcessor>(null, true);
            container.RegisterUnitySingleton<DailyRewardsDefinitionsContainer>(null, true);
            container.RegisterUnitySingleton<SharedTimeHack>(null, true);
            container.RegisterUnitySingleton<UiDailyRewards>(null, true);
            container.RegisterSingleton<ILocalization, LocalizationData>(null);
            container.RegisterUnitySingleton<ChestsRewardVisualizer>(null, true);
            container.RegisterUnitySingleton<SpritesDatabase>(null, true);

            container.RegisterCustom<SharedTime>(() => ContainerHolder.Container.Resolve<SharedTimeHack>());
            container.RegisterCustom<IDailyRewardsConcreteGameInterface>(() => ContainerHolder.Container.Resolve<DailyRewardsExampleGame>());

            container.RegisterCustom(() => ContainerHolder.Container.Resolve<DailyRewardsDefinitionsContainer>().SharedLogicDefs.DailyRewardDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<DailyRewardsDefinitionsContainer>().SharedLogicDefs.DailyRewardDefDict);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<DailyRewardsDefinitionsContainer>().SharedLogicDefs.ChestRewardsDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<DailyRewardsDefinitionsContainer>().SharedLogicDefs.ChestRewardsDefsDict);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ISharedLogic>().GetModule<DailyRewardModule>());
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ISharedLogic>().GetModule<ChestModule>());

            container.RegisterUnitySingleton<Gui>(null, true);
            container.RegisterUnitySingleton<NotificationGui>(null, true);

            OnComplete(this);
        }

    }
}