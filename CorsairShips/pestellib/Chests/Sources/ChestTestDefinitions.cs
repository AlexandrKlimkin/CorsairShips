using System.Collections.Generic;
using PestelLib.SharedLogic.Modules;
using UnityEngine;

namespace PestelLib.Chests
{
    [CreateAssetMenu(fileName = "Data", menuName = "Chest definitions", order = 1)]
    [System.Serializable]
    public class ChestTestDefinitions : ScriptableObject
    {
        private static ChestTestDefinitions _instance;
        public static ChestTestDefinitions Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ChestTestDefinitions>("ChestTestDefinitions");
                }
                return _instance;
            }
        }

        public List<ChestsRewardDef> RewardDefs;

        public List<ChestsRewardPoolDef> RewardPoolDefs;

        public List<ChestsRewardPoolListDef> RewardPoolListDefs;

        public List<ChestDef> Chests;
    }
}