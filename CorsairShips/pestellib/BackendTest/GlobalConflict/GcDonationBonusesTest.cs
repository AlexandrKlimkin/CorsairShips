using BackendCommon.Code.GlobalConflict;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerShared.GlobalConflict;

namespace BackendTest.GlobalConflict
{
    [TestClass]
    public class GcDonationBonusesTest
    {
        [TestMethod]
        public void TestPlayerWinPointsBonus()
        {
            var playerState = new PlayerState()
            {
                DonationBonuses = new []
                {
                    new DonationBonus()
                    {
                        ClientType = "TeamPointsBuff",
                        Value = 1
                    },
                    new DonationBonus()
                    {
                        ClientType = "TeamPointsBuff",
                        ServerType = DonationBonusType.TeamPointsBuff,
                        Value = 1
                    },
                    new DonationBonus()
                    {
                        Value = 1
                    },
                    new DonationBonus()
                    {
                        ServerType = DonationBonusType.TeamPointsBuff,
                        Value = 1
                    },
                }
            };
            var player = new Player(playerState);

            var bonus = player.GetTeamPointsBonus(true);
            Assert.AreEqual((float)2m, bonus);
        }

        [TestMethod]
        public void TestTeamWinPointsBonus()
        {
            var teamState = new TeamState()
            {
                DonationBonuses  = new []
                {
                    new DonationBonus()
                    {
                        ClientType = "TeamPointsBuff",
                        Value = 1
                    },
                    new DonationBonus()
                    {
                        ClientType = "TeamPointsBuff",
                        ServerType = DonationBonusType.TeamPointsBuff,
                        Value = 1
                    },
                    new DonationBonus()
                    {
                        ServerType = DonationBonusType.TeamPointsBuff,
                        Value = 1
                    }
                }
            };
            var team = new Team(teamState);

            var bonus = team.GetTeamPointsBonus(true);
            Assert.AreEqual((float)2m, bonus);
        }
    }
}
