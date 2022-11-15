using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class ChestModule_GiveChestById {
        [MessagePack.Key(1)]
        public string chestId { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class ChestModule_GiveChestByRarity {
        [MessagePack.Key(1)]
        public int chestRarity { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class ChestModule_GiveByRewardId {
        [MessagePack.Key(1)]
        public string rewardId { get; set;}
	}


}