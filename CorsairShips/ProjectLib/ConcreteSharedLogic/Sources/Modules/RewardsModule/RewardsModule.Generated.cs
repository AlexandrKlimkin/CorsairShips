using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class RewardsModule_ClaimRewards {
        [MessagePack.Key(1)]
        public Match_Result matchResult { get; set;}

        [MessagePack.Key(2)]
        public int points { get; set;}
	}


}