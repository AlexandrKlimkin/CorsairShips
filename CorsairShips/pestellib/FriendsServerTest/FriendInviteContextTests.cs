using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FriendsClient;
using FriendsClient.FriendList.Concrete;
using FriendsClient.Private;
using S;

namespace FriendsServerTest
{
    [TestClass]
    public class FriendInviteContextTests
    {
        private long _inviteId = 1;
        private int _accept;
        private int _reject;
        private InviteFriendResult _result;
        private MadId _playerId = new MadId(1);
        private Mock<IFriendsClientPrivate> _mFriendClient;
        private FriendInviteContext _friendInviteContext;
        private Mock<FriendsDelegate.InviteFriendCallback> _mCallback;

        [TestInitialize]
        public void Init()
        {
            ReInit(false);
        }

        [TestMethod]
        public void TestInviteCallback()
        {
            _friendInviteContext.Send();
            _mFriendClient.Verify(_ => _.InviteFriend(_playerId, It.IsAny<FriendsDelegate.InviteFriendCallback>()), Times.Once);
            _mFriendClient.RaiseFriendInviteAccept(_playerId, _inviteId);
            _mCallback.Verify(_ => _(_inviteId, InviteFriendResult.Success));
            Assert.AreEqual(InviteFriendResult.Success, _friendInviteContext.Result);
            Assert.AreEqual(_inviteId, _friendInviteContext.InviteId);
            Assert.AreEqual(1, _accept);
            Assert.AreEqual(0, _reject);
            Assert.AreEqual(InviteFriendResult.Success, _result);
        }

        [TestMethod]
        public void TestInviteReject()
        {
            ReInit(true);
            _friendInviteContext.Send();
            _mFriendClient.RaiseFriendInviteReject(_playerId, _inviteId);
            Assert.AreEqual(0, _accept);
            Assert.AreEqual(1, _reject);
        }

        private void ReInit(bool noCallback)
        {
            _accept = 0;
            _reject = 0;
            _result = InviteFriendResult.None;
            _mFriendClient = new Mock<IFriendsClientPrivate>();
            _mFriendClient.Setup(_ => _.InviteFriend(It.IsAny<MadId>(), It.IsAny<FriendsDelegate.InviteFriendCallback>())).Callback<MadId, FriendsDelegate.InviteFriendCallback>((id, callback) =>
            {
                callback(_inviteId, InviteFriendResult.Success);
            });
            _mCallback = new Mock<FriendsDelegate.InviteFriendCallback>();
            _friendInviteContext = new FriendInviteContext(_mFriendClient.Object, _playerId, noCallback ? null : _mCallback.Object);
            _friendInviteContext.OnAccept += (c) => ++_accept;
            _friendInviteContext.OnReject += (c) => ++_reject;
            _friendInviteContext.OnInviteSendResult += (с,r) => {
                _result = r;
            };
        }
    }
}
