using System;

namespace PestelLib.SharedLogic.Modules
{
    [Serializable]
    public class ConflictLeaderboardRewardDef
    {
        public string ConflictId;
        public int Sector;
        public string RewardId;
        public bool ForLoser;
    }
}
