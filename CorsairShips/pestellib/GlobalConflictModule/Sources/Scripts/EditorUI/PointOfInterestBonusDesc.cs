using System;
using ServerShared.GlobalConflict;

namespace GlobalConflictModule.Scripts
{
    [Serializable]
    public class PointOfInterestBonusDesc
    {
        public string ClientType;
        public PointOfInterestServerLogic ServerType;
        public float Value;
    }

    
}
