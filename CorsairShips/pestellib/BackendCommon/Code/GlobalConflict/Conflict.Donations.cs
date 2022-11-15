using System.Linq;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict
{
    partial class Conflict
    {
        public int GetDonationLevel(int donationPoints, bool team)
        {
            var donationLevels = State.DonationBonusLevels.OrderBy(_ => _.Level).ToArray();
            if (donationLevels.Length == 0)
                return 0;
            var maxLevel = donationLevels.Last();
            var level = donationLevels.LastOrDefault(_ => donationPoints >= _.Threshold && _.Team == team);
            if (level == null)
                return donationPoints > maxLevel.Threshold ? maxLevel.Level : 0;

            return level.Level;
        }

        public bool IsDonationLevelUp(int donationPoints, int addedDonationPoints)
        {
            var lvlBefore = GetDonationLevel(donationPoints, false);
            var lvlAfter = GetDonationLevel(donationPoints + addedDonationPoints, false);
            return lvlAfter > lvlBefore;
        }

        public bool IsDonationTeamLevelUp(string team, int addedPoints)
        {
            var t = GetTeamState(team);
            var lvlBefore = GetDonationLevel(t.DonationPoints, true);
            var lvlAfter = GetDonationLevel(t.DonationPoints + addedPoints, true);
            return lvlAfter > lvlBefore;
        }

        public DonationBonus[] GetDonationBonuses(int level, bool team)
        {
            return State.DonationBonuses.Where(_ => _.Level == level && _.Team == team).ToArray();
        }
    }
}