using System.Collections.Generic;
using PestelLib.Serialization;
using PestelLib.SharedLogic.Modules;

namespace PestelLib.SharedLogic
{
    [System.Serializable]
    public class RouletteTestDefinitions
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
        [GooglePageRef("PirateBoxes")]
        public List<PirateBoxDef> PirateBoxDefs = new List<PirateBoxDef>();
        public Dictionary<string, PirateBoxDef> PirateBoxDefDict = new Dictionary<string, PirateBoxDef>();
        [GooglePageRef("PirateBoxChests")]
        public List<PirateBoxChestDef> PirateBoxChestDefs = new List<PirateBoxChestDef>();
        public Dictionary<string, PirateBoxChestDef> PirateBoxChestDefDict = new Dictionary<string, PirateBoxChestDef>();
        [GooglePageRef("Settings")]
        public List<SettingDef> SettingDefs = new List<SettingDef>();
        public Dictionary<string, SettingDef> SettingDefDict = new Dictionary<string, SettingDef>();
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
            PirateBoxDefDict.Clear();
            for (int i = 0; i < PirateBoxDefs.Count; i++)
            {
                var def = PirateBoxDefs[i];
                PirateBoxDefDict.Add(def.Id, def);
            }
            PirateBoxChestDefDict.Clear();
            for (int i = 0; i < PirateBoxChestDefs.Count; i++)
            {
                var def = PirateBoxChestDefs[i];
                PirateBoxChestDefDict.Add(def.Id, def);
            }

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
