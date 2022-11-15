using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict
{
    public interface ILeaderboardsPrivate
    {
        Task<long> GetDonationTopMyPositionAsync(string userId, bool myTeamOnly, string conflictId);
        Task<PlayerState[]> GetDonationTopAsync(string conflictId, string teamId, int page, int pageSize);

        Task<long> GetWinPointsTopMyPositionAsync(string userId, bool myTeamOnly, string conflictId);
        Task<PlayerState[]> GetWinPointsTopAsync(string conflictId, string teamId, int page, int pageSize);
    }
}
