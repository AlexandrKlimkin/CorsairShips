using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class PremiumShopModule_SetDeveloperPayload {
        [MessagePack.Key(1)]
        public string developerPayload { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class PremiumShopModule_ClaimItem {
        [MessagePack.Key(1)]
        public string skuId { get; set;}

        [MessagePack.Key(2)]
        public UnityPurchaseReceipt receipt { get; set;}
	}


}