using PestelLib.Localization;
using PestelLib.SharedLogicClient;
using PestelLib.TaskQueueLib;
using PestelLib.UI;
using UnityDI;
using PestelLib.Utils;

namespace PestelLib.PremiumShop
{
    public class TaskInitDependencyInjection : Task
    {
        override public void Run()
        {
            var container = new Container();
            ContainerHolder.Container = container;

            //common stuff for every game
            container.RegisterUnitySingleton<Gui>(null, true);
            container.RegisterUnitySingleton<UpdateProvider>(null, true);
            container.RegisterUnitySingleton<CommandProcessor>(null, true);
            container.RegisterUnitySingleton<SpritesDatabase>(null, true);
            container.RegisterSingleton<ILocalization, LocalizationData>(null);
            
            //IAP
            container.RegisterUnitySingleton<Purchaser>(null, true);
            container.RegisterUnitySingleton<PurchaserSettings>(null, true);

            //IAP user interface (optional)
            container.RegisterUnitySingleton<PremiumShopDefinitionsContainer>(null, true);
            container.RegisterCustom(() => ContainerHolder.Container.Resolve<PremiumShopDefinitionsContainer>().SharedLogicDefs.PremiumShopDefs);

            OnComplete(this);
        }

    }
}