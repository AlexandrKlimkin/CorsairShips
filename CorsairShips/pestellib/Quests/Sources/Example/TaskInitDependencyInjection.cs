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

namespace PestelLib.Quests
{
    public class TaskInitDependencyInjection : Task
    {
        //private static ChestModule _chestModule = null;

        override public void Run()
        {
            var container = new Container();
            ContainerHolder.Container = container;

            container.RegisterUnitySingleton<UpdateProvider>(null, true);
            container.RegisterUnitySingleton<CommandProcessor>(null, true);
            container.RegisterUnitySingleton<QuestsDefinitionsContainer>(null, true);
            container.RegisterUnitySingleton<SharedTime>(null, true);
            container.RegisterUnitySingleton<ChestsRewardVisualizer>(null, true);
            container.RegisterUnitySingleton<SpritesDatabase>(null, true);

            container.RegisterSingleton<ILocalization, LocalizationData>(null);

            //container.RegisterCustom<IChestsConcreteGameInterface>(() => ContainerHolder.Container.Resolve<ChestExampleGame>());

            container.RegisterCustom(() => ContainerHolder.Container.Resolve<QuestsDefinitionsContainer>().SharedLogicDefs.Quests);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<QuestsDefinitionsContainer>().SharedLogicDefs.QuestsDict);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<QuestsDefinitionsContainer>().SharedLogicDefs.Localization);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<QuestsDefinitionsContainer>().SharedLogicDefs.ChestsRewardDefDict);

            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ISharedLogic>().GetModule<QuestModule>());
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ISharedLogic>().GetModule<QuestEventsModule>());

            container.RegisterUnitySingleton<Gui>(null, true);

            OnComplete(this);
        }

    }
}