using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendCommon.Code.GlobalConflict;
using BackendCommon.Code.GlobalConflict.Db;
using BackendCommon.Code.GlobalConflict.Server;
using BackendCommon.Code.GlobalConflict.Server.Stages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PestelLib.ServerCommon.Redis;
using ServerLib;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendTest
{
    [TestClass]
    public class GcBattleStageTest
    {
        [TestInitialize]
        public void Init()
        {
            var disposable = new Mock<IDisposable>();
            var lockManager = new Mock<ILockManager>();
            lockManager.Setup(_ => _.Lock(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(() => disposable.Object);
            lockManager.Setup(_ => _.LockAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(() => Task.FromResult(disposable.Object));
            ContainerHolder.Container.RegisterCustom(() => lockManager.Object);

            AppSettings.Default = new AppSettings();
            AppSettings.Default.GlobalConflictSettings.EnableProfiler = false;
        }

        [TestMethod]
        public async Task TestIdleBattleLoop()
        {
            var player = new PlayerState()
            {
                Id = "user1",
                ConflictId = "conflict1",
                TeamId = "team1"
            };
            var team1State = new TeamState()
            {
                Id = "team1"
            };
            var team2State = new TeamState()
            {
                Id = "team2"
            };
            var globalConflict = new GlobalConflictState()
            {
                Id = "conflict1",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                DonationBonusLevels = new DonationBonusLevels[] { },
                DonationBonuses = new DonationBonus[] { },
                Stages = new StageDef[]
                {
                    new StageDef()
                    {
                        Id = StageType.Donation,
                        Period = TimeSpan.FromHours(1)
                    }
                },
                Map = new MapState()
                {
                    Nodes = new[]
                    {
                        new NodeState
                        {
                            Id = 1,
                            CaptureLimit = 1000,
                            CaptureBonus = 500,
                            CaptureThreshold = 500,
                            NodeStatus = NodeStatus.Neutral,
                            LosePoints = 0,
                            WinPoints = 100,
                            LinkedNodes = new int[]{},
                            TeamPoints = new Dictionary<string, int>()
                            {
                                ["team1"] = 0,
                                ["team2"] = 0,
                            }
                        }
                    }
                },
                Teams = new[] { "team1", "team2" },
                TeamsStates = new[] { team1State, team2State },
                PointsOfInterest = new PointOfInterest[] {}
            };

            var scheduleDb = new Mock<IConflictsScheduleDb>();
            scheduleDb.Setup(_ => _.GetByDateAsync(It.IsAny<DateTime>())).Returns(() => Task.FromResult(globalConflict));
            var battleDb = new Mock<IBattleDb>();
            var playersDb = new Mock<IPlayersDb>();
            playersDb.Setup(_ => _.GetPlayerAsync("user1", "conflict1")).Returns(Task.FromResult(player));
            ContainerHolder.Container.RegisterCustom(() => scheduleDb.Object);
            ContainerHolder.Container.RegisterCustom(() => battleDb.Object);
            ContainerHolder.Container.RegisterCustom(() => playersDb.Object);
            var globalConflictApi = new GlobalConflictServer();
            ContainerHolder.Container.RegisterCustom<GlobalConflictApi>(() => globalConflictApi);
            ContainerHolder.Container.RegisterCustom<GlobalConflictPrivateApi>(() => globalConflictApi);

            var stage = new BattleStage();
            await stage.Update();

            battleDb.Verify(_ => _.GetUnprocessedAsync("conflict1", It.IsAny<int>(), It.IsAny<int>()));
            scheduleDb.Verify(_ => _.SaveAsync(It.IsAny<GlobalConflictState>()));
        }

        [TestMethod]
        public async Task TestSimpleBattle()
        {
            var player1 = new PlayerState()
            {
                Id = "user1",
                ConflictId = "conflict1",
                TeamId = "team1"
            };
            var player1Expected = new PlayerState()
            {
                Id = "user1",
                ConflictId = "conflict1",
                TeamId = "team1",
                WinPoints = 200,
                RegisterTime = player1.RegisterTime
            };
            var player2 = new PlayerState()
            {
                Id = "user2",
                ConflictId = "conflict1",
                TeamId = "team2"
            };
            var player2Expected = new PlayerState()
            {
                Id = "user2",
                ConflictId = "conflict1",
                TeamId = "team2",
                WinPoints = 150,
                RegisterTime = player2.RegisterTime
            };
            var team1State = new TeamState()
            {
                Id = "team1"
            };
            var team1Expected = new TeamState()
            {
                Id = "team1",
                WinPoints = 200
            };
            var team2State = new TeamState()
            {
                Id = "team2"
            };
            var team2Expected = new TeamState()
            {
                Id = "team2",
                WinPoints = 150
            };
            var node = new NodeState
            {
                Id = 1,
                CaptureLimit = 1000,
                CaptureBonus = 500,
                CaptureThreshold = 500,
                NeutralThreshold = 300,
                NodeStatus = NodeStatus.Neutral,
                LosePoints = 50,
                WinPoints = 100,
                LinkedNodes = new int[] { },
                TeamPoints = new Dictionary<string, int>()
                {
                    ["team1"] = 0,
                    ["team2"] = 0,
                }
            };
            var globalConflict = new GlobalConflictState()
            {
                Id = "conflict1",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                DonationBonusLevels = new DonationBonusLevels[] { },
                DonationBonuses = new DonationBonus[] { },
                Stages = new StageDef[]
                {
                    new StageDef()
                    {
                        Id = StageType.Donation,
                        Period = TimeSpan.FromHours(1)
                    }
                },
                Map = new MapState()
                {
                    Nodes = new[] { node }
                },
                Teams = new[] { "team1", "team2" },
                TeamsStates = new[] { team1State, team2State },
                PointsOfInterest = new PointOfInterest[] { }
            };

            var scheduleDb = new Mock<IConflictsScheduleDb>();
            scheduleDb.Setup(_ => _.GetByDateAsync(It.IsAny<DateTime>())).Returns(() => Task.FromResult(globalConflict));
            var battleDb = new Mock<IBattleDb>();
            var playersDb = new Mock<IPlayersDb>();
            var poiDb = new Mock<IPointsOfInterestsDb>();
            playersDb.Setup(_ => _.GetPlayerAsync("user1", "conflict1")).Returns(Task.FromResult(player1));
            playersDb.Setup(_ => _.GetPlayerAsync("user2", "conflict1")).Returns(Task.FromResult(player2));
            var firstSet = new[]
            {
                new BattleResultInfo()
                {
                    ConflictId = "conflict1",
                    LoseMod = 1,
                    WinMod = 1,
                    NodeId = 1,
                    PlayerId = "user1",
                    Time = DateTime.UtcNow,
                    Win = true
                },
                new BattleResultInfo()
                {
                    ConflictId = "conflict1",
                    LoseMod = 1.5m,
                    WinMod = 1,
                    NodeId = 1,
                    PlayerId = "user2",
                    Time = DateTime.UtcNow,
                    Win = false
                }
            };
            var secondSet = new[]
            {
                new BattleResultInfo()
                {
                    ConflictId = "conflict1",
                    LoseMod = 1,
                    WinMod = 3,
                    NodeId = 1,
                    PlayerId = "user2",
                    Time = DateTime.UtcNow,
                    Win = true
                }
            };
            battleDb.Setup(_ => _.GetUnprocessedAsync("conflict1", It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(firstSet));
            ContainerHolder.Container.RegisterCustom(() => scheduleDb.Object);
            ContainerHolder.Container.RegisterCustom(() => battleDb.Object);
            ContainerHolder.Container.RegisterCustom(() => playersDb.Object);
            ContainerHolder.Container.RegisterCustom(() => poiDb.Object);
            var globalConflictApi = new GlobalConflictServer();
            ContainerHolder.Container.RegisterCustom<GlobalConflictApi>(() => globalConflictApi);
            ContainerHolder.Container.RegisterCustom<GlobalConflictPrivateApi>(() => globalConflictApi);

            var stage = new BattleStage();
            await stage.Update();
            await stage.Update();

            DeepEqualityUsingJsonSerialization.ShouldDeepEqual(player1, player1Expected);
            DeepEqualityUsingJsonSerialization.ShouldDeepEqual(player2, player2Expected);
            DeepEqualityUsingJsonSerialization.ShouldDeepEqual(team1State, team1Expected);
            DeepEqualityUsingJsonSerialization.ShouldDeepEqual(team2State, team2Expected);

            Assert.IsNull(node.Owner);
            poiDb.Verify(_ => _.GetByNode("conflict1", "team1", 1));
            poiDb.Verify(_ => _.GetByNode("conflict1", "team2", 1));
            battleDb.Verify(_ => _.GetUnprocessedAsync("conflict1", It.IsAny<int>(), It.IsAny<int>()));
            scheduleDb.Verify(_ => _.SaveAsync(It.IsAny<GlobalConflictState>()));
            playersDb.Verify(_ => _.SaveAsync(It.Is((PlayerState p) => p.Id == "user1")));
            playersDb.Verify(_ => _.SaveAsync(It.Is((PlayerState p) => p.Id == "user2")));

            Assert.AreEqual(200, node.TeamPoints["team1"]);
            Assert.AreEqual(150, node.TeamPoints["team2"]);
            Assert.AreEqual(200, team1State.WinPoints);
            Assert.AreEqual(150, team2State.WinPoints);
            Assert.AreEqual(200, player1.WinPoints);
            Assert.AreEqual(150, player2.WinPoints);
            await stage.Update();
            Assert.AreEqual(node.Owner, "team1");
            Assert.AreEqual(200 + node.WinPoints + node.CaptureBonus, node.TeamPoints["team1"]); // 800
            Assert.AreEqual(200, node.TeamPoints["team2"]);

            Assert.AreEqual(300, team1State.WinPoints);
            Assert.AreEqual(225, team2State.WinPoints);
            Assert.AreEqual(300, player1.WinPoints);
            Assert.AreEqual(225, player2.WinPoints);

            battleDb.Setup(_ => _.GetUnprocessedAsync("conflict1", It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(secondSet));

            await stage.Update();
            Assert.AreEqual(node.Owner, "team1");
            Assert.AreEqual(500, node.TeamPoints["team1"]);
            Assert.AreEqual(500, node.TeamPoints["team2"]);
            Assert.AreEqual(300, team1State.WinPoints);
            Assert.AreEqual(525, team2State.WinPoints);
            Assert.AreEqual(300, player1.WinPoints);
            Assert.AreEqual(525, player2.WinPoints);

            await stage.Update();
            Assert.AreEqual(node.Owner, "team2");
            Assert.AreEqual(0, node.TeamPoints["team1"]);
            Assert.AreEqual(1000, node.TeamPoints["team2"]);
            Assert.AreEqual(300, team1State.WinPoints);
            Assert.AreEqual(825, team2State.WinPoints);
            Assert.AreEqual(300, player1.WinPoints);
            Assert.AreEqual(825, player2.WinPoints);
        }
    }
}
