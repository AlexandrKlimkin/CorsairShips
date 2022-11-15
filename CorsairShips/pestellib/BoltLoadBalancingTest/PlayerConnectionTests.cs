using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoltLoadBalancing.MatchMaking;
using MasterServerProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BoltLoadBalancingTest
{
    [TestClass]
    public class PlayerConnectionTests
    {
        [TestMethod]
        public async Task RequestMatch()
        {
            /*
            var masterServerCollection = new MasterServersCollection();
            var matchmaking = new Mock<IMatchmaking>();
            matchmaking.Setup(x => x.GetPossibleMatches(It.IsAny<string>(), It.IsAny<IEnumerable<MasterServer>>()))
                .Returns<IEnumerable<MasterServer>>(
                (allServers) => allServers.Select(x => new MatchMakingGame()
                {
                    MasterServer = x,
                    GameServerStateReport = new GameServerStateReport()
                    {
                    }
                }).ToList());
                
            var createOrJoinGameHelper = new Mock<ICreateOrJoinGameHelper>();
            createOrJoinGameHelper
                .Setup(x => x.GetJoinInfo(
                    It.IsAny<List<MatchMakingGame>>(),
                    It.IsAny<string>()))
                .Returns(async () =>
                {
                    await Task.Delay(10);
                    return new JoinInfo()
                    {
                        Port = 123,
                        IpAddress = "127.0.0.1",
                        ReservedSlot = Guid.Parse("b7ddb605-c17e-42cc-81d5-a1934669b12b")
                    };
                });
            
            var playerConnection = new PlayerConnection();
            */

            /*
            var masterServerStream = new FakeNetworkStream();
            var masterServerCollection = new MasterServersCollection();
            var masterServerConnection = new MasterServerConnection(masterServerCollection, new SemaphoreSlim(1, 1));
            
            masterServerCollection[Guid.Parse("1a559ad4-8b2b-4b11-8505-9f8a4dd2d3c3")] = new MasterServer(
                masterServerConnection, 
                new MasterServerReport()
            );
            
            var mockMatchmaking = new Mock<IMatchmaking>();
            mockMatchmaking
                .Setup(x => x.GetPossibleMatches(
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<MasterServer>>()))
                .Returns(() => new List<MatchMakingGame>(new []
                {
                    new MatchMakingGame()
                    {
                        MasterServer = masterServerCollection.Values.First(),
                        GameServerStateReport = masterServerCollection.Values.First().GameServers.First()
                    }
                }));
            
            var playerStream = new FakeNetworkStream();
            var playerConnection = new PlayerConnection(masterServerCollection, new Matchmaking(), new CreateOrJoinGameHelper(masterServerCollection));
            _ = Task.Run(() => masterServerConnection.MainLoop(masterServerStream));
            _ = Task.Run(() => playerConnection.MainLoop(playerStream));

            await Task.Delay(100);
            
            playerStream.RemotePCStream.WriteMessage(new RequestServer { MatchmakingData = string.Empty });

            await Task.Delay(100);
            var response = playerStream.RemotePCStream.ReadMessage();
            */
        }
    }
}