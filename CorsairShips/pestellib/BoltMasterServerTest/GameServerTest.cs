using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoltMasterServer;
using BoltTransport;
using BoltTestLibrary.Mock;
using MasterServerProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PhotonBoltNetworkUtils;

namespace BoltMasterServerTest
{
    [TestClass]
    public class GameServerTest
    {
        private static readonly Guid ReservationId;
        private static readonly Guid MessageId;
        private static readonly DateTime DefaultTimeStamp;

        static GameServerTest()
        {
            ReservationId = Guid.Parse("becd3a07-9eda-466e-8af9-d0f31fc2038e");
            MessageId = Guid.Parse("42d8691d-dd37-4ef7-a23e-1d53e96f7c1e");
            DefaultTimeStamp = new DateTime(1985, 6, 25);
        }

        private const int PlayersCount = 5;
        private const int Port = 123;
        private const int ProcessId = 555;
        private const int MaxPlayers = 16;

        [TestMethod]
        public void TestProperties()
        {
            var gameServer = InitializeGameServer();
            
            Assert.AreEqual(gameServer.Players, PlayersCount);
            Assert.AreEqual(gameServer.Port, Port);
            Assert.AreEqual(gameServer.ProcessID, ProcessId);
            
            Assert.AreEqual(1, gameServer.Reserved);
        }

        [TestMethod]
        public void TestTimers()
        {
            var settings = new Settings();
            var timeProviderMock = new Mock<ITimeProvider>();
            timeProviderMock
                .Setup(x => x.UtcNow)
                .Returns( DefaultTimeStamp );
            
            var gameServer = InitializeGameServer(timeProviderMock.Object);
            
            Assert.IsFalse(gameServer.IsItTimeToShutdownServer() );
            
            gameServer.State.ReservedSlots.Clear();
            gameServer.State.Players = 0;
            
            Assert.IsFalse(gameServer.IsItTimeToShutdownServer() );

            gameServer.State.NoAnyPlayersTimestamp = (DefaultTimeStamp - settings.InstanceShutdownTimeout).Ticks;
            
            Assert.IsTrue(gameServer.IsItTimeToShutdownServer());

            gameServer.State.NoAnyPlayersTimestamp++;
            
            Assert.IsFalse(gameServer.IsItTimeToShutdownServer());

            gameServer.State.NoAnyPlayersTimestamp = (DefaultTimeStamp - settings.InstanceBlockTimeout).Ticks;
            Assert.IsTrue(gameServer.IsServerGoingToClose());
            
            Assert.IsTrue(gameServer.TimeSinceUpdate.Ticks == 0);

            timeProviderMock
                .Setup(x => x.UtcNow)
                .Returns( DefaultTimeStamp + TimeSpan.FromMinutes(1) );
            
            Assert.IsTrue(gameServer.TimeSinceUpdate == TimeSpan.FromMinutes(1));
        }

        [TestMethod]
        public async Task CheckShutdown()
        {
            var connection = new Connection();
            var gameServer = new GameServer(connection, new GameServerStateReport(), new Settings());
            var stream = new FakeNetworkStream();
            _ = Task.Run(() => connection.MainLoop(stream));
            
            gameServer.Shutdown();
            await Task.Delay(100);

            var messageOnRemoteServer = stream.RemotePCStream.ReadMessage();
            Assert.IsInstanceOfType(messageOnRemoteServer, typeof(ShutdownServer));
        }

        [TestMethod]
        public async Task CheckReserveSlot()
        {
            var gameServer = InitializeGameServer();

            var response = await gameServer.ReserveSlot(new ReserveSlotRequest { ProcessID = ProcessId });
            
            Assert.IsTrue(response.Succeed);
        }

        private static GameServer InitializeGameServer(ITimeProvider timeProvider = null)
        {
            var connectionToGameServer = new Mock<IConnection>();
            connectionToGameServer
                .Setup(x => x.SendMessageAsync<ReserveSlotResponse>(It.IsAny<ReserveSlotRequest>()))
                .Returns<ReserveSlotRequest>(
                    async (reserveSlotRequest) =>
                    {
                        await Task.Delay(10);
                        return new ReserveSlotResponse
                        {
                            MessageId = reserveSlotRequest.MessageId,
                            Succeed = true,
                            JoinInfo = new JoinInfo
                            {
                                Port = Port,
                                IpAddress = "127.0.0.1",
                                ReservedSlot = Guid.Parse("083a0a09-7bb6-430c-8c9b-e45e788806f2")
                            }
                        };
                    });
            
            var gameServerReport = new GameServerStateReport()
            {
                Players = PlayersCount,
                Port = Port,
                MessageId = MessageId,
                ProcessID = ProcessId,
                NoAnyPlayersTimestamp = DefaultTimeStamp.Ticks,
                MatchmakingData = string.Empty,
                ReservedSlots = new List<SlotReservation>
                {
                    new SlotReservation
                    {
                        Timestamp = DefaultTimeStamp.Ticks,
                        ReservationId = ReservationId
                    }
                }
            };

            var settings = new Settings
            {
                MaxPlayersPerServer = MaxPlayers 
            };

            var gameServer = new GameServer(connectionToGameServer.Object, gameServerReport, settings, timeProvider);
            return gameServer;
        }
    }
}