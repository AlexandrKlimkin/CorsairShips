using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.ApiProfiler
{
    class LeaderbordsProfiler : ILeaderboardsPrivate
    {
        private readonly ILeaderboardsPrivate _original;

        public LeaderbordsProfiler(ILeaderboardsPrivate original)
        {
            _original = original;
        }

        public async Task<long> GetDonationTopMyPositionAsync(string userId, bool myTeamOnly, string conflictId)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "ILeaderboardsPrivate_GetDonationTopMyPositionAsync"))
            {
                return await _original.GetDonationTopMyPositionAsync(userId, myTeamOnly, conflictId).ConfigureAwait(false);
            }
        }

        public async Task<PlayerState[]> GetDonationTopAsync(string conflictId, string team, int page, int pageSize)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "ILeaderboardsPrivate_GetDonationTopAsync"))
            {
                return await _original.GetDonationTopAsync(conflictId, team, page, pageSize).ConfigureAwait(false);
            }
        }

        public async Task<long> GetWinPointsTopMyPositionAsync(string userId, bool myTeamOnly, string conflictId)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "ILeaderboardsPrivate_GetWinPointsTopMyPositionAsync"))
            {
                return await _original.GetWinPointsTopMyPositionAsync(userId, myTeamOnly, conflictId).ConfigureAwait(false);
            }
        }

        public async Task<PlayerState[]> GetWinPointsTopAsync(string conflictId, string teamId, int page, int pageSize)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "ILeaderboardsPrivate_GetWinPointsTopAsync"))
            {
                return await _original.GetWinPointsTopAsync(conflictId, teamId, page, pageSize).ConfigureAwait(false);
            }
        }
    }
}