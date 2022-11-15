using System;
using MessagePack;

namespace ClansClientLib
{
    [MessagePackObject()]
    public class ClanRequestRecord
    {
        [Key(0)]
        public Guid Id;
        [Key(1)]
        public DateTime Time;
        [Key(2)]
        public DateTime Expiry;
        [Key(3)]
        public Guid ClanId; // index
        [Key(4)]
        public Guid PlayerId;
        [Key(5)]
        public JoinRequestState Status;
    }
}