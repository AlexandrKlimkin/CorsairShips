using System;
using System.Collections;
using PestelLib.Localization;
using PestelLib.PremiumShop;
using PestelLib.SharedLogic;
using PestelLib.SharedLogicBase;
using PestelLib.SharedLogicClient;
using PestelLib.UI;
using S;
using UnityDI;
using UnityEngine;

public class SuperSimpleInapp : MonoBehaviour {

    IEnumerator Start () {
	    var container = new Container();
	    ContainerHolder.Container = container;

        //register IAPP services
        container.RegisterUnitySingleton<Purchaser>(null, true);
	    container.RegisterUnitySingleton<PurchaserSettings>(null, true);

        container.RegisterSingleton<LocalizationData>();
        container.RegisterCustom<ILocalization>(() => ContainerHolder.Container.Resolve<LocalizationData>());
        container.RegisterUnitySingleton<Gui>();

        //make and register empty shared logic
        container.RegisterUnitySingleton<CommandProcessor>(null, true);
        var userProfile = new UserProfile
        {
            UserId = Guid.NewGuid().ToByteArray()
        };
        var sharedLogic = new SharedLogicDefault<PremiumShopTestDefinitions>(userProfile, new PremiumShopTestDefinitions());
        ContainerHolder.Container.RegisterInstance<ISharedLogic>(sharedLogic);
        sharedLogic.OnLogMessage += delegate (string s) { Debug.Log("SL: " + s); };

        //make purchaser instance
        var purchaser = ContainerHolder.Container.Resolve<Purchaser>();

        //wait for IAPP initialization
        while (!purchaser.IsInitialized())
        {
            yield return null;
        }
	    
        //buy something
	    purchaser.BuyProductID("com.gdcompany.metalmadness.crystal_pack_4");
    }
}
