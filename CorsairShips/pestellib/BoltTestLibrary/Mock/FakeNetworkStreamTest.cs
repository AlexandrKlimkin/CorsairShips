using System.Threading.Tasks;
using BoltMasterServer;
using MasterServerProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoltTestLibrary.Mock
{
    [TestClass]
    public class FakeNetworkStreamTest
    {
        [TestMethod]
        public async Task TestFakeNetworkStream()
        {
            FakeNetworkStream str = new FakeNetworkStream();
            var msg1 = new Message();
            var msg2 = new Message();
            await str.WriteMessageAsync(msg1);
            await str.WriteMessageAsync(msg2);
            var receivedMessage = await str.RemotePCStream.ReadMessageAsync();
            var receivedMessage2 = await str.RemotePCStream.ReadMessageAsync();
            Assert.IsNotNull(receivedMessage);
            Assert.AreEqual(msg1.MessageId, receivedMessage.MessageId);
            Assert.AreEqual(msg2.MessageId, receivedMessage2.MessageId);
        }
    }
}
