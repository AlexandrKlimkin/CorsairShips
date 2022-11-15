using System;
using System.Collections.Generic;
using MessagePack;

namespace S
{
    [MessagePackObject()]
    public class QuestModuleState
    {
        [Key(0)]
        public List<QuestState> Quests = new List<QuestState>();

        [Key(1)]
        public QuestStats QuestStats = new QuestStats();

        [Key(2)]
        public int QuestRerollCounter;

        [Key(3)]
        public long LastQuestRerollTimestamp;

        [Key(4)]
        public long LastQuestRevenueX2Timestamp;
    }
}
