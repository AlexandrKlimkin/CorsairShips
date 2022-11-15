using System;
using ServerShared.GlobalConflict;

namespace GlobalConflictModule.Scripts
{
    [Serializable]
    public class DonationLevelBonusDesc
    {
        public DonationBonusType ServerType;
        public string ClientType;
        public float BonusValue;
    }
}