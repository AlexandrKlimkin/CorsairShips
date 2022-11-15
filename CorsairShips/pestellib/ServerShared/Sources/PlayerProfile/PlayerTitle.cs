using System;
using MessagePack;

namespace ServerShared.PlayerProfile
{
    [MessagePackObject()]
    public class PlayerTitle
    {
        [Key(0)]
        public DateTime Time;
        [Key(1)]
        public string DefId;
    }
}