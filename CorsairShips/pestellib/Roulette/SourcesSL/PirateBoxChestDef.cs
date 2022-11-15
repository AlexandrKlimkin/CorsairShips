using System;

namespace PestelLib.SharedLogic.Modules
{
    [Serializable]
    public class PirateBoxChestDef
    {
        public string Id;
        public string Name;
        public int Rarity;
        public bool ForFree;
        public float ProgressNormalized;
        public int CostKeys;
        public int BonusSoftMin;
        public int BonusSoftMax;
        public string DescriptionKey;
        public string PirateBoxId;
    }
}
