using System;
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

namespace BackendTest
{
    [TestClass]
    public class GcDonationStageTest
    {
        private GlobalConflictServer _globalConflict;
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
        public async Task TestSimpleDonation()
        {
            var dummyDonations = new []
            {
                new Donation
                {
                    Id = Guid.NewGuid(),
                    UserId = "user1",
                    Amount = 100,
                },
                new Donation
                {
                    Id = Guid.NewGuid(),
                    UserId = "user1",
                    Amount = 100,
                }
            };
            var player = new PlayerState()
            {
                Id = "user1",
                ConflictId = "conflict1",
                TeamId = "team1"
            };
            var playerExpected = new PlayerState()
            {
                Id = player.Id,
                ConflictId = player.ConflictId,
                TeamId = player.TeamId,
            };
            var teamState = new TeamState()
            {
                Id = "team1"
            };
            var teamStateExpected = new TeamState()
            {
                Id = teamState.Id,
                DonationPoints = 200
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
                Teams = new[] {"team1"},
                TeamsStates = new[] {teamState}
            };

            var donationDb = new Mock<IDonationsDb>();
            donationDb.Setup(_ => _.GetUnprocessedAsync(It.IsAny<int>())).Returns(() => Task.FromResult(dummyDonations));
            var scheduleDb = new Mock<IConflictsScheduleDb>();
            scheduleDb.Setup(_ => _.GetByDateAsync(It.IsAny<DateTime>())).Returns(() => Task.FromResult(globalConflict));
            var playersDb = new Mock<IPlayersDb>();
            playersDb.Setup(_ => _.GetPlayerAsync("user1", "conflict1")).Returns(Task.FromResult(player));
            ContainerHolder.Container.RegisterCustom(() => donationDb.Object);
            ContainerHolder.Container.RegisterCustom(() => scheduleDb.Object);
            ContainerHolder.Container.RegisterCustom(() => playersDb.Object);
            _globalConflict = new GlobalConflictServer();
            ContainerHolder.Container.RegisterCustom<GlobalConflictApi>(() => _globalConflict);
            ContainerHolder.Container.RegisterCustom<GlobalConflictPrivateApi>(() => _globalConflict);

            var stage = new DonationStage();

            await stage.Update();

            DeepEqualityUsingJsonSerialization.ShouldDeepEqual(playerExpected, player);
            DeepEqualityUsingJsonSerialization.ShouldDeepEqual(teamState, teamStateExpected);

            donationDb.Verify(_ => _.GetUnprocessedAsync(It.IsAny<int>()));
            donationDb.Verify(_ => _.MarkProcessedAsync(It.Is((Donation[] donations) => donations.Length == 2 && donations.All(d => dummyDonations.Any(dum => dum.Id == d.Id)))));
            donationDb.VerifyNoOtherCalls();

            scheduleDb.Verify(_ => _.GetByDateAsync(It.IsAny<DateTime>()));
            scheduleDb.Verify(_ => _.SaveAsync(It.Is((GlobalConflictState state) => state.Id == globalConflict.Id)));
            scheduleDb.VerifyNoOtherCalls();

            playersDb.Verify(_ => _.GetPlayerAsync("user1", "conflict1"));
            playersDb.Verify(_ => _.IncrementPlayerDonationAsync("conflict1", "user1", 100), Times.Exactly(2));
            playersDb.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task TestLevelUpDonation()
        {
            var dummyDonations = new[]
            {
                new Donation
                {
                    Id = Guid.NewGuid(),
                    UserId = "user1",
                    Amount = 100,
                },
                new Donation
                {
                    Id = Guid.NewGuid(),
                    UserId = "user1",
                    Amount = 100,
                }
            };
            var player = new PlayerState()
            {
                Id = "user1",
                ConflictId = "conflict1",
                TeamId = "team1"
            };
            var playerExpected = new PlayerState()
            {
                Id = player.Id,
                ConflictId = player.ConflictId,
                TeamId = player.TeamId,
                DonationPoints = 200
            };
            var playerExpected2 = new PlayerState()
            {
                Id = player.Id,
                ConflictId = player.ConflictId,
                TeamId = player.TeamId,
                DonationPoints = 400
            };
            var teamState = new TeamState()
            {
                Id = "team1"
            };
            var teamStateExpected = new TeamState()
            {
                Id = teamState.Id,
                DonationPoints = 200
            };
            var teamDonationBonus = new DonationBonus()
            {
                Level = 1,
                ClientType = "team_bonus",
                Team = true,
            };
            var teamStateExpected2 = new TeamState()
            {
                Id = teamState.Id,
                DonationPoints = 400,
                DonationBonuses = new[] {teamDonationBonus}
            };
            var globalConflict = new GlobalConflictState()
            {
                Id = "conflict1",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                DonationBonusLevels = new []
                {
                    new DonationBonusLevels()
                    {
                        Level = 1,
                        Threshold = 99,
                    },
                    new DonationBonusLevels()
                    {
                        Level = 1,
                        Threshold = 299,
                        Team = true
                    }
                },
                DonationBonuses = new []
                {
                    new DonationBonus()
                    {
                        Level = 1,
                        ClientType = "player_bonus",
                        Team = false,
                    },
                    teamDonationBonus
                },
                Stages = new StageDef[]
                {
                    new StageDef()
                    {
                        Id = StageType.Donation,
                        Period = TimeSpan.FromHours(1)
                    }
                },
                Teams = new[] { "team1" },
                TeamsStates = new[] { teamState }
            };

            var donationDb = new Mock<IDonationsDb>();
            donationDb.Setup(_ => _.GetUnprocessedAsync(It.IsAny<int>())).Returns(() => Task.FromResult(dummyDonations));
            var scheduleDb = new Mock<IConflictsScheduleDb>();
            scheduleDb.Setup(_ => _.GetByDateAsync(It.IsAny<DateTime>())).Returns(() => Task.FromResult(globalConflict));
            var playersDb = new Mock<IPlayersDb>();
            playersDb.Setup(_ => _.GetPlayerAsync("user1", "conflict1")).Returns(Task.FromResult(player));
            playersDb.Setup(_ => _.IncrementPlayerDonationAsync("conflict1", "user1", 100)).Returns(() => {
                player.DonationPoints += 100;
                return Task.CompletedTask;
            });
            ContainerHolder.Container.RegisterCustom(() => donationDb.Object);
            ContainerHolder.Container.RegisterCustom(() => scheduleDb.Object);
            ContainerHolder.Container.RegisterCustom(() => playersDb.Object);
            _globalConflict = new GlobalConflictServer();
            ContainerHolder.Container.RegisterCustom<GlobalConflictApi>(() => _globalConflict);
            ContainerHolder.Container.RegisterCustom<GlobalConflictPrivateApi>(() => _globalConflict);

            var stage = new DonationStage();

            // должен выдаться бонус игроку, но не команде
            await stage.Update();

            DeepEqualityUsingJsonSerialization.ShouldDeepEqual(playerExpected, player);
            DeepEqualityUsingJsonSerialization.ShouldDeepEqual(teamState, teamStateExpected);

            donationDb.Verify(_ => _.GetUnprocessedAsync(It.IsAny<int>()), Times.Exactly(1));
            donationDb.Verify(_ => _.MarkProcessedAsync(It.Is((Donation[] donations) => donations.Length == 2 && donations.All(d => dummyDonations.Any(dum => dum.Id == d.Id)))), Times.Exactly(1));
            donationDb.VerifyNoOtherCalls();

            scheduleDb.Verify(_ => _.GetByDateAsync(It.IsAny<DateTime>()), Times.Exactly(1));
            scheduleDb.Verify(_ => _.SaveAsync(It.Is((GlobalConflictState state) => state.Id == globalConflict.Id)), Times.Exactly(1));
            scheduleDb.VerifyNoOtherCalls();

            playersDb.Verify(_ => _.GetPlayerAsync("user1", "conflict1"), Times.Exactly(1));
            playersDb.Verify(_ => _.IncrementPlayerDonationAsync("conflict1", "user1", 100), Times.Exactly(2));
            playersDb.Verify(_ => _.GiveBonusesToPlayerAsync("conflict1", "user1", It.Is((DonationBonus[] b) => b.Length == 1 && b.First().ClientType == "player_bonus")), Times.Exactly(1));
            playersDb.VerifyNoOtherCalls();

            // должен выдаться бонус команде, но не игроку
            await stage.Update();

            DeepEqualityUsingJsonSerialization.ShouldDeepEqual(playerExpected2, player);
            DeepEqualityUsingJsonSerialization.ShouldDeepEqual(teamState, teamStateExpected2);

            donationDb.Verify(_ => _.GetUnprocessedAsync(It.IsAny<int>()), Times.Exactly(2));
            donationDb.Verify(_ => _.MarkProcessedAsync(It.Is((Donation[] donations) => donations.Length == 2 && donations.All(d => dummyDonations.Any(dum => dum.Id == d.Id)))), Times.Exactly(2));
            donationDb.VerifyNoOtherCalls();

            scheduleDb.Verify(_ => _.GetByDateAsync(It.IsAny<DateTime>()), Times.Exactly(2));
            scheduleDb.Verify(_ => _.SaveAsync(It.Is((GlobalConflictState state) => state.Id == globalConflict.Id)), Times.Exactly(2));
            scheduleDb.VerifyNoOtherCalls();

            playersDb.Verify(_ => _.GetPlayerAsync("user1", "conflict1"), Times.Exactly(2));
            playersDb.Verify(_ => _.IncrementPlayerDonationAsync("conflict1", "user1", 100), Times.Exactly(4));
            playersDb.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task TestGenerals()
        {
            var players = new PlayerState[]
            {
                new PlayerState()
                {
                    Id = "user1a",
                    ConflictId = "conflict1",
                    TeamId = "team1",
                    DonationPoints = 500
                },
                new PlayerState()
                {
                    Id = "user2a",
                    ConflictId = "conflict1",
                    TeamId = "team1",
                    DonationPoints = 100
                },
                new PlayerState()
                {
                    Id = "user3a",
                    ConflictId = "conflict1",
                    TeamId = "team1",
                    DonationPoints = 50
                },
                new PlayerState()
                {
                    Id = "user1b",
                    ConflictId = "conflict1",
                    TeamId = "team2",
                    DonationPoints = 1000
                },
                new PlayerState()
                {
                    Id = "user2b",
                    ConflictId = "conflict1",
                    TeamId = "team2",
                    DonationPoints = 500,
                },
                new PlayerState()
                {
                    Id = "user3b",
                    ConflictId = "conflict1",
                    TeamId = "team2",
                    DonationPoints = 300
                }
            };
            var globalConflict = new GlobalConflictState()
            {
                Id = "conflict1",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                DonationBonusLevels = new DonationBonusLevels[] {},
                DonationBonuses = new DonationBonus[] {},
                Stages = new []
                {
                    new StageDef()
                    {
                        Id = StageType.Donation,
                        Period = TimeSpan.FromMinutes(30)
                    }, 
                    new StageDef()
                    {
                        Id = StageType.Cooldown,
                        Period = TimeSpan.FromMinutes(30)
                    }
                },
                Teams = new[] { "team1", "team2" },
                TeamsStates = new[] {
                    new TeamState()
                    {
                        Id = "team1"
                    },
                    new TeamState()
                    {
                        Id = "team2"
                    }
                },
                GeneralsCount = 2
            };
            var donationDb = new Mock<IDonationsDb>();
            donationDb.Setup(_ => _.GetUnprocessedAsync(It.IsAny<int>())).Returns(() => Task.FromResult(new Donation[] {}));
            var scheduleDb = new Mock<IConflictsScheduleDb>();
            scheduleDb.Setup(_ => _.GetByDateAsync(It.IsAny<DateTime>())).Returns(() => Task.FromResult(globalConflict));
            var playersDb = new Mock<IPlayersDb>();
            var leadersDb = new Mock<ILeaderboardsDb>();
            leadersDb.Setup(_ => _.GetDonationTopAsync("conflict1", "team1", 0, 2)).Returns(() => Task.FromResult(
                players.Where(_ => _.TeamId == "team1").Take(2).ToArray()
            ));
            leadersDb.Setup(_ => _.GetDonationTopAsync("conflict1", "team2", 0, 2)).Returns(() => Task.FromResult(
                players.Where(_ => _.TeamId == "team2").Take(2).ToArray()
            ));
            ContainerHolder.Container.RegisterCustom(() => donationDb.Object);
            ContainerHolder.Container.RegisterCustom(() => scheduleDb.Object);
            ContainerHolder.Container.RegisterCustom(() => playersDb.Object);
            ContainerHolder.Container.RegisterCustom(() => leadersDb.Object);
            _globalConflict = new GlobalConflictServer();
            ContainerHolder.Container.RegisterCustom<GlobalConflictApi>(() => _globalConflict);
            ContainerHolder.Container.RegisterCustom<GlobalConflictPrivateApi>(() => _globalConflict);

            var stage = new DonationStage();

            // генералы не назначаются во время этапа доната
            await stage.Update();

            donationDb.Verify(_ => _.GetUnprocessedAsync(It.IsAny<int>()), Times.Exactly(1));
            donationDb.VerifyNoOtherCalls();

            scheduleDb.Verify(_ => _.GetByDateAsync(It.IsAny<DateTime>()), Times.Exactly(1));
            scheduleDb.VerifyNoOtherCalls();
            playersDb.VerifyNoOtherCalls();
            leadersDb.VerifyNoOtherCalls();

            globalConflict.StartTime -= TimeSpan.FromMinutes(30);
            globalConflict.EndTime -= TimeSpan.FromMinutes(30);

            // генералы назначились
            await stage.Update();

            playersDb.Verify(_ => _.SaveAsync(It.IsAny<PlayerState>()), Times.Exactly(4));
            leadersDb.Verify(_ => _.GetDonationTopAsync("conflict1", "team1", 0, 2), Times.Exactly(1));
            leadersDb.Verify(_ => _.GetDonationTopAsync("conflict1", "team2", 0, 2), Times.Exactly(1));

            Assert.AreEqual(players[0].GeneralLevel, 2);
            Assert.AreEqual(players[1].GeneralLevel, 1);
            Assert.AreEqual(players[2].GeneralLevel, 0);

            Assert.AreEqual(players[3].GeneralLevel, 2);
            Assert.AreEqual(players[4].GeneralLevel, 1);
            Assert.AreEqual(players[5].GeneralLevel, 0);
        }
    }
}
