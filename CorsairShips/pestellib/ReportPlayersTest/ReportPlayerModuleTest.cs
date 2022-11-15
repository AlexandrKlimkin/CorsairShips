using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PestelLib.UniversalSerializer;
using ReportPlayers;
using ReportPlayersProtocol;
using S;

namespace ReportPlayersTest
{
    [TestClass]
    public class ReportPlayerModuleTest
    {
        [TestMethod]
        public void AddOneReport()
        {
            var request = new PlayerReportData
            {
                Sender = Guid.NewGuid(),
                Reported = Guid.NewGuid(),
                Type = "UndefinedType",
                Description = "One shot everyone",
                ReportedBySystem = false
            };
            var requestSerialized = Serializer.Serialize<BaseReportRequest>(request);
            var reportModule = new ReportPlayerModule(new MongoReportsStorage("mongodb://localhost:27017"));
            var response = reportModule.ProcessRequest(requestSerialized);

            Assert.IsNotNull(response);
            Assert.AreEqual(ResponseCode.OK, response.ResponseCode);
        }

        [TestMethod]
        public void ReportNewSession()
        {
            var playerId = Guid.NewGuid();
            var request = new RegisterNewSession {PlayerId = playerId};
            var requestSerialized = Serializer.Serialize<BaseReportRequest>(request);
            var storage = new MongoReportsStorage("mongodb://localhost:27017");
            var reportModule = new ReportPlayerModule(storage);
            const int sessionCounter = 5;
            for (var i = 0; i < sessionCounter; i++)
            {
                var response = reportModule.ProcessRequest(requestSerialized);
                Assert.IsNotNull(response);
                Assert.IsTrue(response.ResponseCode == ResponseCode.OK);
            }

            var playerCounters = storage.GetCounterData(playerId);
            Assert.IsNotNull(playerCounters);
            Assert.AreEqual(sessionCounter, playerCounters.SessionCounter);
        }

        [TestMethod]
        public void SendReportTwice()
        {
            var request = new PlayerReportData
            {
                Sender = Guid.NewGuid(),
                Reported = Guid.NewGuid(),
                Type = "UndefinedType",
                Description = "One shot everyone",
                ReportedBySystem = false
            };
            var requestSerialized = Serializer.Serialize<BaseReportRequest>(request);
            var reportModule = new ReportPlayerModule(new MongoReportsStorage("mongodb://localhost:27017"));
            var response = reportModule.ProcessRequest(requestSerialized);
            Assert.IsNotNull(response);
            Assert.AreEqual(ResponseCode.OK, response.ResponseCode);
            response = reportModule.ProcessRequest(requestSerialized);
            Assert.IsNotNull(response);
            Assert.AreEqual(ResponseCode.OK, response.ResponseCode);
        }

        [TestMethod]
        public void TestSessionCounters()
        {
            var playerGuid = Guid.NewGuid();
            var storage = new MongoReportsStorage("mongodb://localhost:27017");
            storage.IncrementSessionCounter(playerGuid, "testPlayerName");
            storage.IncrementSessionCounter(playerGuid, "testPlayerName");

            storage.RegisterReport(new PlayerReportData
            {
                Sender = Guid.NewGuid(),
                Reported = playerGuid,
                Description = "desc",
                Type = "type"
            });

            var counters = storage.GetCounterData(playerGuid);

            Assert.AreEqual(2, counters.SessionCounter);
            Assert.AreEqual(1, counters.ReportsCounter);
            Assert.AreEqual(0.5f, counters.ReportsPerSession);
        }

        [TestMethod]
        public async Task TestGetReportsAsync()
        {
            var playerGuid = Guid.NewGuid();
            var storage = new MongoReportsStorage("mongodb://localhost:27017");

            const int sessions = 10;
            for (var i = 0; i < sessions; i++)
            {
                storage.IncrementSessionCounter(playerGuid, "testPlayerName");
            }

            const int reports = 200;
            for (var i = 0; i < reports; i++ ){
                storage.RegisterReport(new PlayerReportData
                {
                    Sender = Guid.NewGuid(),
                    Reported = playerGuid,
                    Description = "desc",
                    Type = "type"
                });
            }

            var cheaterReports = await storage.GetReportsByReportedAsync(playerGuid, 1024);

            Assert.AreEqual(reports, cheaterReports.Count());

            var playerCounters = storage.GetCounterData(playerGuid);
            Assert.AreEqual(reports, playerCounters.ReportsCounter);
            Assert.AreEqual(sessions, playerCounters.SessionCounter);

            var reportRate = (float)playerCounters.ReportsCounter / playerCounters.SessionCounter;
            Assert.AreEqual(reportRate, playerCounters.ReportsPerSession);
        }
    }
}
