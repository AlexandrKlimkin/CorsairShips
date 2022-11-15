using System.Collections.Generic;
using MessagePack;

namespace PestelLib.SharedLogic.Modules
{
    [MessagePackObject]
    public class MessageInboxModuleState
    {
        [Key(0)]
        public List<long> UnreadMessages;
        [Key(1)]
        public List<long> ReadMessages;
        [Key(2)]
        public long LastSeenMessage;
        [Key(3)]
        public long EarliestStoredMessage;
        [Key(4)]
        public long StateBirthday;
        [Key(5)]
        public long WelcomeLetter;
    }
}
