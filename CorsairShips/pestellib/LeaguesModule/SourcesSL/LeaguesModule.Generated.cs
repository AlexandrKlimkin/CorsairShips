using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class LeaguesModule_ClaimRewards {
        [MessagePack.Key(1)]
        public SeasonEndInfo seasonEndInfo { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class LeaguesModule_ScoreSL {
        [MessagePack.Key(1)]
        public long score { get; set;}
	}


}