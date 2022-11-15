using System.Collections.Generic;
using PestelLib.Serialization;
using PestelLib.SharedLogic.Modules;

namespace PestelLib.SharedLogic
{
    [System.Serializable]
    public class DailyRewardsTestDefinitions
    {
        #region AUTO_GENERATED_DEFINITIONS
        [GooglePageRef("DailyRewards")]
        public List<DailyRewardsDef> DailyRewardDefs;
        public Dictionary<string, DailyRewardsDef> DailyRewardDefDict = new Dictionary<string, DailyRewardsDef>();
        [GooglePageRef("ChestRewards")]
        public List<ChestsRewardDef> ChestRewardsDefs;
        public Dictionary<string, ChestsRewardDef> ChestRewardsDefsDict = new Dictionary<string, ChestsRewardDef>();
        #endregion

        public void OnAfterDeserialize()
        {
            #region AUTO_GENERATED_DICT_INIT
            DailyRewardDefDict.Clear();
            for (int i = 0; i < DailyRewardDefs.Count; i++)
            {
                var def = DailyRewardDefs[i];
                DailyRewardDefDict.Add(def.Id, def);
            }

            ChestRewardsDefsDict.Clear();
            for (int i = 0; i < ChestRewardsDefs.Count; i++)
            {
                var def = ChestRewardsDefs[i];
                ChestRewardsDefsDict.Add(def.Id, def);
            }
            #endregion
        }
    }
}
