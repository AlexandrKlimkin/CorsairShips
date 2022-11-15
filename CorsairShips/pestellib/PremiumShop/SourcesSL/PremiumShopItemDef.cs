using System;

namespace PestelLib.SharedLogic.Modules
{
    [Serializable]
    public class PremiumShopItemDef
    {
        public string Id;
        public string TabId;
        public string Name;
        public string Description;
        public string SkuId;
        public string Icon;
        public string ClaimGroupId;
        public int Discount;
        public string ItemType;
    }
}
