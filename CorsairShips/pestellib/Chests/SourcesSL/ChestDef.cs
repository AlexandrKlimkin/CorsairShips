using System;

namespace PestelLib.SharedLogic.Modules
{
    [Serializable]
    public class ChestDef
    {
        public string Id;
        public string Tag;
        public string Name;
        public string PoolListId;
        public int ChestRandomWeight;
        public int ChestRarity;
        public int OpenTimeSeconds;
        public int Level;
        public int ImmediatelyOpenCost;
    }
}