using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PestelLib.ServerCommon;
using Moq;
using PestelLib.MatchmakerServer;
using PestelLib.MatchmakerServer.Config;
using PestelLib.MatchmakerShared;
using System.IO;
using Nerdbank;
using Microsoft.Extensions.Configuration;
using System.Threading;
using MatchmakerTest.Dummies;
using ServerLib;

namespace MatchmakerTest
{
    [TestClass]
    public class MatchmakerServerTest
    {
        [TestInitialize]
        public void Init()
        {
            Log.Init();
            string configPath = AppDomain.CurrentDomain.BaseDirectory + "appsettings.json";
            AppSettings.LoadConfig(configPath);
        }

        [TestMethod]
        public void RegisterAndLeaveDeliveryTest()
        {
            var strategy = new Mock<IMatchmakerStrategy>();
            var stream = new MatchmakerMessageStream(new MemoryStream());
            var processor = new MatchmakerMessageProcessor();
            var proto = new MatchmakerServerProtocol<MatchmakerRequest, SimpleMatch, MatchingStats>(processor, new MatchmakerConfig());
            var server = new MatchmakerServer(proto, strategy.Object, new MatchmakerConfig());
            var duplex = FullDuplexStream.CreateStreams();
            var playerId = Guid.NewGuid();
            var serverStream = new MatchmakerMessageStream(duplex.Item1);
            var clientStream = new MatchmakerMessageStream(duplex.Item2);
            var mmRegisterMessage = new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = new MatchmakerRequest() { PlayerId = playerId } };

            processor.AddSource(stream);
            processor.AddSource(serverStream);
            clientStream.Write(mmRegisterMessage);

            strategy.Verify(_ => _.Leave(It.IsAny<Guid>()), Times.Never);

            clientStream.Dispose();
            serverStream.Dispose();

            strategy.Verify(_ => _.NewRequest(It.Is<MatchmakerRequest>(__ => __.PlayerId == mmRegisterMessage.MatchParams.PlayerId)), Times.Once);
            strategy.Verify(_ => _.Leave(It.Is<Guid>(__ => __ == playerId)), Times.Once);

            server.Dispose();
            proto.Dispose();
            processor.Dispose();
        }
    }
}
