using FriendsClient;
using FriendsClient.FriendList;
using FriendsClient.Lobby;
using FriendsClient.Private;
using FriendsClient.Sources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using S;

namespace FriendsServerTest
{
    [TestClass]
    public class InviteableFriendTests
    {
        private static long _roomId = 1;
        private static MadId _friendId = new MadId(1);
        private Mock<IFriendList> _mFriendsList;
        private Mock<IFriendsClientPrivate> _mFriendsClient;
        private InviteableFriend _canInvite;
        private FriendBase _friend = new FriendBase()
        {
            Id = _friendId,
            Status = FriendStatus.Online
        };

        private FriendBase Me = new FriendBase()
        {
            Id = new MadId(2),
            Status = FriendStatus.Online
        };
        private int _accepted = 0;
        private int _rejected = 0;
        private int _status = 0;
        private int? _lastStatus = null;
        private RoomResult? _answer = null;
        private Mock<FriendsDelegate.RoomAnswerCallback> _mCallback;

        [TestInitialize]
        public void Init()
        {
            _accepted = 0;
            _rejected = 0;
            _status = 0;
            _lastStatus = null;
            _answer = null;
            _mFriendsList = new Mock<IFriendList>();
            _mFriendsList.Setup(_ => _.Me).Returns(Me);
            _mFriendsClient = new Mock<IFriendsClientPrivate>();
            _mFriendsClient.Setup(_ => _.FriendList).Returns(_mFriendsList.Object);
            _canInvite = new InviteableFriend(_mFriendsClient.Object, _roomId, _friend, false);
            _canInvite.OnAccepted += () => ++_accepted;
            _canInvite.OnRejected += () => ++_rejected;
            _canInvite.OnStatusChanged += (s) =>
            {
                ++_status;
                _lastStatus = s;
            };

            _mCallback = new Mock<FriendsDelegate.RoomAnswerCallback>();
            _mCallback.Setup(_ => _(It.IsAny<long>(), It.IsAny<RoomResult>())).Callback<long, RoomResult>((l, r) =>
            {
                _answer = r;
            });
        }

        [TestMethod]
        public void TestCanInviteFriendAccepted()
        {
            Assert.AreEqual(InviteStatus.None, _canInvite.InviteStatus);

            _canInvite.Invite(null);

            _mFriendsClient.Verify(_ => _.InviteRoom(_roomId, _friendId, It.IsAny<FriendsDelegate.RoomAnswerCallback>()), Times.Once);
            _mFriendsClient.RaiseRoomInviteAccept(_roomId, _friendId);

            Assert.AreEqual(InviteStatus.Accepted, _canInvite.InviteStatus);
            Assert.AreEqual(1, _accepted);
            Assert.AreEqual(0, _rejected);

            // dont invite twice
            _canInvite.Invite(null);
            _mFriendsClient.Verify(_ => _.InviteRoom(_roomId, _friendId, It.IsAny<FriendsDelegate.RoomAnswerCallback>()), Times.Once);
        }

        [TestMethod]
        public void TestCanInviteFriendReject()
        {
            _mFriendsClient.SetupLobbyInviteCallback(RoomResult.Success);

            Assert.AreEqual(InviteStatus.None, _canInvite.InviteStatus);

            _canInvite.Invite(_mCallback.Object);

            _mFriendsClient.Verify(_ => _.InviteRoom(_roomId, _friendId, It.IsAny<FriendsDelegate.RoomAnswerCallback>()), Times.Once);
            _mFriendsClient.RaiseRoomInviteReject(_roomId, _friendId);

            Assert.AreEqual(RoomResult.Success, _answer);
            Assert.AreEqual(InviteStatus.Rejected, _canInvite.InviteStatus);
            Assert.AreEqual(0, _accepted);
            Assert.AreEqual(1, _rejected);

            // dont invite twice
            _canInvite.Invite(_mCallback.Object);
            _mFriendsClient.Verify(_ => _.InviteRoom(_roomId, _friendId, It.IsAny<FriendsDelegate.RoomAnswerCallback>()), Times.Once);

            _mCallback.Verify(_ => _(It.IsAny<long>(), It.Is<RoomResult>(r => r == RoomResult.Success)), Times.Once);
            _mCallback.Verify(_ => _(It.IsAny<long>(), It.Is<RoomResult>(r => r == RoomResult.InvalidStatus)), Times.Once);
        }

        [TestMethod]
        public void TestCanInviteFriendError()
        {
            _mFriendsClient.SetupLobbyInviteCallback(RoomResult.NotAllowed);

            Assert.AreEqual(InviteStatus.None, _canInvite.InviteStatus);

            _canInvite.Invite(_mCallback.Object);

            _mFriendsClient.Verify(_ => _.InviteRoom(_roomId, _friendId, It.IsAny<FriendsDelegate.RoomAnswerCallback>()), Times.Once);

            Assert.AreEqual(RoomResult.NotAllowed, _answer);
            Assert.AreEqual(InviteStatus.Error, _canInvite.InviteStatus);
            Assert.AreEqual(0, _accepted);
            Assert.AreEqual(0, _rejected);

            // dont invite twice
            _canInvite.Invite(_mCallback.Object);
            _mFriendsClient.Verify(_ => _.InviteRoom(_roomId, _friendId, It.IsAny<FriendsDelegate.RoomAnswerCallback>()), Times.Exactly(2));

            _mCallback.Verify(_ => _(It.IsAny<long>(), It.IsAny<RoomResult>()), Times.Exactly(2));
        }

        [TestMethod]
        public void TestStatusChange()
        {
            Assert.AreEqual(0, _status);
            Assert.IsNull(_lastStatus);

            _mFriendsClient.RaiseFriendStatusChanged(_friendId, FriendStatus.Online);

            Assert.AreEqual(1, _status);
            Assert.AreEqual(FriendStatus.Online, _lastStatus);
            Assert.AreEqual(FriendStatus.Online, _canInvite.FriendInfo.Status);
        }

        [TestMethod]
        public void TestEventFiltering()
        {
            _canInvite.Invite(null);
            var badId = new MadId((uint)_friendId + 1);

            _mFriendsClient.RaiseRoomInviteAccept(_roomId + 1, _friendId);
            _mFriendsClient.RaiseRoomInviteAccept(_roomId, badId);
            _mFriendsClient.RaiseFriendStatusChanged(badId, FriendStatus.Offline);

            Assert.AreEqual(0, _accepted);
            Assert.AreEqual(0, _rejected);
            Assert.AreEqual(0, _status);
            Assert.IsNull(_lastStatus);
        }

        [TestMethod]
        public void TestCanInviteByStatus()
        {
            Assert.IsTrue(_canInvite.CanInvite);

            var canInvite = new InviteableFriend(_mFriendsClient.Object, _roomId, new FriendBase()
            {
                Id = new MadId(2),
                Status = FriendStatus.Offline,
            }, false);
            Assert.IsFalse(canInvite.CanInvite);
            canInvite.Invite(_mCallback.Object);
            _mCallback.Verify(_ => _(_roomId, RoomResult.InvalidStatus));

            canInvite = new InviteableFriend(_mFriendsClient.Object, _roomId, new FriendBase()
            {
                Id = new MadId(3),
                Status = FriendStatus.InRoom
            }, false);
            Assert.IsFalse(canInvite.CanInvite);
            canInvite.Invite(_mCallback.Object);
            _mCallback.Verify(_ => _(_roomId, RoomResult.InvalidStatus), Times.Exactly(2));
        }
    }
}
