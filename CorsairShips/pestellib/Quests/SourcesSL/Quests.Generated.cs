using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class QuestEventsModule_UpdateEvent {}

    [MessagePack.MessagePackObject]
    public class QuestModule_IncrementQuestByTypeSL {
        [MessagePack.Key(1)]
        public string questType { get; set;}

        [MessagePack.Key(2)]
        public int increment { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class QuestModule_IncrementQuestById {
        [MessagePack.Key(1)]
        public string questId { get; set;}

        [MessagePack.Key(2)]
        public int delta { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class QuestModule_GiveQuestRevenue {
        [MessagePack.Key(1)]
        public string questId { get; set;}

        [MessagePack.Key(2)]
        public bool doubleRevenue { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class QuestModule_RemoveQuest {
        [MessagePack.Key(1)]
        public string questId { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class QuestModule_CleanAbsentQuests {}

    [MessagePack.MessagePackObject]
    public class QuestModule_SetAdsRevenueX2TimeStamp {}

    [MessagePack.MessagePackObject]
    public class QuestModule_RerollQuest {
        [MessagePack.Key(1)]
        public string questId { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class QuestModule_QuestWatchAds {
        [MessagePack.Key(1)]
        public string id { get; set;}
	}


}