using System;
using System.IO;
using System.Threading.Tasks;
using BoltMasterServer;
using BoltMasterServer.Connections;
using BoltTestLibrary.Mock;
using MasterServerProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotonBoltNetworkUtils;

namespace BoltMasterServerTest
{
    [TestClass]
    public class GameServerConnectionTest
    {
        [TestMethod]
        public async Task GameServerConnection()
        {
            var gameServers = new GameServersCollection();
            var settings = new Settings();
            var gameServerConnection = new GameServerConnection(gameServers, settings);
            var stream = new FakeNetworkStream();

            var processId = 123;
            stream.RemotePCStream.WriteMessage(new GameServerStateReport
            {
                ProcessID = processId
            });

            //это запуск циклической обработки всех сообщений
            _ = Task.Run(() => gameServerConnection.MainLoop(stream));
            await Task.Delay(100);
            Assert.IsTrue(gameServers.ContainsKey(processId),
                "OnConnectionEstablished must add server report to gameServers");
            
            SendReport(5);
            await Task.Delay(100);
            Assert.IsTrue(gameServers[processId].Players == 5, "first report failed");

            SendReport(17);
            await Task.Delay(200);
            Assert.IsTrue(gameServers[processId].Players == 17, "second report failed");

            gameServers.TryRemove(processId, out var gameServerStateReport);
            Assert.IsTrue(!gameServers.ContainsKey(processId), "can't remove registred game server");
            
            SendReport(25);
            await Task.Delay(100);
            Assert.IsTrue(gameServers[processId].Players == 25, "report after removing gameServer failed");
 
            var anotherGameServerConnection = new GameServerConnection(gameServers, settings);
            stream.RemotePCStream.WriteMessage(new GameServerStateReport { ProcessID = processId } );
            _ = Task.Run(() => anotherGameServerConnection.MainLoop(stream));
            await Task.Delay(100);

            Assert.IsTrue(gameServers.Count == 1, "game server with the same processId must not be duplicated in gameServers");

            Assert.ThrowsException<NotImplementedException>(() => new GameServerConnection(), "GameServer can't have default constructor");
            
            void SendReport(int playerCount)
            {
                stream.RemotePCStream.WriteMessage(new GameServerStateReport
                {
                    ProcessID = processId,
                    Players = playerCount
                });
            }
        }
    }
}