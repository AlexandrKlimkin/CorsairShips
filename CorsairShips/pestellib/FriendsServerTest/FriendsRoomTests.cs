using System;
using System.Linq;
using FriendsClient;
using FriendsClient.FriendList;
using FriendsClient.Lobby;
using FriendsClient.Lobby.Concrete;
using FriendsClient.Private;
using FriendsClient.Sources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PestelLib.Utils;
using S;
using UnityDI;

namespace FriendsServerTest
{
    class FakeSharedTime : ITimeProvider
    {
        public bool IsSynced => true;
        public DateTime Now => DateTime.UtcNow;
    }

    [TestClass]
    public class FriendsRoomTests
    {
        private FriendBase[] _friends = new FriendBase[]
        {
            new FriendBase() {Id = new MadId(2), Status = FriendStatus.Online},
            new FriendBase() {Id = new MadId(3), Status = FriendStatus.Online},
            new FriendBase() {Id = new MadId(4), Status = FriendStatus.Online},
        };

        private MadId _owner = new MadId(1);
        private FriendBase _ownerInfo = new FriendBase() {Id = new MadId(1), Status = FriendStatus.Online};
        private MadId _foreignOwner = new MadId(1001);
        private string _gameData = "gamedata";
        private long _roomId = 1;
        private MadId _alienUser = new MadId(5);
        private Mock<IFriendsClientPrivate> _mFriendClient;
        private Mock<IFriendList> _mFriendList;
        private FriendsRoom _room;
        private int _leave;
        private int _kick;
        private int _join;
        private int _status;
        private int _imhost;

        [TestInitialize]
        public void Init()
        {
            if (ContainerHolder.Container.Resolve<ITimeProvider>() == null)
            {
                ContainerHolder.Container.RegisterInstance<ITimeProvider>(new FakeSharedTime());
            }

            ReInit(_owner);
        }

        [TestMethod]
        public void TestInit()
        {
            Assert.AreEqual(_roomId, _room.RoomId);
            Assert.IsTrue(_room.ImHost);
            Assert.AreEqual(1, _room.Party.Count);
            Assert.AreEqual(_friends.Length, _room.CanInvite.Count);
        }

        [TestMethod]
        public void TestLeave()
        {
            Assert.IsFalse(_room.RoomStatus == RoomStatus.Left);
            _room.Leave(null);
            Assert.IsTrue(_room.RoomStatus == RoomStatus.Left);
            _mFriendClient.Verify(_ => _.LeaveRoom(_roomId, It.IsAny<FriendsDelegate.FriendsClientCallback>()), Times.Once);
        }

        [TestMethod]
        public void TestInviteAcceptAndLeave()
        {
            Assert.IsTrue(_room.CanInvite.Count > 0);

            var users = _room.CanInvite.Count;
            var usersWithOwner = users + 1;
            foreach (var canInviteFriend in _room.CanInvite.ToArray())
            {
                canInviteFriend.Invite(null);
                _mFriendClient.RaiseRoomInviteAccept(_roomId, canInviteFriend.FriendInfo.Id);
            }

            Assert.AreEqual(0, _leave);
            Assert.AreEqual(0, _kick);
            Assert.AreEqual(0, _status);
            Assert.AreEqual(users, _join);
            Assert.AreEqual(0, _imhost);

            // all friends in party
            Assert.AreEqual(usersWithOwner, _room.Party.Count);
            Assert.AreEqual(0, _room.CanInvite.Count);

            foreach (var invited in _room.Party.Where(_ => _.FriendInfo.Id != _owner).ToArray())
            {
                _mFriendClient.RaiseRoomLeave(_roomId, invited.FriendInfo.Id);
            }

            Assert.AreEqual(users, _leave);
            Assert.AreEqual(0, _kick);
            Assert.AreEqual(0, _status);
            Assert.AreEqual(users, _join);
            Assert.AreEqual(0, _imhost);

            // all friends leaves and can be invited again
            Assert.AreEqual(users, _room.CanInvite.Count);
            Assert.AreEqual(1, _room.Party.Count);
        }

        [TestMethod]
        public void TestKickLeaveReinvite()
        {
            var users = _room.CanInvite.Count;
            var usersWithOwner = users + 1;
            foreach (var canInviteFriend in _room.CanInvite.ToArray())
            {
                canInviteFriend.Invite(null);
                _mFriendClient.RaiseRoomInviteAccept(_roomId, canInviteFriend.FriendInfo.Id);
            }

            Assert.IsTrue(users > 0);
            Assert.AreEqual(usersWithOwner, _room.Party.Count);

            var leave = 1;
            var kick = users - leave;

            var party = _room.Party.Where(_ => _.FriendInfo.Id != _owner).ToArray();
            for (var i = 0; i < party.Length; ++i)
            {
                if(i < leave)
                    _mFriendClient.RaiseRoomLeave(_roomId, party[i].FriendInfo.Id);
                else
                    party[i].Kick(null);
            }

            _mFriendClient.Verify(_ => _.KickRoom(It.IsAny<long>(), It.IsAny<MadId>(), It.IsAny<FriendsDelegate.RoomAnswerCallback>()), Times.Exactly(kick));

            Assert.AreEqual(leave, _leave);
            Assert.AreEqual(kick, _kick);
            Assert.AreEqual(0, _status);
            Assert.AreEqual(users, _join);
            Assert.AreEqual(0, _imhost);

            Assert.AreEqual(users, _room.CanInvite.Count);
            Assert.AreEqual(1, _room.Party.Count);

            foreach (var canInviteFriend in _room.CanInvite.ToArray())
            {
                canInviteFriend.Invite(null);
                _mFriendClient.RaiseRoomInviteAccept(_roomId, canInviteFriend.FriendInfo.Id);
            }

            Assert.AreEqual(0, _room.CanInvite.Count);
            Assert.AreEqual(usersWithOwner, _room.Party.Count);
        }

        [TestMethod]
        public void TestJoin()
        {
            Assert.IsTrue(_room.CanInvite.Count > 0);

            ReInit(_foreignOwner);

            var users = _room.CanInvite.Count;
            foreach (var f in _room.CanInvite.ToArray())
            {
                _mFriendClient.RaiseRoomJoin(_roomId, f);
            }
            Assert.AreEqual(0, _room.Party.Count);
            Assert.AreEqual(0, _room.CanInvite.Count);

            Assert.AreEqual(0, _leave);
            Assert.AreEqual(0, _kick);
            Assert.AreEqual(0, _status);
            Assert.AreEqual(users, _join);
            Assert.AreEqual(0, _imhost);
        }

        [TestMethod]
        public void TestFriendStatusChange()
        {
            Assert.IsTrue(_room.CanInvite.Count > 0);

            var user = _room.CanInvite[0];

            _mFriendClient.RaiseFriendStatusChanged(user.FriendInfo.Id, FriendStatus.Online);

            Assert.AreEqual(1, _status);
            Assert.AreEqual(FriendStatus.Online, user.FriendInfo.Status);
            Assert.AreEqual(1, _room.Party.Count);
            user.Invite(null);
            _mFriendClient.RaiseRoomInviteAccept(_roomId, user.FriendInfo.Id);
            Assert.AreEqual(2, _room.Party.Count);

            _mFriendClient.RaiseFriendStatusChanged(user.FriendInfo.Id, FriendStatus.Offline);
            Assert.AreEqual(2, _status);
            Assert.AreEqual(FriendStatus.Offline, user.FriendInfo.Status);
            Assert.AreEqual(1, _room.Party.Count); // offline status leaves room (that could be done by Leave event beforehand in real logic).
        }

        [TestMethod]
        public void TestEventFiltering()
        {
            Assert.IsTrue(_room.CanInvite.Count > 0);

            var user = _room.CanInvite[0];
            var wrongRoom = _roomId + 1;
            user.Invite(null);
            _mFriendClient.RaiseRoomInviteAccept(wrongRoom, user.FriendInfo.Id);
            Assert.AreEqual(1, _room.Party.Count); // invite acceped at another room.

            _mFriendClient.RaiseRoomInviteAccept(_roomId, user.FriendInfo.Id);
            Assert.AreEqual(2, _room.Party.Count);

            _mFriendClient.RaiseRoomLeave(wrongRoom, user.FriendInfo.Id);
            Assert.AreEqual(0, _leave); // leaves another room (impossible scenario, just test filtering).

            _mFriendClient.RaiseRoomKick(wrongRoom, user.FriendInfo.Id);
            Assert.AreEqual(0, _kick); // kicked from another room (impossible scenario, just test filtering).

            _mFriendClient.RaiseFriendStatusChanged(_alienUser, FriendStatus.Online);
            Assert.AreEqual(0, _status); // react only on status change from friends and users in room.
        }

        [TestMethod]
        public void TestStartBattle()
        {
            var callback = new Mock<FriendsDelegate.StartBattleCallback>();
            _room.StartBattle(callback.Object);

            callback.Verify(_ => _(RoomResult.Success), Times.Once);
            _mFriendClient.Verify(_ => _.StartBattle(_roomId, _gameData, It.IsAny<FriendsDelegate.StartBattleCallback>()), Times.Once);
        }

        [TestMethod]
        public void TestNowAllowedToStartBattle()
        {
            ReInit(_foreignOwner);
            var callback = new Mock<FriendsDelegate.StartBattleCallback>();
            _room.StartBattle(callback.Object);

            callback.Verify(_ => _(RoomResult.NotAllowed), Times.Once);
            _mFriendClient.Verify(_ => _.StartBattle(_roomId, _gameData, It.IsAny<FriendsDelegate.StartBattleCallback>()), Times.Never);
        }

        private void ReInit(MadId owner)
        {
            _leave = 0;
            _kick = 0;
            _join = 0;
            _status = 0;
            _imhost = 0;

            _mFriendClient = new Mock<IFriendsClientPrivate>();
            _mFriendList = new Mock<IFriendList>();

            _mFriendList.Setup(_ => _.Me).Returns(_ownerInfo);
            _mFriendClient.Setup(_ => _.Id).Returns(_owner);
            _mFriendClient.Setup(_ => _.GetFriends()).Returns(_friends);
            _mFriendClient.Setup(_ => _.FriendList).Returns(_mFriendList.Object);
            _mFriendClient.Setup(_ => _.IsMyFriend(It.IsAny<MadId>())).Returns((Func<MadId,bool>)(id =>
            {
                return _friends.Any(_ => _.Id == id);
            }));
            _mFriendClient
                .Setup(_ => _.LeaveRoom(It.IsAny<long>(),
                    It.IsAny<FriendsDelegate.FriendsClientCallback>()))
                .Callback<long, FriendsDelegate.FriendsClientCallback>(
                    (l, callback) => { callback(true); });
            _mFriendClient.Setup(_ =>
                    _.KickRoom(It.IsAny<long>(), It.IsAny<MadId>(), It.IsAny<FriendsDelegate.RoomAnswerCallback>()))
                .Callback<long, MadId, FriendsDelegate.RoomAnswerCallback>((l, id, callback) =>
                {
                    callback(l, RoomResult.Success);
                    _mFriendClient.RaiseRoomKick(l, id);
                });
            _mFriendClient.Setup(_ => _.StartBattle(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<FriendsDelegate.StartBattleCallback>()))
                .Callback<long, string, FriendsDelegate.StartBattleCallback>((r, gd, callback) =>
                {
                    callback(RoomResult.Success);
                });

            _room = new FriendsRoom(_mFriendClient.Object, _roomId, owner, TimeSpan.FromMinutes(2), 3);
            _room.GameSpecificData = _gameData;
            _room.OnLeave += (_,id) => ++_leave;
            _room.OnKick += (_, id) => ++_kick;
            _room.OnJoined += (_, f) => ++_join;
            _room.OnFriendStatus += (_, m) => ++_status;
            _room.OnImHost += (_) => ++_imhost;

        }
    }
}

