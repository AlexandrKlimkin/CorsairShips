using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class PropertyModule_BuyProperty {
        [MessagePack.Key(1)]
        public string propertyId { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class PropertyModule_SetPropertyAsOwned {
        [MessagePack.Key(1)]
        public string propertyId { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class PropertyModule_SetSpotted {
        [MessagePack.Key(1)]
        public string id { get; set;}
	}


}