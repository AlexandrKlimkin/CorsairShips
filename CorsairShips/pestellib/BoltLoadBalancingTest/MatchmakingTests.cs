using System.Collections.Generic;
using System.Linq;
using BoltLoadBalancing;
using BoltLoadBalancing.Logic;
using BoltLoadBalancing.MasterServer;
using MasterServerProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace BoltLoadBalancingTest
{
    [TestClass]
    public class MatchmakingTests
    {
        [TestMethod]
        public void MatchmakingTest()
        {
            var possibleMatches = new Matchmaking().GetPossibleMatches(MatchmakingString, TestMasterServers);
            
            Assert.IsTrue(possibleMatches.Select(x =>
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(x.GameServerStateReport.MatchmakingData))
                .All(x => x["sceneName"] == "dust"));

            Assert.IsTrue(possibleMatches.Select(x =>
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(x.GameServerStateReport.MatchmakingData))
                .All(x => x["level"] == "1"));
            
            Assert.AreEqual(2, possibleMatches.Count);
        }

        internal static string MatchmakingString =>
            JsonConvert.SerializeObject(new Dictionary<string, string>()
            {
                {"level", "1"},
                {"sceneName", "dust"}
            });

        internal static IEnumerable<IMasterServer> TestMasterServers
        {
            get
            {
                var testMatchmakingData = new Dictionary<string, string>()
                {
                    {"level", "1"},
                    {"sceneName", "dust"}
                };

                var testMatchmakingDataWithMoreParams = new Dictionary<string, string>()
                {
                    {"level", "1"},
                    {"sceneName", "dust"},
                    {"gameMode", "defuse"}
                };

                var testMatchmakingDataWithWrongParams = new Dictionary<string, string>()
                {
                    {"level", "1"},
                    {"sceneName", "militia"}
                };

                var masters = new List<IMasterServer>()
                {
                    new MasterServer(new MasterServerConnection(null, null), new MasterServerReport
                    {
                        GameServers = new[]
                        {
                            new GameServerStateReport
                            {
                                MatchmakingData = JsonConvert.SerializeObject(testMatchmakingData)
                            },
                            new GameServerStateReport
                            {
                                MatchmakingData = JsonConvert.SerializeObject(testMatchmakingDataWithWrongParams)
                            }
                        }
                    }),
                    new MasterServer(new MasterServerConnection(null, null), new MasterServerReport()
                    {
                        GameServers = new[]
                        {
                            new GameServerStateReport
                            {
                                MatchmakingData = JsonConvert.SerializeObject(testMatchmakingDataWithMoreParams)
                            }
                        }
                    })
                };
                return masters;
            }
        }
    }
}