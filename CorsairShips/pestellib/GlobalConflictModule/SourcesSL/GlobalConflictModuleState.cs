using System.Collections.Generic;
using MessagePack;
using S;

namespace PestelLib.SharedLogic.Modules
{
    [MessagePackObject]
    public class GlobalConflictModuleState
    {
        [Key(0)]
        public long LastEnergyRecalcTime;
        [Key(1)]
        public int Energy;
        [Key(2)]
        public List<ConflictResult> ConflictResults;
        [Key(3)]
        public List<string> ClaimedRewards;
        [Key(4)]
        public int QuestLevel;
        [Key(5)]
        public List<GlobalConflictDeployedQuest> Quests;
        [Key(6)]
        public List<string> VisitedConflicts;
    }
}
