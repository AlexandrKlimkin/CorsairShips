using FriendsClient;
using FriendsClient.Lobby;
using FriendsClient.Private;
using FriendsClient.Sources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using S;

namespace FriendsServerTest
{
    [TestClass]
    public class InvitedFriendTests
    {
        private long _roomId = 1;
        private Mock<IFriendsClientPrivate> _mFriendClient;
        private Mock<IFriendsRoom> _mRoom;
        private InvitedFriend _invitedFriend;
        private FriendBase _friend = new FriendBase()
        {
            Id = new MadId(1)
        };
        private int _kicked;
        private int _status;
        private int _leave;

        [TestInitialize]
        public void Init()
        {
            _kicked = 0;
            _status = 0;
            _leave = 0;
            _mFriendClient = new Mock<IFriendsClientPrivate>();
            _mRoom = new Mock<IFriendsRoom>();
            _mRoom.Setup(_ => _.RoomId).Returns(_roomId);
            _mRoom.Setup(_ => _.ImHost).Returns(true);
            _invitedFriend = new InvitedFriend(_mFriendClient.Object, _mRoom.Object, _friend);
            _invitedFriend.OnKicked += (_) => ++_kicked;
            _invitedFriend.OnStatusChanged += (_, s) => ++_status;
            _invitedFriend.OnLeave += (_) => ++_leave;

            _mFriendClient.Setup(_ => _.KickRoom(_roomId, _friend.Id, It.IsAny<FriendsDelegate.RoomAnswerCallback>()))
                .Callback<long, MadId, FriendsDelegate.RoomAnswerCallback>(
                    (l, id, cb) => { cb(l, RoomResult.Success); });
        }

        [TestMethod]
        public void TestInvitedFriendLogic()
        {
            Assert.AreEqual(RoomResult.None, _invitedFriend.KickResult);
            _invitedFriend.Kick(null);
            _mFriendClient.Verify(_ => _.KickRoom(_roomId, _friend.Id, It.IsAny<FriendsDelegate.RoomAnswerCallback>()), Times.Once);
            Assert.AreEqual(RoomResult.Success, _invitedFriend.KickResult);
            Assert.AreEqual(1, _kicked);
        }

        [TestMethod]
        public void TestNotHostNoKick()
        {
            _mRoom.Setup(_ => _.ImHost).Returns(false);
            _invitedFriend.Kick(null);
            _mFriendClient.Verify(_ => _.KickRoom(It.IsAny<long>(), It.IsAny<MadId>(), It.IsAny<FriendsDelegate.RoomAnswerCallback>()), Times.Never);
            Assert.AreEqual(RoomResult.NotAllowed, _invitedFriend.KickResult);
            Assert.AreEqual(0, _kicked);
        }

        [TestMethod]
        public void TestStatusChanged()
        {
            _mFriendClient.RaiseFriendStatusChanged(_invitedFriend.FriendInfo.Id, FriendStatus.Online);
            Assert.AreEqual(FriendStatus.Online, _invitedFriend.FriendInfo.Status);

            _mFriendClient.RaiseFriendStatusChanged(_invitedFriend.FriendInfo.Id, FriendStatus.Offline);
            Assert.AreEqual(FriendStatus.Offline, _invitedFriend.FriendInfo.Status);
            Assert.AreEqual(2, _status);
        }

        [TestMethod]
        public void TestLeave()
        {
            _mFriendClient.RaiseRoomLeave(_roomId, _friend.Id);
            Assert.AreEqual(1, _leave);
        }

        [TestMethod]
        public void TestEventFiltering()
        {
            var alienUser = new MadId((uint) _invitedFriend.FriendInfo.Id + 1);

            _mFriendClient.RaiseFriendStatusChanged(alienUser, FriendStatus.InRoom);
            _mFriendClient.RaiseRoomLeave(_roomId, alienUser);
            Assert.AreEqual(0, _status);
            Assert.AreEqual(0, _leave);
            Assert.AreEqual(FriendStatus.Offline, _invitedFriend.FriendInfo.Status);
        }
    }
}
