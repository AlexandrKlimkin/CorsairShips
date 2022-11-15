using System;
using System.Collections.Generic;
using System.Text;
using FriendsClient;
using FriendsClient.Private;
using FriendsClient.Sources;
using Moq;
using S;

namespace FriendsServerTest
{
    static class FriendClientMockExtensions
    {

        public static void SetupLobbyInviteCallback(this Mock<IFriendsClientPrivate> client, RoomResult result)
        {
            client.Setup(_ => _.InviteRoom(It.IsAny<long>(), It.IsAny<MadId>(),
                    It.IsAny<FriendsDelegate.RoomAnswerCallback>()))
                .Callback<long, MadId, FriendsDelegate.RoomAnswerCallback>((l, id, evt) =>
                {
                    evt(l, result);
                });
        }

        public static void RaiseFriendInviteAccept(this Mock<IFriendsClientPrivate> client, MadId friendId, long inviteId)
        {
            client.Raise(_ => _.OnInviteAccepted += null, new FriendsInviteEventMessage() { From = friendId, Event = FriendEvent.FriendAccept, InviteId = inviteId });
        }

        public static void RaiseFriendInviteReject(this Mock<IFriendsClientPrivate> client, MadId friendId, long inviteId)
        {
            client.Raise(_ => _.OnInviteRejected += null, new FriendsInviteEventMessage() { From = friendId, Event = FriendEvent.FriendReject, InviteId = inviteId });
        }

        public static void RaiseRoomInviteReject(this Mock<IFriendsClientPrivate> client, long roomId, MadId frienId)
        {
            client.Raise(_ => _.OnRoomReject += null, new RoomInviteEventMessage() { RoomId = roomId, Event = FriendEvent.RoomReject, From = frienId });
        }

        public static void RaiseRoomInviteAccept(this Mock<IFriendsClientPrivate> client, long rooomId, MadId frienId)
        {
            client.Raise(_ => _.OnRoomAccept += null, new RoomInviteEventMessage() { RoomId = rooomId, Event = FriendEvent.RoomAccept, From = frienId });
        }

        public static void RaiseRoomLeave(this Mock<IFriendsClientPrivate> client, long roomId, MadId friendId)
        {
            client.Raise(_ => _.OnRoomLeave += null, new RoomLeaveKickEventMessage() { RoomId = roomId, Event = FriendEvent.RoomLeave, FriendId = friendId });
        }

        public static void RaiseRoomKick(this Mock<IFriendsClientPrivate> client, long roomId, MadId friendId)
        {
            client.Raise(_ => _.OnRoomKick += null, new RoomLeaveKickEventMessage() { RoomId = roomId, Event = FriendEvent.RoomLeave, FriendId = friendId });
        }

        public static void RaiseRoomJoin(this Mock<IFriendsClientPrivate> client, long roomId, FriendBase friend)
        {
            client.Raise(_ => _.OnRoomJoin += null, new RoomJoinEventMessage() { RoomId = roomId, Event = FriendEvent.RoomJoin, Friend = friend });
        }

        public static void RaiseFriendStatusChanged(this Mock<IFriendsClientPrivate> client, MadId from, int status)
        {
            client.Raise(_ => _.OnFriendStatus += null, new FriendStatusChangedMessage() { Event = FriendEvent.StatusChanged, From = from, StatusCode = status});
        }
    }
}
