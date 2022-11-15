using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nerdbank;
using PestelLib.MatchmakerShared;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace MatchmakerTest
{
    [TestClass]
    public class MatchmakerMessageStreamTest
    {
        [TestMethod]
        public void SendReceiveTest()
        {
            var streams = FullDuplexStream.CreateStreams();
            var client = new MatchmakerMessageStream(streams.Item1);
            var server = new MatchmakerMessageStream(streams.Item2);
            var req = new MatchmakerRequest() { PlayerId = Guid.NewGuid(), IsBot = true, RegTime = DateTime.UtcNow, TeamId = 1 };

            client.Write(new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams =  req});
            var msg = ((ClientMatchmakerRegister<MatchmakerRequest>)server.Read()).MatchParams;

            Assert.AreEqual(req.PlayerId, msg.PlayerId);
            Assert.AreEqual(req.IsBot, msg.IsBot);
            Assert.AreEqual(req.RegTime, msg.RegTime);
            Assert.AreEqual(req.TeamId, msg.TeamId);

            client.Dispose();
            server.Dispose();
        }

        [TestMethod]
        public void SimultaneousRwTest()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 0);
            tcpListener.Start();
            var port = ((IPEndPoint)tcpListener.Server.LocalEndPoint).Port;
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, port);
            var tcpClientOnServer = tcpListener.AcceptTcpClient();

            int repeat = 100;
            const int count = 100;
            var client = new MatchmakerMessageStream(tcpClient.GetStream());
            var server = new MatchmakerMessageStream(tcpClientOnServer.GetStream());
            var req = new MatchmakerRequest() { PlayerId = Guid.NewGuid(), IsBot = true, RegTime = DateTime.UtcNow, TeamId = 1 };
            var acceptMatch = new ClientHostMatchAccepted() { MatchId = Guid.NewGuid(), Acceptance = false };

            while (repeat-- > 0)
            {

                var threads = new List<Thread>();

                ConcurrentBag<int> writeThreads = new ConcurrentBag<int>();
                for (var i = 0; i < count; ++i)
                {
                    var msg = i % 2 == 0 ? new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = req } : (MatchmakerMessage)acceptMatch;
                    threads.Add(
                        new Thread(() =>
                        {
                            writeThreads.Add(Thread.CurrentThread.ManagedThreadId);
                            client.Write(msg);
                        }));
                }
                for (var i = 0; i < count; ++i)
                {
                    threads[i].Start();
                }
                ConcurrentBag<int> readThreads = new ConcurrentBag<int>();
                for (var i = 0; i < count; ++i)
                {
                    var k = i;
                    threads.Add(
                        new Thread(() =>
                        {

                            readThreads.Add(Thread.CurrentThread.ManagedThreadId);

                            var msg = server.Read();
                            if (msg is ClientMatchmakerRegister<MatchmakerRequest>)
                            {
                                var msgR = (msg as ClientMatchmakerRegister<MatchmakerRequest>).MatchParams;
                                Assert.AreEqual(req.PlayerId, msgR.PlayerId);
                                Assert.AreEqual(req.IsBot, msgR.IsBot);
                                Assert.AreEqual(req.RegTime, msgR.RegTime);
                                Assert.AreEqual(req.TeamId, msgR.TeamId);
                            }
                            else if(msg is ClientHostMatchAccepted)
                            {
                                var msgA = msg as ClientHostMatchAccepted;
                                Assert.AreEqual(acceptMatch.MatchId, msgA.MatchId);
                                Assert.AreEqual(acceptMatch.Acceptance, msgA.Acceptance);
                            }
                        })
                        );
                }
                for (var i = 0; i < count; ++i)
                {
                    threads[count + i].Start();
                }

                for (var i = 0; i < threads.Count; ++i)
                {
                    threads[i].Join();
                }
            }

            client.Dispose();
            server.Dispose();
        }
    }
}
