using FriendsClient.Private;
using MessagePack;
using S;

namespace FriendsServer.Bus
{

    [MessagePackObject()]
    public class FriendEventMessageGlobal
    {
        [Key(0)]
        public MadId Target;
        [Key(1)]
        public FriendEventMessage Message;
    }

    [MessagePackObject]
    public class FriendsInviteEventGlobal
    {
        [Key(0)]
        public MadId Target;
        [Key(1)]
        public FriendsInviteEventMessage Message;
    }

    [MessagePackObject()]
    public class FriendGiftEventMessageGlobal
    {
        [Key(0)]
        public MadId Target;
        [Key(1)]
        public FriendGiftEventMessage Message;
    }
}
