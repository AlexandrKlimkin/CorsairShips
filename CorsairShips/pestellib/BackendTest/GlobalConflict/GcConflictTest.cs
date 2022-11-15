using System;
using System.Collections.Generic;
using System.Linq;
using BackendCommon.Code.GlobalConflict;
using BackendCommon.Code.GlobalConflict.Server.Stages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerShared.GlobalConflict;
using ServerShared.Sources.GlobalConflict;

namespace BackendTest.GlobalConflict
{
    [TestClass]
    public class GcConflictTest
    {
        [TestMethod]
        public void TestMiscs()
        {
            var team1 = new TeamState()
            {
                Id = "team1",
                WinPoints = 1000,
                DonationPoints = 100,
            };
            var team2 = new TeamState()
            {
                Id = "team2",
                WinPoints = 500,
                DonationPoints = 1000
            };
            var conflictState = new GlobalConflictState()
            {
                TeamsStates = new[]
                {
                    team1, team2
                },
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(4),
                Stages = new []
                {
                    new StageDef()
                    {
                        Id = StageType.Donation,
                        Period = TimeSpan.FromDays(1)
                    },
                    new StageDef()
                    {
                        Id = StageType.Cooldown,
                        Period = TimeSpan.FromDays(1)
                    },
                    new StageDef()
                    {
                        Id = StageType.Battle,
                        Period = TimeSpan.FromDays(1)
                    }
                }
            };
            var conflict = new Conflict(conflictState);
            Assert.IsFalse(conflict.Started);
            Assert.IsFalse(conflict.Finished);
            Assert.IsFalse(conflict.InProgress);
            conflictState.CaptureTime = 3600;
            Assert.AreEqual(0, conflict.CurrentRound());
            conflictState.StartTime = DateTime.UtcNow.AddDays(-1);
            conflictState.EndTime = conflictState.StartTime + conflict.Period;
            Assert.IsTrue(conflict.Started);
            Assert.IsFalse(conflict.Finished);
            Assert.IsTrue(conflict.InProgress);
            conflictState.CaptureTime = 0;
            Assert.AreEqual(TimeSpan.FromDays(3), conflict.Period);
            Assert.AreEqual("team1", conflict.GetWinningTeam().Id);
            var t = team2.WinPoints;
            team2.WinPoints = team1.WinPoints + 1;
            Assert.AreEqual("team2", conflict.GetWinningTeam().Id);
            team2.WinPoints = t;
            Assert.AreEqual(0, conflict.CurrentRound());
            conflictState.CaptureTime = 3600;
            Assert.AreEqual(24, conflict.CurrentRound());
            conflictState.StartTime = DateTime.UtcNow.AddDays(-3);
            conflictState.EndTime = conflictState.StartTime + conflict.Period;
            Assert.IsTrue(conflict.Started);
            Assert.IsTrue(conflict.Finished);
            Assert.IsFalse(conflict.InProgress);
        }

        [TestMethod]
        public void TestDonations()
        {
            var levels = new[]
            {
                new DonationBonusLevels
                {
                    Level = 1,
                    Threshold = 100
                },
                new DonationBonusLevels
                {
                    Level = 2,
                    Threshold = 200
                }
            };
            var bonuses = new[]
            {
                new DonationBonus()
                {
                    Level = 1,
                    ClientType = "valid1"
                },
                new DonationBonus()
                {
                    Level = 2,
                    ClientType = "valid2"
                },
                new DonationBonus()
                {
                    Level = 1,
                    Team = true
                },
            };
            var conflictState = new GlobalConflictState()
            {
                DonationBonusLevels = new DonationBonusLevels[] {},
                DonationBonuses = bonuses,
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow.AddDays(2),
                Stages = new[]
                {
                    new StageDef()
                    {
                        Id = StageType.Donation,
                        Period = TimeSpan.FromDays(1)
                    },
                    new StageDef()
                    {
                        Id = StageType.Cooldown,
                        Period = TimeSpan.FromDays(1)
                    },
                    new StageDef()
                    {
                        Id = StageType.Battle,
                        Period = TimeSpan.FromDays(1)
                    }
                }
            };
            var conflict = new Conflict(conflictState);
            Assert.AreEqual(0, conflict.GetDonationLevel(100, false));
            conflictState.DonationBonusLevels = levels;
            Assert.AreEqual(0, conflict.GetDonationLevel(99, false));
            Assert.AreEqual(1, conflict.GetDonationLevel(100, false));
            Assert.AreEqual(1, conflict.GetDonationLevel(199, false));
            Assert.AreEqual(2, conflict.GetDonationLevel(200, false));
            Assert.AreEqual(2, conflict.GetDonationLevel(300, false));

            Assert.IsFalse(conflict.IsDonationLevelUp(0, 99));
            Assert.IsTrue(conflict.IsDonationLevelUp(99, 1));
            Assert.IsTrue(conflict.IsDonationLevelUp(0, 200));
            Assert.IsFalse(conflict.IsDonationLevelUp(200, 100));

            var b = conflict.GetDonationBonuses(1, false);
            Assert.AreEqual(1, b.Length);
            Assert.AreEqual("valid1", b[0].ClientType);

            b = conflict.GetDonationBonuses(2, false);
            Assert.AreEqual(1, b.Length);
            Assert.AreEqual("valid2", b[0].ClientType);

            b = conflict.GetDonationBonuses(3, false);
            Assert.AreEqual(0, b.Length);
        }

        [TestMethod]
        public void TestPoi()
        {
            var conflictState = new GlobalConflictState()
            {
                DonationBonusLevels = new DonationBonusLevels[] { },
                DonationBonuses = new DonationBonus[] {},
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow.AddDays(2),
                Stages = new[]
                {
                    new StageDef()
                    {
                        Id = StageType.Donation,
                        Period = TimeSpan.FromDays(1)
                    },
                    new StageDef()
                    {
                        Id = StageType.Cooldown,
                        Period = TimeSpan.FromDays(1)
                    },
                    new StageDef()
                    {
                        Id = StageType.Battle,
                        Period = TimeSpan.FromDays(1)
                    }
                },
                Teams = new []
                {
                    "team1"
                },
                Map = new MapState()
                {
                    Nodes = new NodeState[] {}
                }
            };
            var nodesStates = new NodeState[]
            {
                new NodeState()
                {
                    Id = 1,
                    CaptureLimit = 1000,
                    CaptureBonus = 500,
                    CaptureThreshold = 500,
                    NeutralThreshold = 300,
                    NodeStatus = NodeStatus.Base,
                    LosePoints = 50,
                    WinPoints = 100,
                    LinkedNodes = new [] {2 },
                    TeamPoints = new Dictionary<string, int>()
                    {
                        ["team1"] = 0,
                        ["team2"] = 0
                    }
                },
                new NodeState()
                {
                    Id = 2,
                    CaptureLimit = 1000,
                    CaptureBonus = 500,
                    CaptureThreshold = 500,
                    NeutralThreshold = 300,
                    NodeStatus = NodeStatus.Captured,
                    Owner = "team1",
                    LosePoints = 50,
                    WinPoints = 100,
                    LinkedNodes = new [] { 1, 3, 4 },
                    TeamPoints = new Dictionary<string, int>()
                    {
                        ["team1"] = 0,
                        ["team2"] = 0
                    }
                },
                new NodeState()
                {
                    Id = 3,
                    CaptureLimit = 1000,
                    CaptureBonus = 500,
                    CaptureThreshold = 500,
                    NeutralThreshold = 300,
                    NodeStatus = NodeStatus.Captured,
                    Owner = "team2",
                    LosePoints = 50,
                    WinPoints = 100,
                    LinkedNodes = new [] { 2, 4 },
                    TeamPoints = new Dictionary<string, int>()
                    {
                        ["team1"] = 100,
                        ["team2"] = 0
                    }
                },
                new NodeState()
                {
                    Id = 4,
                    CaptureLimit = 1000,
                    CaptureBonus = 500,
                    CaptureThreshold = 500,
                    NeutralThreshold = 300,
                    NodeStatus = NodeStatus.Captured,
                    Owner = "team2",
                    LosePoints = 50,
                    WinPoints = 100,
                    LinkedNodes = new [] { 2, 3,5 },
                    TeamPoints = new Dictionary<string, int>()
                    {
                        ["team1"] = 0,
                        ["team2"] = 0
                    }
                },
                new NodeState()
                {
                    Id = 5,
                    CaptureLimit = 1000,
                    CaptureBonus = 500,
                    CaptureThreshold = 500,
                    NeutralThreshold = 300,
                    NodeStatus = NodeStatus.Base,
                    Owner = "team2",
                    LosePoints = 50,
                    WinPoints = 100,
                    LinkedNodes = new [] { 3 },
                    TeamPoints = new Dictionary<string, int>()
                    {
                        ["team1"] = 0,
                        ["team2"] = 0
                    }
                }
            };
            var conflict = new Conflict(conflictState);
            var nodes = conflictState.GetReachableNodes("team1").ToArray();
            var picker = new DefaultPointOfInterestNodePicker();
            var pickedNode = picker.PickEasyReachableNode(conflict, "team1");

            Assert.AreEqual(0, nodes.Length);
            Assert.IsNull(pickedNode);

            conflictState.Map.Nodes = nodesStates;

            nodes = conflictState.GetReachableNodes("team1").ToArray();
            pickedNode = picker.PickEasyReachableNode(conflict, "team1");
            Assert.AreEqual(2, nodes.Length);
            foreach (var nodeState in nodes)
            {
                Assert.IsTrue(nodeState.Id == 3 || nodeState.Id == 4);
            }
            Assert.IsNotNull(pickedNode);
            Assert.AreEqual(3, pickedNode.Id);
            pickedNode = picker.PickRandomReachableNode(conflict, "team1");
            Assert.IsTrue(pickedNode.Id == 3 || pickedNode.Id == 4);
        }
    }
}
