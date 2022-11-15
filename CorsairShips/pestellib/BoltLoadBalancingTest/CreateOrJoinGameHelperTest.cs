using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BoltLoadBalancing.Logic;
using BoltLoadBalancing.MasterServer;
using BoltLoadBalancing.MatchMaking;
using MasterServerProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoltLoadBalancingTest
{
    [TestClass]
    public class CreateOrJoinGameHelperTest
    {
        [TestMethod]
        public async Task NoAnyGames()
        {
            var createOrJoinGameHelper = Initialize(out var game, out var testMasterServer);
            
            var result = await createOrJoinGameHelper.GetJoinInfo(new List<MatchMakingGame>(), string.Empty);
            CheckJoinInfo(game, result, testMasterServer);
            
            Assert.IsFalse(testMasterServer.ReserveSlotWasCalled);
            Assert.IsTrue(testMasterServer.CreateGameWasCalled);
        }
        
        [TestMethod]
        public async Task JoinExistingGame()
        {
            var createOrJoinGameHelper = Initialize(out var game, out var testMasterServer);
            
            var result = await createOrJoinGameHelper.GetJoinInfo(new List<MatchMakingGame> {game, game, game}, string.Empty);
            CheckJoinInfo(game, result, testMasterServer);
            
            Assert.IsTrue(testMasterServer.ReserveSlotWasCalled);
            Assert.IsFalse(testMasterServer.CreateGameWasCalled);
        }
        
        [TestMethod]
        public async Task SlotReservationFailedOnAllServers()
        {
            var createOrJoinGameHelper = Initialize(out var game, out var testMasterServer);
            testMasterServer.ForceReserveToAlwaysFail = true;
            
            var result = await createOrJoinGameHelper.GetJoinInfo(new List<MatchMakingGame> {game, game, game}, string.Empty);
            CheckJoinInfo(game, result, testMasterServer);
            
            Assert.IsTrue(testMasterServer.ReserveSlotWasCalled);
            Assert.IsTrue(testMasterServer.CreateGameWasCalled);
        }

        private static void CheckJoinInfo(MatchMakingGame game, JoinInfo result, MasterServerMock testMasterServer)
        {
            Assert.AreEqual(game.GameServerStateReport.Port, result.Port);
            Assert.AreEqual(game.MasterServer.RemoteIP.ToString(), result.IpAddress);
            Assert.AreEqual(testMasterServer.ReservedSlot, result.ReservedSlot);
        }

        private static CreateOrJoinGameHelper Initialize(out MatchMakingGame game, out MasterServerMock testMasterServer)
        {
            var masterServerCollection = new MasterServersCollection();
            testMasterServer = new MasterServerMock();
            masterServerCollection[Guid.Parse("1a559ad4-8b2b-4b11-8505-9f8a4dd2d3c3")] = testMasterServer;

            game = new MatchMakingGame()
            {
                MasterServer = testMasterServer,
                GameServerStateReport = new GameServerStateReport
                {
                    Port = 123
                }
            };

            var createOrJoinGameHelper = new CreateOrJoinGameHelper(masterServerCollection);
            return createOrJoinGameHelper;
        }
    }
}