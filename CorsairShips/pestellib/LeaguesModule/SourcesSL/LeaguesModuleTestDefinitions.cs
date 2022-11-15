using System.Collections.Generic;
using PestelLib.Serialization;
using PestelLib.SharedLogic.Modules;

namespace PestelLib.SharedLogic
{
    [System.Serializable]
    public class LeaguesModuleTestDefinitions
    {
        #region AUTO_GENERATED_DEFINITIONS
        [GooglePageRef("Leagues")]
        public List<LeagueDef> LeagueDefs;
        [GooglePageRef("LeagueRewards")]
        public List<LeagueRewardDef> LeagueRewardDefs;
        [GooglePageRef("ChestRewards")]
        public List<ChestsRewardDef> RewardDefs;
        [GooglePageRef("ChestRewardsPool")]
        public List<ChestsRewardPoolDef> RewardPoolDefs;
        [GooglePageRef("Chests")]
        public List<ChestDef> ChestDefs;
        [GooglePageRef("ChestRewardsPoolList")]
        public List<ChestsRewardPoolListDef> RewardPoolListDefs;
        [GooglePageRef("Settings")]
        public List<SettingDef> SettingDefs = new List<SettingDef>();
        public Dictionary<string, SettingDef> SettingDefDict = new Dictionary<string, SettingDef>();
        #endregion

        public void OnAfterDeserialize()
        {
            #region AUTO_GENERATED_DICT_INIT
            SettingDefDict.Clear();
            for (int i = 0; i < SettingDefs.Count; i++)
            {
                var def = SettingDefs[i];
                SettingDefDict.Add(def.Id, def);
            }
            #endregion
        }
    }
}