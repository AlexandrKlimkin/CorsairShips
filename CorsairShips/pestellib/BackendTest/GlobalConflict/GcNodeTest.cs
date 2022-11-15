using System.Collections.Generic;
using BackendCommon.Code.GlobalConflict;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerShared.GlobalConflict;

namespace BackendTest.GlobalConflict
{
    [TestClass]
    public class GcNodeTest
    {
        [TestMethod]
        public void TestNodeWinPoints()
        {
            var nodeState = new NodeState
            {
                Id = 1,
                CaptureLimit = 1000,
                CaptureBonus = 500,
                CaptureThreshold = 500,
                NeutralThreshold = 300,
                NodeStatus = NodeStatus.Neutral,
                TeamPoints = new Dictionary<string, int>()
            };

            var node = new Node(nodeState);
            node.AddTeamPoints("team1", 10000);
            Assert.AreEqual(1000, nodeState.TeamPoints["team1"]);
            node.AddTeamPoints("team1", 500);
            Assert.AreEqual(1000, nodeState.TeamPoints["team1"]);
            node.AddTeamPoints("team2", 800);
            Assert.AreEqual(200, nodeState.TeamPoints["team1"]);
            Assert.AreEqual(800, nodeState.TeamPoints["team2"]);
            Assert.IsFalse(nodeState.TeamPoints.ContainsKey("team3"));
            node.AddTeamPoints("team3", 123);
            Assert.AreEqual(175, nodeState.TeamPoints["team1"]);
            Assert.AreEqual(702, nodeState.TeamPoints["team2"]);
            Assert.AreEqual(123, nodeState.TeamPoints["team3"]);

            node.AddTeamPoints("team3", 123);
            Assert.AreEqual(150, nodeState.TeamPoints["team1"]);
            Assert.AreEqual(604, nodeState.TeamPoints["team2"]);
            Assert.AreEqual(246, nodeState.TeamPoints["team3"]);

            node.AddTeamPoints("team3", 749);
            Assert.AreEqual(0, nodeState.TeamPoints["team1"]);
            Assert.AreEqual(5, nodeState.TeamPoints["team2"]);
            Assert.AreEqual(995, nodeState.TeamPoints["team3"]);

            node.AddTeamPoints("team3", 10);
            node.AddTeamPoints("team3", 5);
            Assert.AreEqual(0, nodeState.TeamPoints["team1"]);
            Assert.AreEqual(0, nodeState.TeamPoints["team2"]);
            Assert.AreEqual(1000, nodeState.TeamPoints["team3"]);

            node.AddTeamPoints("team2", 10000);
            Assert.AreEqual(0, nodeState.TeamPoints["team1"]);
            Assert.AreEqual(1000, nodeState.TeamPoints["team2"]);
            Assert.AreEqual(0, nodeState.TeamPoints["team3"]);
        }

        [TestMethod]
        public void TestEvalNodeStatus()
        {
            var nodeState = new NodeState
            {
                Id = 1,
                CaptureLimit = 1000,
                CaptureBonus = 500,
                CaptureThreshold = 500,
                NeutralThreshold = 300,
                NodeStatus = NodeStatus.Neutral,
                TeamPoints = new Dictionary<string, int>()
                {
                    ["team1"] = 0,
                    ["team2"] = 200,
                }
            };
            var node = new Node(nodeState);
            Assert.IsFalse(node.EvaluateNodeStatus());
            Assert.IsNull(nodeState.Owner);
            Assert.AreEqual(NodeStatus.Neutral, nodeState.NodeStatus);

            node.AddTeamPoints("team2", 100);
            Assert.IsTrue(node.EvaluateNodeStatus());
            Assert.AreEqual(NodeStatus.Captured, nodeState.NodeStatus);
            Assert.AreEqual("team2", nodeState.Owner);
            Assert.AreEqual(nodeState.CaptureBonus + 300, nodeState.TeamPoints["team2"]); // 800

            node.AddTeamPoints("team1", 800 + nodeState.CaptureThreshold);
            Assert.IsTrue(node.EvaluateNodeStatus());
            Assert.AreEqual(NodeStatus.Captured, nodeState.NodeStatus);
            Assert.AreEqual("team1", nodeState.Owner);
            Assert.AreEqual(nodeState.CaptureLimit, nodeState.TeamPoints["team1"]);
        }
    }
}
