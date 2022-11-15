using MessagePack;

namespace S
{
    [MessagePackObject()]
    public class QuestStats
    {
        [Key(0)]
        public int BuyItem;
        [Key(1)]
        public int BuyNewShip;
        [Key(2)]
        public int BuyEmblem;
        [Key(3)]
        public int UpgradeShip;
    }
}