using System;
using System.Collections;
using System.Collections.Generic;
using PestelLib.SharedLogic.Modules;
using UnityEngine;

namespace GlobalConflict.Example
{
    [Serializable]
    public class GlobConflictTestDefinitions : MonoBehaviour
    {
        public List<ConflictLeaderboardRewardDef> LeaderboardRewardDefs = new List<ConflictLeaderboardRewardDef>()
        {
            new ConflictLeaderboardRewardDef() {RewardId = "FakeWinReward", Sector = 0}
        };
        public List<ConflictTeamRewardDef> TeamRewardDefs = new List<ConflictTeamRewardDef>()
        {
            new ConflictTeamRewardDef() {RewardId = "TeamWinReward", ForLoser = false},
            new ConflictTeamRewardDef() {RewardId = "TeamLoseReward", ForLoser = true}
        };
    }
}
