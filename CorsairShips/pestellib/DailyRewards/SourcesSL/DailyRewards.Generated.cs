using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class DailyRewardModule_ResetRewards {}

    [MessagePack.MessagePackObject]
    public class DailyRewardModule_ClaimRewards {
        [MessagePack.Key(1)]
        public bool withBonus { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class DailyRewardModule_Refresh {}


}