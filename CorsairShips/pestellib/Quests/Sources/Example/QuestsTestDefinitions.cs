using System.Collections.Generic;
using PestelLib.SharedLogic.Defs;
using PestelLib.SharedLogic.Modules;
using UnityEngine;

namespace PestelLib.Quests
{
    [CreateAssetMenu(fileName = "Data", menuName = "Chest definitions", order = 1)]
    [System.Serializable]
    public class QuestsTestDefinitions : ISerializationCallbackReceiver
    {
        public List<LocalizationDef> Localization;

        public List<QuestDef> Quests;
        
        public Dictionary<string, QuestDef> QuestsDict = new Dictionary<string, QuestDef>();

        public List<ChestsRewardDef> ChestsRewardDefs;

        public Dictionary<string, ChestsRewardDef> ChestsRewardDefDict = new Dictionary<string, ChestsRewardDef>();

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            foreach (var questDef in Quests)
            {
                QuestsDict[questDef.Id] = questDef;
            }

            foreach (var chestsRewardDef in ChestsRewardDefs)
            {
                ChestsRewardDefDict[chestsRewardDef.Id] = chestsRewardDef;
            }
        }
    }
}