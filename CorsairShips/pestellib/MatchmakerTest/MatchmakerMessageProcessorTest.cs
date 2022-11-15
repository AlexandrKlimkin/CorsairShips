using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PestelLib.MatchmakerShared;
using Nerdbank;
using System.Threading;
using PestelLib.ServerCommon;
using Moq;

namespace MatchmakerTest
{
    [TestClass]
    public class MatchmakerMessageProcessorTest
    {
        [TestInitialize]
        public void Init()
        {
            Log.Init();
        }

        [TestMethod]
        public void MainFlowTest()
        {
            ManualResetEventSlim manualReset = new ManualResetEventSlim(false);
            var streams = FullDuplexStream.CreateStreams();
            var s = new MatchmakerMessageStream(streams.Item1);
            var c = new MatchmakerMessageStream(streams.Item2);
            var processor = new MatchmakerMessageProcessor(s);
            var req = new MatchmakerRequest() { PlayerId = Guid.NewGuid(), IsBot = true, RegTime = DateTime.UtcNow, TeamId = 1 };
            var match = new ClientHostMatchAccepted() { MatchId = Guid.NewGuid(), Acceptance = true };
            MatchmakerRequest req1 = null;
            var msg = new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = req };
            using (
            processor.RegisterCallback<ClientMatchmakerRegister<MatchmakerRequest>>((r, id) =>
            {
                req1 = r.MatchParams;

                manualReset.Set();

                return match;
            }))
            {
                c.Write(msg);
                Assert.IsTrue(manualReset.Wait(1000));

                Thread.Sleep(100);

                Assert.AreEqual(req.PlayerId, req1.PlayerId);
                Assert.AreEqual(req.IsBot, req1.IsBot);
                Assert.AreEqual(req.RegTime, req1.RegTime);
                Assert.AreEqual(req.TeamId, req1.TeamId);

                var message = (ClientHostMatchAccepted)c.Read();

                Assert.AreEqual(match.MatchId, message.MatchId);
                Assert.AreEqual(match.Acceptance, message.Acceptance);
            }
        }

        [TestMethod]
        public void UnknownMessageTest()
        {
            ManualResetEventSlim manualReset = new ManualResetEventSlim(false);
            var streams = FullDuplexStream.CreateStreams();
            var s = new MatchmakerMessageStream(streams.Item1);
            var c = new MatchmakerMessageStream(streams.Item2);
            var processor = new MatchmakerMessageProcessor(s);
            var req = new MatchmakerRequest() { PlayerId = Guid.NewGuid(), IsBot = true, RegTime = DateTime.UtcNow, TeamId = 1 };
            var match = new ClientHostMatchAccepted() { MatchId = Guid.NewGuid(), Acceptance = true };
            var msg = new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = req };

            // MatchmakerMessageProcessor must close source of unknown message immediately
            // we dont register ClientMatchmakerRegister handler so it's unknown message
            c.Write(msg);
            processor.OnClose += (l) => manualReset.Set();

            Assert.IsTrue(manualReset.Wait(100));
        }
    }
}
