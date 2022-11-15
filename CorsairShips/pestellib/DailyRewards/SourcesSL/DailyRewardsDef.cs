using System;

namespace PestelLib.SharedLogic.Modules
{
    [Serializable]
    public class DailyRewardsDef
    {
        public string Id;
        public string RewardId;
        public string Icon;
        public string Color;
        public int MonthIndex; // legacy
        public string PremiumRewardId;
        public int WeekIndex;
    }
}
