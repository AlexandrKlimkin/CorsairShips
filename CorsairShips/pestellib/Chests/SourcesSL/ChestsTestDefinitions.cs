using System.Collections.Generic;
using PestelLib.Serialization;
using PestelLib.SharedLogic.Modules;

namespace PestelLib.SharedLogic
{
    [System.Serializable]
    public class ChestsTestDefinitions
    {

#region AUTO_GENERATED_DEFINITIONS
        [GooglePageRef("ChestRewards")]
        public List<ChestsRewardDef> ChestsRewardDefs = new List<ChestsRewardDef>();
        public Dictionary<string, ChestsRewardDef> ChestsRewardDefDict = new Dictionary<string, ChestsRewardDef>();
        [GooglePageRef("ChestRewardsPool")]
        public List<ChestsRewardPoolDef> ChestsRewardPoolDefs = new List<ChestsRewardPoolDef>();
        public Dictionary<string, ChestsRewardPoolDef> ChestsRewardPoolDefDict = new Dictionary<string, ChestsRewardPoolDef>();
        [GooglePageRef("Chests")]
        public List<ChestDef> ChestDefs = new List<ChestDef>();
        public Dictionary<string, ChestDef> ChestDefDict = new Dictionary<string, ChestDef>();
        [GooglePageRef("ChestRewardsPoolList")]
        public List<ChestsRewardPoolListDef> ChestsRewardPoolListDefs = new List<ChestsRewardPoolListDef>();
        public Dictionary<string, ChestsRewardPoolListDef> ChestsRewardPoolListDefDict = new Dictionary<string, ChestsRewardPoolListDef>();
        #endregion

        public void OnAfterDeserialize()
        {
#region AUTO_GENERATED_DICT_INIT
            ChestsRewardDefDict.Clear();
            for (int i = 0; i < ChestsRewardDefs.Count; i++)
            {
                var def = ChestsRewardDefs[i];
                ChestsRewardDefDict.Add(def.Id, def);
            }
            ChestDefDict.Clear();
            for (int i = 0; i < ChestDefs.Count; i++)
            {
                var def = ChestDefs[i];
                ChestDefDict.Add(def.Id, def);
            }
#endregion
        }
    }
}
