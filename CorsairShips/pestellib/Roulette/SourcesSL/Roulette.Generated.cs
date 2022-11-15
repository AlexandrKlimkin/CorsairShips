using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class RouletteEventsModule_SetAdsTimeStamp {}

    [MessagePack.MessagePackObject]
    public class RouletteEventsModule_StartSeason {}

    [MessagePack.MessagePackObject]
    public class RouletteModule_AddKeys {
        [MessagePack.Key(1)]
        public int amount { get; set;}

        [MessagePack.Key(2)]
        public string source { get; set;}

        [MessagePack.Key(3)]
        public string itemId { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class RouletteModule_OpenBox {
        [MessagePack.Key(1)]
        public string boxId { get; set;}

        [MessagePack.Key(2)]
        public bool free { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class RouletteModule_OpenBoxTyped {
        [MessagePack.Key(1)]
        public string boxId { get; set;}

        [MessagePack.Key(2)]
        public string type { get; set;}
	}


}