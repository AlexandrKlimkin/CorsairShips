using PestelLib.Chests;
using PestelLib.Localization;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicBase;
using PestelLib.SharedLogicClient;
using PestelLib.TaskQueueLib;
using PestelLib.UI;
using UnityDI;
using PestelLib.Utils;

namespace PestelLib.Chests
{
    public class TaskInitDependencyInjection : Task
    {
        override public void Run()
        {
            var container = new Container();
            ContainerHolder.Container = container;

            container.RegisterUnitySingleton<UpdateProvider>(null, true);
            container.RegisterUnitySingleton<ChestExampleGame>(null, true);
            container.RegisterUnitySingleton<CommandProcessor>(null, true);
            container.RegisterUnitySingleton<ChestDefinitionsContainer>(null, true);
            container.RegisterUnitySingleton<SpritesDatabase>(null, true);

            container.RegisterSingleton<ILocalization, LocalizationData>(null);

            container.RegisterCustom<IChestsConcreteGameInterface>(() => ContainerHolder.Container.Resolve<ChestExampleGame>());

            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ChestDefinitionsContainer>().SharedLogicDefs.ChestDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ChestDefinitionsContainer>().SharedLogicDefs.ChestsRewardDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ChestDefinitionsContainer>().SharedLogicDefs.ChestsRewardPoolDefs);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ChestDefinitionsContainer>().SharedLogicDefs.ChestsRewardPoolListDefs);

            container.RegisterCustom(() => ContainerHolder.Container.Resolve<ISharedLogic>().GetModule<ChestModule>());

            container.RegisterUnitySingleton<Gui>(null, true);

            OnComplete(this);
        }

    }
}