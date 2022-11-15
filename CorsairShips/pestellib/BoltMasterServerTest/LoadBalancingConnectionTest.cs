using System;
using System.Net;
using System.Threading.Tasks;
using BoltMasterServer;
using BoltMasterServer.Connections;
using BoltTestLibrary.Mock;
using MasterServerProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PhotonBoltNetworkUtils;

namespace BoltMasterServerTest
{
    [TestClass]
    public class LoadBalancingConnectionTest
    {
        [TestMethod]
        public void LoadBalancingConnectionDefaultConstructor()
        {
            Assert.ThrowsException<NotImplementedException>(() => new LoadBalancingConnection(),
                "Some subclasses of Connection can be instantiated from Server<>/Client<> with parameterless" +
                " constructor, but LoadBalancing connection - is not");
        }
        
        [TestMethod]
        public void LoadBalancingConnectionReport()
        {
            var connection = InitLoadBalancingConnection(out var settings, out var gameServers);

            var report = connection.GetReport();
            
            ValidateReport(settings, report, gameServers);
        }

        [TestMethod]
        public async Task CheckReportRoutine()
        {
            var stream = new FakeNetworkStream();
            var connection = InitLoadBalancingConnection(out var settings, out var gameServers);

            _ = Task.Run(() => connection.MainLoop(stream));
            _ = Task.Run(connection.ReportToLoadBalancing);
            await Task.Delay(1100);
            var report = (MasterServerReport)stream.RemotePCStream.ReadMessage();
            
            ValidateReport(settings, report, gameServers);
        }
        
        [TestMethod]
        public async Task TestCreateGameRequest()
        {
            var stream = new FakeNetworkStream();
            var connection = InitLoadBalancingConnection(out var settings, out var gameServers);
            _ = Task.Run(() => connection.MainLoop(stream));

            var createGameRequest = new CreateGameRequest();
            stream.RemotePCStream.WriteMessage(createGameRequest);
            await Task.Delay(300);
            var message = stream.RemotePCStream.ReadMessage();
            
            Assert.IsInstanceOfType(message, typeof(CreateGameResponse));
            var response = (CreateGameResponse) message;
            Assert.AreEqual(createGameRequest.MessageId, response.MessageId);
            Assert.IsTrue(response.JoinInfo.Port > 0);
            Assert.IsTrue(response.JoinInfo.Port < 65535);
            Assert.IsTrue(IPAddress.TryParse(response.JoinInfo.IpAddress, out var _));
            Assert.AreNotEqual(Guid.Empty, response.JoinInfo.ReservedSlot);
        }

        [TestMethod]
        public async Task TestReserveSlotRequest()
        {
            var stream = new FakeNetworkStream();
            var connection = InitLoadBalancingConnection(out var settings, out var gameServers);
            _ = Task.Run(() => connection.MainLoop(stream));
            
            var reserveSlotRequest = new ReserveSlotRequest(){ ProcessID = 123 };
            stream.RemotePCStream.WriteMessage(reserveSlotRequest);
            await Task.Delay(300);

            var response = (ReserveSlotResponse)stream.RemotePCStream.ReadMessage();

            Assert.IsTrue(response.Succeed);
            Assert.AreEqual(reserveSlotRequest.MessageId, response.MessageId);
            Assert.IsNotNull(response.JoinInfo);
            Assert.IsTrue(IPAddress.TryParse(response.JoinInfo.IpAddress, out var _));
            Assert.IsTrue(response.JoinInfo.Port > 0 && response.JoinInfo.Port < 65535);
            Assert.AreNotSame(Guid.Empty, response.JoinInfo.ReservedSlot);
        }

        private static Mock<IGameServerLauncher> GameServerLauncherMock(GameServersCollection gameServersCollection)
        {
            var mock = new Mock<IGameServerLauncher>();
            mock.Setup(x => x.StartNewServerInstance(It.IsAny<string>()))
                .Returns(async () =>
                {
                    await Task.Delay(10);
                    return new JoinInfo
                    {
                        Port = 500,
                        IpAddress = "127.0.0.1",
                        ReservedSlot = Guid.Parse("e83a0a09-7bb6-430c-8c9b-e45e788806f1")
                    };
                });
            return mock;
        }
        
        private static LoadBalancingConnection InitLoadBalancingConnection(out Settings settings, out GameServersCollection gameServers)
        {
            var mockGameServer = new Mock<IGameServer>();
            mockGameServer.Setup(x => x.ReserveSlot(It.IsAny<ReserveSlotRequest>())).Returns<ReserveSlotRequest>(async (request) =>
            {
                await Task.Delay(10);
                return new ReserveSlotResponse()
                {
                    Succeed = true,
                    JoinInfo = new JoinInfo()
                    {
                        Port = 123,
                        IpAddress = "127.0.0.1",
                        ReservedSlot = Guid.Parse("083a0a09-7bb6-430c-8c9b-e45e788806f2")
                    },
                    MessageId = request.MessageId
                };
            });
            mockGameServer.Setup(x => x.Port).Returns(() => 123);
            
            settings = new Settings();
            gameServers = new GameServersCollection
            {
                [123] = mockGameServer.Object, 
                [456] = mockGameServer.Object
            };
            var loadbalancingConnection = new LoadBalancingConnection(new Settings(),
                GameServerLauncherMock(gameServers).Object, gameServers);
            return loadbalancingConnection;
        }
        
        private static void ValidateReport(Settings settings, MasterServerReport report, GameServersCollection gameServers)
        {
            Assert.AreEqual(settings.GameName, report.GameInfo.GameName);
            Assert.AreEqual(settings.GameVersion, report.GameInfo.GameVersion);
            Assert.AreEqual(gameServers.Count, report.GameServers.Length);
            Assert.AreEqual(settings.MasterListenerPort, report.MasterListenerPort);
            Assert.IsTrue(report.InstanceId != Guid.Empty);
        }

    }
}