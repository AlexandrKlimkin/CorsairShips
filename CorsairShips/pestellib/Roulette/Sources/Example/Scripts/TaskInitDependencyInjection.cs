using PestelLib.Chests;
using PestelLib.Localization;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicBase;
using PestelLib.SharedLogicClient;
using PestelLib.TaskQueueLib;
using PestelLib.UI;
using UnityDI;
using PestelLib.Utils;

namespace PestelLib.Roulette
{
    public class TaskInitDependencyInjection : Task
    {
        override public void Run()
        {
            var container = new Container();
            ContainerHolder.Container = container;

            container.RegisterUnitySingleton<UpdateProvider>(null, true);
            container.RegisterUnitySingleton<CommandProcessor>(null, true);
            container.RegisterUnitySingleton<RouletteDefinitionsContainer>(null, true);
            container.RegisterSingleton<ILocalization, LocalizationData>(null);
            container.RegisterUnitySingleton<SharedTime>(null, true);

            container.RegisterUnitySingleton<ChestsRewardVisualizer>(null, true);
            container.RegisterUnitySingleton<SpritesDatabase>(null, true);
            
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<RouletteDefinitionsContainer>().SharedLogicDefs.ChestDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<RouletteDefinitionsContainer>().SharedLogicDefs.ChestsRewardDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<RouletteDefinitionsContainer>().SharedLogicDefs.ChestsRewardPoolDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<RouletteDefinitionsContainer>().SharedLogicDefs.ChestsRewardPoolListDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<RouletteDefinitionsContainer>().SharedLogicDefs.PirateBoxDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<RouletteDefinitionsContainer>().SharedLogicDefs.PirateBoxChestDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<RouletteDefinitionsContainer>().SharedLogicDefs.SettingDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<RouletteDefinitionsContainer>().SharedLogicDefs.SettingDefDict);
            
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ISharedLogic>().GetModule<RouletteModule>());
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ISharedLogic>().GetModule<ChestModule>());
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ISharedLogic>().GetModule<RandomModule>());
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ISharedLogic>().GetModule<RouletteEventsModule>());
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ISharedLogic>().GetModule<SettingsModuleBase>());

            container.RegisterUnitySingleton<Gui>(null, true);

            OnComplete(this);
        }

    }
}