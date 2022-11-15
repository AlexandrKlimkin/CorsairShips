using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FriendsClient;
using FriendsClient.FriendList.Concrete;
using FriendsClient.Private;
using S;

namespace FriendsServerTest
{
    [TestClass]
    public class PlayerContextTests
    {
        private FriendBase _alienPlayer = new FriendBase { Id = new MadId(1) };
        private FriendBase[] _friends = new FriendBase[] {
            new FriendBase() { Id = new MadId(2)},
        };

        private Mock<IFriendsClientPrivate> _mFriendClient = new Mock<IFriendsClientPrivate>();
        private FriendContext _friendContext;

        [TestInitialize]
        public void Init()
        {
            ReInit(_friends[0]);
        }

        [TestMethod]
        public void TestIsMyFriend()
        {
            Assert.IsTrue(_friendContext.IsMyFriend);
            ReInit(_alienPlayer);
            Assert.IsFalse(_friendContext.IsMyFriend);
        }

        private void ReInit(FriendBase player)
        {
            _mFriendClient.Setup(_ => _.GetFriends()).Returns(_friends);
            _friendContext = new FriendContext(_mFriendClient.Object, player);
        }
    }
}
