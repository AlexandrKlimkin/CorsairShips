using System;
using System.Threading.Tasks;
using BoltTestLibrary.Mock;
using BoltTransport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoltTransportTest
{
    [TestClass]
    public class TestClientServerInteraction
    {
        [TestMethod]
        public async Task PingPongTest()
        {
            using var _ = new Server<PingPongConnection>(5678);
            using var client = new Client<PingPongConnection>("127.0.0.1", 5678);
            
            var ping = new Ping();
            var pong = await client.SendMessageAsync<Pong>(ping);
            Assert.AreEqual(ping.MessageId, pong.MessageId);

            var pong2 = await client.Connection.Ping();
            Assert.IsNotNull(pong2);
        }
        
        [TestMethod]
        public async Task PingPongTestWithExternalConnectionConstructor()
        {
            using var _ = new Server<PingPongConnection>(5679, () => new PingPongConnection());
            using var client = new Client<PingPongConnection>(new PingPongConnection(), "127.0.0.1", 5679); 
            
            var ping = new Ping();
            var pong = await client.SendMessageAsync<Pong>(ping);
            Assert.AreEqual(ping.MessageId, pong.MessageId);
        }
        
        [TestMethod]
        public async Task PingPongTestReconnect()
        {
            async Task StartAnotherServerWithDelay()
            {
                await Task.Delay(5000);
                
                Console.WriteLine("Second server started at: " + DateTime.Now);
                var secondServer = new Server<PingPongConnection>(
                    5680, () => new PingPongConnection());
            }
            
            Console.WriteLine("First server started at: " + DateTime.Now);
            var firstServer = new Server<PingPongConnection>(5680, () => new PingPongConnection());
            
            using var client = new Client<PingPongConnection>(new PingPongConnection(), "127.0.0.1", 5680); 
            
            var ping = new Ping();
            var pong = await client.SendMessageAsync<Pong>(ping);
            Assert.AreEqual(ping.MessageId, pong.MessageId);
            
            firstServer.Dispose();
            
            _ = Task.Run(StartAnotherServerWithDelay);
            
            Console.WriteLine("Send message begin: " + DateTime.Now);
            var pong2 = await client.SendMessageAsync<Pong>(ping);
            Console.WriteLine("Send message finish: " + DateTime.Now);
            Assert.AreEqual(ping.MessageId, pong2.MessageId);
            
        }
        
        [TestMethod]
        public async Task DuplicatedMessageId()
        {
            using var server = new Server<PingPongConnection>(5681);
            using var client = new Client<PingPongConnection>("127.0.0.1", 5681);
            
            var ping = new Ping();
            _ = client.SendMessageAsync<Pong>(ping);
            var pong = await client.SendMessageAsync<Pong>(ping);

            Assert.AreEqual(ping.MessageId, pong.MessageId);
        }
    }
}