using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.ApiProfiler
{
    class PointOfInterestProfiler : IPointOfInterestPrivate
    {
        private readonly IPointOfInterestPrivate _original;

        public PointOfInterestProfiler(IPointOfInterestPrivate original)
        {
            _original = original;
        }

        public async Task DeployPointOfInterestAsync(string conflictId, string playerId, string team, int nodeId, string poiId)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IPointOfInterestPrivate_DeployPointOfInterestAsync"))
            {
                await _original.DeployPointOfInterestAsync(conflictId, playerId, team, nodeId, poiId).ConfigureAwait(false);
            }
        }

        public async Task<PointOfInterest> GetPointOfInterestAsync(string conflictId, string poiId, string team)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IPointOfInterestPrivate_GetPointOfInterestAsync"))
            {
                return await _original.GetPointOfInterestAsync(conflictId, poiId, team).ConfigureAwait(false);
            }
        }

        public async Task<PointOfInterest> GetPointOfInterestAsync(string conflictId, string teamId, int nodeId)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IPointOfInterestPrivate_GetPointOfInterestAsync"))
            {
                return await _original.GetPointOfInterestAsync(conflictId, teamId, nodeId).ConfigureAwait(false);
            }
        }

        public async Task<PointOfInterest[]> GetTeamPointsOfInterestAsync(string conflictId, string teamId)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IPointOfInterestPrivate_GetTeamPointsOfInterestAsync"))
            {
                return await _original.GetTeamPointsOfInterestAsync(conflictId, teamId).ConfigureAwait(false);
            }
        }
    }
}