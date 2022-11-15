using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PestelLib.ServerCommon;
using Moq;
using PestelLib.MatchmakerServer;
using PestelLib.MatchmakerServer.Config;
using PestelLib.MatchmakerShared;
using System.IO;
using Nerdbank;
using System.Threading;
using MatchmakerTest.Dummies;
using Microsoft.Extensions.Configuration;
using ServerLib;

namespace MatchmakerTest
{
    /// <summary>
    /// Summary description for MatchmakerServerProtocolTest
    /// </summary>
    [TestClass]
    public class MatchmakerProtocolTest
    {
        public MatchmakerProtocolTest()
        {
            Log.Init();
            string configPath = AppDomain.CurrentDomain.BaseDirectory + "appsettings.json";
            AppSettings.LoadConfig(configPath);
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void ClientRegisterTest()
        {
            ConcurrentBag<Guid> leavers = new ConcurrentBag<Guid>();
            var streams = FullDuplexStream.CreateStreams();
            var clientStream = new MatchmakerMessageStream(streams.Item1);
            var serverStream = new MatchmakerMessageStream(streams.Item2);
            var serverMessageProcessor = new MatchmakerMessageProcessor(serverStream);
            var serverProto = new MatchmakerServerProtocol<MatchmakerRequest, SimpleMatch, MatchingStats>(serverMessageProcessor, new MatchmakerConfig());
            var clientReg1 = new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = new MatchmakerRequest { PlayerId = Guid.NewGuid() } };
            var clientReg2 = new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = new MatchmakerRequest { PlayerId = Guid.NewGuid() } };

            serverProto.UserLeave += (u) => leavers.Add(u);

            serverMessageProcessor.AddSource(serverStream);

            clientStream.Write(clientReg1);
            clientStream.Write(clientReg2);

            Thread.Sleep(100);
            var t = serverProto.GetRequestsAsync();
            Assert.IsTrue(t.Wait(100));
            var requests = t.Result;

            Assert.AreEqual(0, leavers.Count);

            Assert.AreEqual(2, requests.Length);
            Assert.AreEqual(clientReg1.MatchParams.PlayerId, requests[0].PlayerId);
            Assert.AreEqual(clientReg2.MatchParams.PlayerId, requests[1].PlayerId);

            clientStream.Dispose();
            serverStream.Dispose();

            Thread.Sleep(100);
            // many users can seat on one transport layer
            Assert.AreEqual(2, leavers.Count);

            serverProto.Dispose();
            serverMessageProcessor.Dispose();
        }

        [TestMethod]
        public void AnnounceMatchTest()
        {
            ConcurrentBag<Guid> leavers = new ConcurrentBag<Guid>();
            ConcurrentBag<SimpleMatch> hostMatch = new ConcurrentBag<SimpleMatch>();
            ConcurrentBag<SimpleMatch> joinMatch = new ConcurrentBag<SimpleMatch>();
            var streams = FullDuplexStream.CreateStreams();
            var manualEvent1 = new ManualResetEvent(false);
            var manualEvent2 = new ManualResetEvent(false);
            var clientStream = new MatchmakerMessageStream(streams.Item1);
            var serverStream = new MatchmakerMessageStream(streams.Item2);
            var clientMessageProcessor = new MatchmakerMessageProcessor(clientStream);
            var serverMessageProcessor = new MatchmakerMessageProcessor(serverStream);
            var serverProto = new MatchmakerServerProtocol<MatchmakerRequest, SimpleMatch, MatchingStats>(serverMessageProcessor, new MatchmakerConfig());
            var clientProto = new MatchmakerClientProtocol<MatchmakerRequest, SimpleMatch, MatchingStats>(clientMessageProcessor);
            var config = new MatchmakerConfig();
            var strategy = new SimpleStrategy(config);
            var server = new MatchmakerServer(serverProto, strategy, config);
            var clientReg1 = new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = new MatchmakerRequest { PlayerId = Guid.NewGuid() } };
            var clientReg2 = new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = new MatchmakerRequest { PlayerId = Guid.NewGuid() } };
            var clientReg3 = new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = new MatchmakerRequest { PlayerId = Guid.NewGuid() } };
            JoinMatchPromise<SimpleMatch> joinMatchPromise = null;
            Func<JoinMatchPromise<SimpleMatch>, bool?> syncHostMatch = (m) => { hostMatch.Add(m.Match); manualEvent1.Set(); return true; };
            Func<JoinMatchPromise<SimpleMatch>, bool?> asyncHostMatch = (m) => { joinMatchPromise = m; hostMatch.Add(m.Match); manualEvent1.Set(); return null; };

            serverProto.UserLeave += (u) => leavers.Add(u);
            clientProto.OnHostMatch += syncHostMatch;
            clientProto.OnJoinMatch += (m) => { joinMatch.Add(m); if (joinMatch.Count == 3 || joinMatch.Count == 6) { manualEvent2.Set(); } return true; };

            clientProto.Register(clientReg1);
            clientProto.Register(clientReg2);
            clientProto.Register(clientReg3);

            Assert.IsTrue(manualEvent1.WaitOne(10000));
            Assert.AreEqual(1, hostMatch.Count);
            Assert.IsTrue(manualEvent2.WaitOne(10000));
            Assert.AreEqual(3, joinMatch.Count);
            var match = hostMatch.First();
            Assert.AreEqual(joinMatch.ElementAt(0).Id, match.Id);
            Assert.AreEqual(joinMatch.ElementAt(1).Id, match.Id);
            Assert.AreEqual(joinMatch.ElementAt(2).Id, match.Id);
            Assert.AreEqual(joinMatch.ElementAt(0).CountBots, 0);
            Assert.AreEqual(joinMatch.ElementAt(1).CountBots, 0);
            Assert.AreEqual(joinMatch.ElementAt(2).CountBots, 0);

            manualEvent1.Reset();
            manualEvent2.Reset();
            clientProto.OnHostMatch -= syncHostMatch;
            clientProto.OnHostMatch += asyncHostMatch;

            clientProto.Register(clientReg1);
            clientProto.Register(clientReg2);
            clientProto.Register(clientReg3);

            Assert.IsTrue(manualEvent1.WaitOne(10000));
            Assert.AreEqual(2, hostMatch.Count);
            joinMatchPromise.TryAnswer(true);
            Assert.IsTrue(manualEvent2.WaitOne(10000));
            Assert.AreEqual(6, joinMatch.Count);
            match = hostMatch.Last();
            Assert.AreEqual(joinMatch.ElementAt(3).Id, match.Id);
            Assert.AreEqual(joinMatch.ElementAt(4).Id, match.Id);
            Assert.AreEqual(joinMatch.ElementAt(5).Id, match.Id);
            Assert.AreEqual(joinMatch.ElementAt(3).CountBots, 0);
            Assert.AreEqual(joinMatch.ElementAt(4).CountBots, 0);
            Assert.AreEqual(joinMatch.ElementAt(5).CountBots, 0);

            clientStream.Dispose();
            serverStream.Dispose();
            clientMessageProcessor.Dispose();
            serverMessageProcessor.Dispose();
            serverProto.Dispose();
            clientProto.Dispose();
            serverMessageProcessor.Dispose();
            strategy.Dispose();
            server.Dispose();
        }

        [TestMethod]
        public void AnnounceWithRejectTest()
        {
            ConcurrentBag<Guid> leavers = new ConcurrentBag<Guid>();
            ConcurrentBag<SimpleMatch> hostMatch = new ConcurrentBag<SimpleMatch>();
            ConcurrentBag<SimpleMatch> joinMatch = new ConcurrentBag<SimpleMatch>();
            var streams = FullDuplexStream.CreateStreams();
            var manualEvent1 = new ManualResetEvent(false);
            var manualEvent2 = new ManualResetEvent(false);
            var clientStream = new MatchmakerMessageStream(streams.Item1);
            var serverStream = new MatchmakerMessageStream(streams.Item2);
            var clientMessageProcessor = new MatchmakerMessageProcessor(clientStream);
            var serverMessageProcessor = new MatchmakerMessageProcessor(serverStream);
            var serverProto = new MatchmakerServerProtocol<MatchmakerRequest, SimpleMatch, MatchingStats>(serverMessageProcessor, new MatchmakerConfig());
            var clientProto = new MatchmakerClientProtocol<MatchmakerRequest, SimpleMatch, MatchingStats>(clientMessageProcessor);
            var config = new MatchmakerConfig();
            var strategy = new SimpleStrategy(config);
            var server = new MatchmakerServer(serverProto, strategy, config);
            var clientReg1 = new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = new MatchmakerRequest { PlayerId = Guid.NewGuid() } };
            var clientReg2 = new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = new MatchmakerRequest { PlayerId = Guid.NewGuid() } };
            var clientReg3 = new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = new MatchmakerRequest { PlayerId = Guid.NewGuid() } };
            var clientReg4 = new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = new MatchmakerRequest { PlayerId = Guid.NewGuid() } };

            serverProto.UserLeave += (u) => leavers.Add(u);
            clientProto.OnHostMatch += (m) => 
            {
                hostMatch.Add(m.Match);
                if (hostMatch.Count == 1)
                    return false;

                manualEvent1.Set();
                return true;
            };
            clientProto.OnJoinMatch += (m) => 
            {
                joinMatch.Add(m);
                if (joinMatch.Count == 2) { manualEvent2.Set(); }
                return true;
            };

            serverMessageProcessor.AddSource(serverStream);

            clientProto.Register(clientReg1);
            clientProto.Register(clientReg2);
            clientProto.Register(clientReg3);
            clientProto.Register(clientReg4);

            Assert.IsTrue(manualEvent1.WaitOne(10000));
            Assert.AreEqual(2, hostMatch.Count);
            Assert.IsTrue(manualEvent2.WaitOne(10000));
            Assert.AreEqual(2, joinMatch.Count);
            var match = hostMatch.First();
            Assert.AreEqual(match.Id, joinMatch.ElementAt(0).Id);
            Assert.AreEqual(match.Id, joinMatch.ElementAt(1).Id);
            Assert.AreEqual(1, joinMatch.ElementAt(0).CountBots);
            Assert.AreEqual(1, joinMatch.ElementAt(1).CountBots);

            clientStream.Dispose();
            serverStream.Dispose();
            clientMessageProcessor.Dispose();
            serverMessageProcessor.Dispose();
            serverProto.Dispose();
            clientProto.Dispose();
            serverMessageProcessor.Dispose();
            strategy.Dispose();
            server.Dispose();
        }
    }
}
