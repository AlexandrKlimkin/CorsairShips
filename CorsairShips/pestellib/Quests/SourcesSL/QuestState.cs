using MessagePack;
using System.Collections.Generic;
using PestelLib.SharedLogic.Modules;

namespace S
{
    [MessagePackObject()]
    public class QuestState
    {
        [Key(0)]
        public string Id;

        [Key(1)]
        public long Timestamp;

        [Key(2)]
        public bool CompleteRegistred;

        [Key(3)]
        public bool RevenueUsed;

        [Key(4)]
        public int Completed;

        [Key(6)]
        public List<ChestsRewardDef> Rewards = new List<ChestsRewardDef>();
        
        [Key(7)]
        public string AdditionalQuestParameter;
    }
}
