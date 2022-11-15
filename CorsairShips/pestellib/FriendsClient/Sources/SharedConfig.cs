using System;
using MessagePack;

namespace FriendsClient
{
    [MessagePackObject()]
    public struct SharedConfig
    {
        [Key(0)]
        public int MaxFriends;
        [Key(1)]
        public int MaxInvites;
        [Key(2)]
        public int MaxRoomParty;
        [Key(3)]
        public TimeSpan InviteTTL;
        [Key(4)]
        public TimeSpan RoomInviteTTL;
        [Key(5)]
        public TimeSpan GiftCooldown;
        [Key(6)]
        public bool UnlockGiftsAtMidnight;
        [Key(7)]
        public bool MadIdMixed;
        [Key(8)]
        public int GiftsStackSize;
        [Key(9)]
        public bool DontCloseRoomOnBattleStart;
        [Key(10)]
        public bool RoomInviteAnybody;
    }
}
