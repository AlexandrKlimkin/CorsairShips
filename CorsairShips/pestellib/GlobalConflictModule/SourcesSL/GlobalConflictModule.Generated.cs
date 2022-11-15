namespace S{
    [MessagePack.MessagePackObject]
    public class GlobalConflictModuleBase_ClaimReward {
        [MessagePack.Key(1)]
        public ConflictResult result { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class GlobalConflictModuleBase_Update {}

    [MessagePack.MessagePackObject]
    public class GlobalConflictModuleBase_UpdateAndGetQuests {}

    [MessagePack.MessagePackObject]
    public class GlobalConflictModuleBase_DeployQuest {}

    [MessagePack.MessagePackObject]
    public class GlobalConflictModuleBase_RecalcEnergy {}

    [MessagePack.MessagePackObject]
    public class GlobalConflictModuleBase_BattleForNode {
        [MessagePack.Key(1)]
        public int nodeId { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class GlobalConflictModuleBase_RegisterResult {
        [MessagePack.Key(1)]
        public ConflictResult result { get; set;}
	}


}