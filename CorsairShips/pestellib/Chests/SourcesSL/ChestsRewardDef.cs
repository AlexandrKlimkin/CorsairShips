using MessagePack;

namespace PestelLib.SharedLogic.Modules
{
    [System.Serializable]
    [MessagePackObject]
    public class ChestsRewardDef
    {
        [Key(0)]
        public string Id;
        [Key(1)]
        public string Description;
        [Key(2)]
        public string ItemType;
        [Key(3)]
        public int Amount;
        [Key(4)]
        public string ItemId;
        [Key(5)]
        public bool UseRarityAndTier;
        [Key(6)]
        public int Rarity;
        [Key(7)]
        public int Tier;
        [Key(8)]
        public bool UseMaxPlayerTier;
        [Key(9)]
        public string Tag;
        [Key(10)]
        public string RewardSource;

        [Key(11)]
        public int CellId;

        public ChestsRewardDef Copy() {
            return MemberwiseClone() as ChestsRewardDef;
        }
    }
}