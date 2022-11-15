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
using S;
using ServerLib;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendTest.GlobalConflict
{
    [TestClass]
    public class GcFinalStageTest
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
        public async Task TestFinalStage()
        {
            var resultsDb = new Mock<IConflictResultsDb>();
            var scheduleDb = new Mock<IConflictsScheduleDb>();
            ContainerHolder.Container.RegisterCustom(() => resultsDb.Object);
            ContainerHolder.Container.RegisterCustom(() => scheduleDb.Object);
            var globalConflict = new GlobalConflictServer();
            ContainerHolder.Container.RegisterCustom<GlobalConflictApi>(() => globalConflict);
            ContainerHolder.Container.RegisterCustom<GlobalConflictPrivateApi>(() => globalConflict);

            var globalConflictState = new GlobalConflictState()
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
                        Id = StageType.Final,
                        Period = TimeSpan.FromHours(1)
                    }
                },
                Map = new MapState()
                {
                    Nodes = new []
                    {
                        new NodeState()
                        {
                            Id = 1,
                            Owner = "team1",
                            NodeStatus = NodeStatus.Base
                        },
                        new NodeState()
                        {
                            Id = 2,
                            Owner = "team2",
                            NodeStatus = NodeStatus.Base
                        },
                        new NodeState()
                        {
                            Id = 3,
                            Owner = "team1",
                            NodeStatus = NodeStatus.Captured,
                            ResultPoints = 1
                        },
                        new NodeState()
                        {
                            Id = 4,
                            Owner = "team1",
                            NodeStatus = NodeStatus.Captured,
                            ResultPoints = 1
                        },
                        new NodeState()
                        {
                            Id = 5,
                            Owner = "team2",
                            NodeStatus = NodeStatus.Captured,
                            ResultPoints = 1
                        },
                        new NodeState()
                        {
                            Id = 6,
                            NodeStatus = NodeStatus.Neutral,
                            ResultPoints = 1
                        },
                    }
                },
                Teams = new[] { "team1", "team2" },
            };
            scheduleDb.Setup(_ => _.GetByDateAsync(It.IsAny<DateTime>())).Returns(() => Task.FromResult(globalConflictState));

            var finalStage = new FinalStage();
            await finalStage.Update();

            var resultExpected = new ConflictResult()
            {
                ConflictId = "conflict1",
                WinnerTeam = "team1",
                ResultPoints = new List<ConflictResultPoints>()
                {
                    new ConflictResultPoints() {Points = 2, Team = "team1"},
                    new ConflictResultPoints() {Points = 1, Team = "team2"},
                }
            };
            resultsDb.Verify(_ => _.InsertAsync(It.Is((ConflictResult cr) => DeepEqualityUsingJsonSerialization.IsDeepEqual(resultExpected, cr))));
        }
    }
}
