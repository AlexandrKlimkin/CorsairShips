using FriendsClient;

namespace FriendsServer
{
    static class SharedConfigExtension
    {
        public static SharedConfig MakeSharedConfig(this ServerConfig config)
        {
            return new SharedConfig()
            {
                MaxFriends = config.MaxFriends,
                MaxInvites = config.MaxInvites,
                MaxRoomParty = config.MaxRoomParty,
                InviteTTL = config.InviteTTL,
                RoomInviteTTL = config.RoomInviteTTL,
                GiftCooldown = config.GiftCooldown,
                UnlockGiftsAtMidnight = config.UnlockGiftsAtMidnight,
                MadIdMixed = config.MadIdMixed,
                GiftsStackSize = config.GiftsStackSize,
                DontCloseRoomOnBattleStart = config.DontCloseRoomOnBattleStart,
                RoomInviteAnybody = config.RoomInviteAnybody
            };
        }
    }
}
