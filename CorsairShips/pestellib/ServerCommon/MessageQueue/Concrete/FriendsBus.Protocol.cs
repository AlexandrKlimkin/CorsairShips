using MessagePack;

namespace FriendsServer.Bus
{
    public enum BusMessageType
    {
        StatusChanged,
        FriendInvite,
        FriendInviteAnswered,
        FriendInviteCanceled,
        FriendRemove,
        Gift,
        GiftClaim,
        ProfileUpdate,
        PlayerDelete
    }

    [MessagePackObject()]
    public class FriendsBusMessage
    {
        [Key(0)]
        public int Type;
        [Key(1)]
        public byte[] Data;
    }
}
