using System;
using System.Linq;
using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict
{
    public class DeployedPointOfInterest
    {
        public string Id;
        public string ConflictId;
        public string TeamId;
        public string PlayerId;
        public DateTime UpdateTime;
        public PointOfInterest Data;
    }

    public interface IPointOfInterestPrivate
    {
        Task DeployPointOfInterestAsync(string conflictId, string playerId, string team, int nodeId, string poiId);
        Task<PointOfInterest> GetPointOfInterestAsync(string conflictId, string poiId, string team);
        Task<PointOfInterest> GetPointOfInterestAsync(string conflictId, string teamId, int nodeId);
        Task<PointOfInterest[]> GetTeamPointsOfInterestAsync(string conflictId, string teamId);
    }

    static class PointOfInterestExtensions
    {
        public static decimal GetBonus(this PointOfInterest point, PointOfInterestServerLogic type)
        {
            return point.Bonuses.Where(_ => _.ServerType == type).Sum(_ => _.Value);
        }
    }
}
