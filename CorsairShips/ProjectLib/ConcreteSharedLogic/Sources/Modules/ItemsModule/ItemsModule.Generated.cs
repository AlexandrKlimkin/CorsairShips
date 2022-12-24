using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class ItemsModule_AddItem {
        [MessagePack.Key(1)]
        public string itemId { get; set;}

        [MessagePack.Key(2)]
        public int count { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class ItemsModule_SpendItem {
        [MessagePack.Key(1)]
        public string itemId { get; set;}

        [MessagePack.Key(2)]
        public int count { get; set;}
	}


}