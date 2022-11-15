using System.Collections.Generic;
using MessagePack;

namespace PestelLib.SharedLogic.Modules
{
    [MessagePackObject]
    public class DailyRewardState
    {
        [Key(0)]
        public long LastRewardTime;

        [Key(1)]
        public int RewardStreak;

        [Key(2)]
        public int RewardClaimPrem;

        [Key(3)]
        public int WeekIndex;
    }
}
