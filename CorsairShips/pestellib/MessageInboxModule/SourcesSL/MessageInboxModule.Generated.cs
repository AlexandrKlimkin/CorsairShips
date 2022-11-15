using PestelLib.SharedLogic.Modules; 
namespace S{
    [MessagePack.MessagePackObject]
    public class MessageInboxModule_GetEarliestMessage {}

    [MessagePack.MessagePackObject]
    public class MessageInboxModule_DeleteAllMessages {}

    [MessagePack.MessagePackObject]
    public class MessageInboxModule_MarkAsRead {
        [MessagePack.Key(1)]
        public long[] serialIds { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class MessageInboxModule_SetWelcomeLetter {
        [MessagePack.Key(1)]
        public long serialId { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class MessageInboxModule_ReplaceMessages {
        [MessagePack.Key(1)]
        public long[] serialIds { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class MessageInboxModule_ClaimCustomReward {
        [MessagePack.Key(1)]
        public long serialId { get; set;}

        [MessagePack.Key(2)]
        public string id { get; set;}

        [MessagePack.Key(3)]
        public int amount { get; set;}
	}

    [MessagePack.MessagePackObject]
    public class MessageInboxModule_ClaimChestReward {
        [MessagePack.Key(1)]
        public long serialId { get; set;}

        [MessagePack.Key(2)]
        public string chestRewardId { get; set;}
	}


}