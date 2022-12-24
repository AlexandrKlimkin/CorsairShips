using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class CurrencyModule_AddCurrency {
        [MessagePack.Key(1)]
        public CurrencyType currencyType { get; set;}

        [MessagePack.Key(2)]
        public int count { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class CurrencyModule_SpendCurrency {
        [MessagePack.Key(1)]
        public CurrencyType currencyType { get; set;}

        [MessagePack.Key(2)]
        public int count { get; set;}
	}


}