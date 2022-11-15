using System;
using MessagePack;

namespace ServerShared.PlayerProfile
{
    [MessagePackObject()]
    public class Achievement
    {
        [Key(0)]
        public DateTime Time;
        [Key(1)]
        public string DefId;
        [Key(2)]
        public int SlotId;
    }
}