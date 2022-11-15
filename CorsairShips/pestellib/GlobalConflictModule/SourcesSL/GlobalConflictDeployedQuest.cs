using MessagePack;

namespace PestelLib.SharedLogic.Modules
{
    [MessagePackObject]
    public class GlobalConflictDeployedQuest
    {
        [Key(0)]
        public string QuestId;
        [Key(1)]
        public int Progress;
        [Key(2)]
        public int NodeId;
        [Key(3)]
        public long DeployTime;
        [Key(4)]
        public bool Expired;
        [Key(5)]
        public int Level;
        [Key(6)]
        public bool Completed;
    }
}