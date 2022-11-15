namespace PestelLib.SharedLogic.Modules
{
    [System.Serializable]
    public class PirateBoxDef
    {
        public string Id;
        public string Name;
        public string PoolListId;
        public int Rarity;
        public string LockerId;
        public float BaitChance;
        public int BaitRarity;
        public float BaitShipChance;
        public bool IsSpecialBox;
        public int BonusSoftMin;
        public int BonusSoftMax;
        public string Icon;
    }
}
