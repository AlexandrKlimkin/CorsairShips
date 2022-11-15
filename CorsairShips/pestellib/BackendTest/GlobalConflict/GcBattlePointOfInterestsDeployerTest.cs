using System;
using System.Collections.Generic;
using System.Linq;
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

namespace BackendTest.GlobalConflict
{
    [TestClass]
    public class GcBattlePointOfInterestsDeployerTest
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
        public async Task TestLoadPointsState()
        {
            var deployed = new PointOfInterest()
            {
                Id = "auto_poi1",
                OwnerTeam = "team1",
                BonusTime = TimeSpan.FromMinutes(30),
                DeployCooldown = TimeSpan.FromHours(1),
                NextDeploy = DateTime.UtcNow.AddHours(1),
                Auto = true
            };
            var deployedExpired = new PointOfInterest()
            {
                Id = "auto_poi1",
                OwnerTeam = "team1",
                BonusTime = TimeSpan.FromMinutes(30),
                DeployCooldown = TimeSpan.FromHours(1),
                NextDeploy = DateTime.UtcNow.AddHours(-1),
                Auto = true
            };
            var globalConflict = new GlobalConflictState()
            {
                Id = "conflict1",
                PointsOfInterest = new[]
                {
                    new PointOfInterest()
                    {
                        Id = "auto_poi1",
                        BonusTime = TimeSpan.FromMinutes(30),
                        DeployCooldown = TimeSpan.FromHours(1),
                        Auto = true
                    },
                    new PointOfInterest()
                    {
                        Id = "poi2",
                        BonusTime = TimeSpan.FromMinutes(1),
                        DeployCooldown = TimeSpan.FromHours(1),
                        Auto = false
                    }
                },
                Teams = new[] {"team1","team2"}
            };

            var scheduleDb = new Mock<IPointsOfInterestsDb>();
            ContainerHolder.Container.RegisterCustom(() => scheduleDb.Object);
            var globalConflictApi = new GlobalConflictServer();
            ContainerHolder.Container.RegisterCustom<GlobalConflictApi>(() => globalConflictApi);
            ContainerHolder.Container.RegisterCustom<GlobalConflictPrivateApi>(() => globalConflictApi);
            var deployer = new BattlePointOfInterestsDeployer();

            var r = await deployer.LoadPointsState(globalConflict);

            scheduleDb.Verify(_ => _.GetByIdAsync("conflict1", "auto_poi1", "team1"));
            scheduleDb.Verify(_ => _.GetByIdAsync("conflict1", "auto_poi1", "team2"));
            scheduleDb.VerifyNoOtherCalls();

            Assert.AreEqual(1, r.protos.Length);
            Assert.AreEqual(0, r.deployed.Count);
            Assert.AreEqual("auto_poi1", r.protos.First().Id);

            scheduleDb.Setup(_ => _.GetByIdAsync("conflict1", "auto_poi1", "team1")).Returns(Task.FromResult(deployed));

            r = await deployer.LoadPointsState(globalConflict);

            Assert.AreEqual(1, r.protos.Length);
            Assert.AreEqual(1, r.deployed.Count);
            Assert.IsTrue(r.deployed.HasTeamPoi("auto_poi1", "team1"));

            scheduleDb.Setup(_ => _.GetByIdAsync("conflict1", "auto_poi1", "team1")).Returns(Task.FromResult(deployedExpired));

            r = await deployer.LoadPointsState(globalConflict);

            Assert.AreEqual(1, r.protos.Length);
            Assert.AreEqual(0, r.deployed.Count);
        }

        [TestMethod]
        public async Task TestAutoDeployPointsOfInterests()
        {
            var deployed = new PointOfInterest()
            {
                Id = "auto_poi1",
                BonusTime = TimeSpan.FromMinutes(30),
                DeployCooldown = TimeSpan.FromHours(1),
                NextDeploy = DateTime.UtcNow.AddHours(1),
                OwnerTeam = "team1",
                Auto = true
            };
            var deployedExpired = new PointOfInterest()
            {
                Id = "auto_poi1",
                BonusTime = TimeSpan.FromMinutes(30),
                DeployCooldown = TimeSpan.FromHours(1),
                NextDeploy = DateTime.UtcNow.AddHours(-1),
                OwnerTeam = "team1",
                Auto = true
            };
            var globalConflict = new GlobalConflictState()
            {
                Id = "conflict1",
                TeamsStates = new []
                {
                    new TeamState()
                    {
                        Id = "team1",
                        WinPoints = 100
                    },
                    new TeamState()
                    {
                        Id = "team2",
                    },
                },
                Stages = new StageDef[]
                {
                    new StageDef()
                    {
                        Id = StageType.Battle,
                        Period = TimeSpan.FromHours(1)
                    }
                },
                Teams = new [] {"team1", "team2"},
                PointsOfInterest = new[]
                {
                    new PointOfInterest()
                    {
                        Id = "auto_poi1",
                        BonusTime = TimeSpan.FromMinutes(30),
                        DeployCooldown = TimeSpan.FromHours(1),
                        Auto = true
                    },
                    new PointOfInterest()
                    {
                        Id = "auto_poi2",
                        BonusTime = TimeSpan.FromMinutes(30),
                        DeployCooldown = TimeSpan.FromHours(1),
                        Auto = true
                    },
                    new PointOfInterest()
                    {
                        Id = "poi2",
                        BonusTime = TimeSpan.FromMinutes(1),
                        DeployCooldown = TimeSpan.FromHours(1),
                        Auto = false
                    }
                }
            };
            var scheduleDb = new Mock<IPointsOfInterestsDb>();
            ContainerHolder.Container.RegisterCustom(() => scheduleDb.Object);
            var globalConflictApi = new GlobalConflictServer();
            ContainerHolder.Container.RegisterCustom<GlobalConflictApi>(() => globalConflictApi);
            ContainerHolder.Container.RegisterCustom<GlobalConflictPrivateApi>(() => globalConflictApi);
            var deployer = new Mock<BattlePointOfInterestsDeployer>();
            deployer.CallBase = true;
            deployer.Setup(_ => _.DeployForTeams(It.IsAny<GlobalConflictState>(), It.IsAny<PointOfInterest>(),
                It.IsAny<List<string>>(), It.IsAny<DeployedPointsOfInterest>())).Returns(Task.CompletedTask);

            var conflict = new Conflict(globalConflict);
            await deployer.Object.AutoDeployPointsOfInterests(conflict);

            scheduleDb.Setup(_ => _.GetByIdAsync("conflict1", "auto_poi1", "team1")).Returns(Task.FromResult(deployed));

            await deployer.Object.AutoDeployPointsOfInterests(conflict);

            deployer.Verify(_ => _.DeployForTeams(
                It.Is((GlobalConflictState c) => c.Id == "conflict1"),
                It.Is((PointOfInterest poi) => poi.Id == "auto_poi1"),
                It.Is((List<string> teams) => teams.Count == 1 && teams[0] == "team2"),
                It.Is((DeployedPointsOfInterest autoPoints) => autoPoints.Count == 1)), Times.Exactly(1));

            deployer.Verify(_ => _.DeployForTeams(
                It.Is((GlobalConflictState c) => c.Id == "conflict1"),
                It.Is((PointOfInterest poi) => poi.Id == "auto_poi2"),
                It.Is((List<string> teams) => teams.Count == 2 && teams.All(t => globalConflict.Teams.Any(ts => ts == t))),
                It.Is((DeployedPointsOfInterest autoPoints) => autoPoints.Count == 1)), Times.Exactly(1));


            scheduleDb.Setup(_ => _.GetByIdAsync("conflict1", "auto_poi1", "team1")).Returns(Task.FromResult(deployedExpired));
            await deployer.Object.AutoDeployPointsOfInterests(conflict);

            deployer.Verify(_ => _.DeployForTeams(
                It.Is((GlobalConflictState c) => c.Id == "conflict1"),
                It.Is((PointOfInterest poi) => poi.Id == "auto_poi1"),
                It.Is((List<string> teams) => teams.Count == 2 && teams.All(t => globalConflict.TeamsStates.Any(ts => ts.Id == t))),
                It.Is((DeployedPointsOfInterest autoPoints) => autoPoints.Count == 0)), Times.Exactly(2));

            deployer.Verify(_ => _.DeployForTeams(
                It.Is((GlobalConflictState c) => c.Id == "conflict1"),
                It.Is((PointOfInterest poi) => poi.Id == "auto_poi2"),
                It.Is((List<string> teams) => teams.Count == 2 && teams.All(t => globalConflict.Teams.Any(ts => ts == t))),
                It.Is((DeployedPointsOfInterest autoPoints) => autoPoints.Count == 0)), Times.Exactly(2));
        }

        [TestMethod]
        public async Task TestDeployForTeams()
        {
            var deployed1 = new PointOfInterest()
            {
                Id = "auto_poi1",
                OwnerTeam = "team1",
                NodeId = 2,
                NextDeploy = DateTime.UtcNow.AddHours(1),
                BonusTime = TimeSpan.FromMinutes(30),
                DeployCooldown = TimeSpan.FromHours(1),
                Auto = true
            };
            var deployed2 = new PointOfInterest()
            {
                Id = "auto_poi2",
                OwnerTeam = "team2",
                NodeId = 2,
                NextDeploy = DateTime.UtcNow.AddHours(1),
                BonusTime = TimeSpan.FromMinutes(30),
                DeployCooldown = TimeSpan.FromHours(1),
                Auto = true
            };
            var autoPoi1 = new PointOfInterest()
            {
                Id = "auto_poi1",
                BonusTime = TimeSpan.FromMinutes(30),
                DeployCooldown = TimeSpan.FromHours(1),
                Auto = true
            };
            var autoPoi2 = new PointOfInterest()
            {
                Id = "auto_poi2",
                BonusTime = TimeSpan.FromMinutes(30),
                DeployCooldown = TimeSpan.FromHours(1),
                Auto = true
            };
            var globalConflict = new GlobalConflictState()
            {
                Id = "conflict1",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                MaxPointsOfInterestAtNode = 1,
                MaxSameTypePointsOfInterestAtNode = 1,
                TeamsStates = new[]
                {
                    new TeamState()
                    {
                        Id = "team1",
                        WinPoints = 100
                    },
                    new TeamState()
                    {
                        Id = "team2",
                    },
                },
                Stages = new StageDef[]
                {
                    new StageDef()
                    {
                        Id = StageType.Battle,
                        Period = TimeSpan.FromHours(1)
                    }
                },
                Teams = new[] { "team1", "team2" },
                PointsOfInterest = new[]
                {
                    autoPoi1, autoPoi2,
                    new PointOfInterest()
                    {
                        Id = "poi2",
                        BonusTime = TimeSpan.FromMinutes(1),
                        DeployCooldown = TimeSpan.FromHours(1),
                        Auto = false
                    }
                },
                Map = new MapState()
                {
                    Nodes = new[]
                    {
                        new NodeState()
                        {
                            Id = 1,
                            NodeStatus = NodeStatus.Captured,
                            Owner = "team1",
                            LinkedNodes = new [] { 2,3}
                        },
                        new NodeState()
                        {
                            Id = 2,
                            NodeStatus = NodeStatus.Neutral,
                            LinkedNodes = new [] {1, 4}
                        },
                        new NodeState()
                        {
                            Id = 3,
                            NodeStatus = NodeStatus.Neutral,
                            LinkedNodes = new [] {1,4}
                        },
                        new NodeState()
                        {
                            Id = 4,
                            NodeStatus = NodeStatus.Captured,
                            Owner = "team2",
                            LinkedNodes = new [] { 2,3}
                        },

                    }
                }
            };
            var picker = new DefaultPointOfInterestNodePicker();
            var poiDb = new Mock<IPointsOfInterestsDb>();
            var conflictSchedDb = new Mock<IConflictsScheduleDb>();
            ContainerHolder.Container.RegisterCustom(() => poiDb.Object);
            ContainerHolder.Container.RegisterCustom(() => conflictSchedDb.Object);
            ContainerHolder.Container.RegisterCustom<IPointOfInterestNodePicker>(() => picker);
            var globalConflictApi = new GlobalConflictServer();
            ContainerHolder.Container.RegisterCustom<GlobalConflictApi>(() => globalConflictApi);
            ContainerHolder.Container.RegisterCustom<GlobalConflictPrivateApi>(() => globalConflictApi);
            var deployer = new BattlePointOfInterestsDeployer();
            var deployedPoints = new DeployedPointsOfInterest();

            conflictSchedDb.Setup(_ => _.GetByDateAsync(It.IsAny<DateTime>())).Returns(Task.FromResult(globalConflict));

            var node = autoPoi1.NodeId;
            var st = autoPoi1.NextDeploy;
            await deployer.DeployForTeams(globalConflict, autoPoi1, globalConflict.Teams.ToList(), deployedPoints);
            Assert.AreEqual(node, autoPoi1.NodeId);
            Assert.AreEqual(st, autoPoi1.NextDeploy);
            Assert.AreEqual(2, deployedPoints.Count);
            poiDb.Verify(_ => _.InsertAsync("conflict1", null, "team1", It.IsIn(2,3), It.Is((PointOfInterest poi) => poi.Id == "auto_poi1"), It.IsAny<DateTime>()), Times.Exactly(1));
            poiDb.Verify(_ => _.InsertAsync("conflict1", null, "team2", It.IsIn(2,3), It.Is((PointOfInterest poi) => poi.Id == "auto_poi1"), It.IsAny<DateTime>()), Times.Exactly(1));

            await deployer.DeployForTeams(globalConflict, autoPoi2, globalConflict.Teams.ToList(), deployedPoints);
            Assert.AreEqual(4, deployedPoints.Count);
            poiDb.Verify(_ => _.InsertAsync("conflict1", null, "team1", It.IsIn(2, 3), It.Is((PointOfInterest poi) => poi.Id == "auto_poi2"), It.IsAny<DateTime>()), Times.Exactly(1));
            poiDb.Verify(_ => _.InsertAsync("conflict1", null, "team2", It.IsIn(2, 3), It.Is((PointOfInterest poi) => poi.Id == "auto_poi2"), It.IsAny<DateTime>()), Times.Exactly(1));

            var team1pois = deployedPoints.GetTeamPois("team1");
            var team2pois = deployedPoints.GetTeamPois("team2");

            Assert.AreEqual(2, team1pois.Select(_ => _.Id).Distinct().Count());
            Assert.AreEqual(2, team2pois.Select(_ => _.Id).Distinct().Count());
        }

        [TestMethod]
        public async Task TestDeployForTeamsNoNodes()
        {
            var deployed1 = new PointOfInterest()
            {
                Id = "auto_poi1",
                OwnerTeam = "team1",
                NodeId = 2,
                NextDeploy = DateTime.UtcNow.AddHours(1),
                BonusTime = TimeSpan.FromMinutes(30),
                DeployCooldown = TimeSpan.FromHours(1),
                Auto = true
            };
            var autoPoi1 = new PointOfInterest()
            {
                Id = "auto_poi1",
                BonusTime = TimeSpan.FromMinutes(30),
                DeployCooldown = TimeSpan.FromHours(1),
                Auto = true
            };
            var autoPoi2 = new PointOfInterest()
            {
                Id = "auto_poi2",
                BonusTime = TimeSpan.FromMinutes(30),
                DeployCooldown = TimeSpan.FromHours(1),
                Auto = true
            };
            var globalConflict = new GlobalConflictState()
            {
                Id = "conflict1",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                MaxPointsOfInterestAtNode = 1,
                MaxSameTypePointsOfInterestAtNode = 1,
                TeamsStates = new[]
                {
                    new TeamState()
                    {
                        Id = "team1",
                        WinPoints = 100
                    },
                    new TeamState()
                    {
                        Id = "team2",
                    },
                },
                Stages = new StageDef[]
                {
                    new StageDef()
                    {
                        Id = StageType.Battle,
                        Period = TimeSpan.FromHours(1)
                    }
                },
                Teams = new[] { "team1", "team2" },
                PointsOfInterest = new[]
                {
                    autoPoi1, autoPoi2,
                    new PointOfInterest()
                    {
                        Id = "poi2",
                        BonusTime = TimeSpan.FromMinutes(1),
                        DeployCooldown = TimeSpan.FromHours(1),
                        Auto = false
                    }
                },
                Map = new MapState()
                {
                    Nodes = new[]
                    {
                        new NodeState()
                        {
                            Id = 1,
                            NodeStatus = NodeStatus.Captured,
                            Owner = "team1",
                            LinkedNodes = new [] { 2}
                        },
                        new NodeState()
                        {
                            Id = 2,
                            NodeStatus = NodeStatus.Neutral,
                            LinkedNodes = new [] {1, 4}
                        },
                        new NodeState()
                        {
                            Id = 4,
                            NodeStatus = NodeStatus.Captured,
                            Owner = "team2",
                            LinkedNodes = new [] { 2}
                        },

                    }
                }
            };
            var picker = new DefaultPointOfInterestNodePicker();
            var poiDb = new Mock<IPointsOfInterestsDb>();
            var conflictSchedDb = new Mock<IConflictsScheduleDb>();
            ContainerHolder.Container.RegisterCustom(() => poiDb.Object);
            ContainerHolder.Container.RegisterCustom(() => conflictSchedDb.Object);
            ContainerHolder.Container.RegisterCustom<IPointOfInterestNodePicker>(() => picker);
            var globalConflictApi = new GlobalConflictServer();
            ContainerHolder.Container.RegisterCustom<GlobalConflictApi>(() => globalConflictApi);
            ContainerHolder.Container.RegisterCustom<GlobalConflictPrivateApi>(() => globalConflictApi);
            var deployer = new BattlePointOfInterestsDeployer();
            var deployedPoints = new DeployedPointsOfInterest();

            conflictSchedDb.Setup(_ => _.GetByDateAsync(It.IsAny<DateTime>())).Returns(Task.FromResult(globalConflict));

            deployedPoints = new DeployedPointsOfInterest();
            deployedPoints.AddDeployed(deployed1, true);

            Assert.AreEqual(1, deployedPoints.Count);
            await deployer.DeployForTeams(globalConflict, autoPoi1, globalConflict.Teams.ToList(), deployedPoints);
            Assert.AreEqual(2, deployedPoints.Count);
            poiDb.Verify(_ => _.InsertAsync("conflict1", null, "team2", 2, It.Is((PointOfInterest poi) => poi.Id == "auto_poi1"), It.IsAny<DateTime>()));

            await deployer.DeployForTeams(globalConflict, autoPoi2, globalConflict.Teams.ToList(), deployedPoints);
            Assert.AreEqual(2, deployedPoints.Count);
        }
    }
}
