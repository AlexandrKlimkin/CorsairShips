using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerShared.GlobalConflict;

namespace ServerShared.Sources.GlobalConflict
{
    public static class GlobalConflictExtensions
    {
        public static NodeState GetNode(this GlobalConflictState conflict, int id)
        {
            return conflict.Map.Nodes.First(_ => _.Id == id);
        }

        public static IEnumerable<NodeState> GetReachableNodes(this GlobalConflictState conflict, string team, bool includeOwned = false)
        {
            var nodes = conflict.Map.Nodes
                .Where(_ => _.Owner == team || _.NodeStatus == NodeStatus.Base && _.BaseForTeam == team)
                .SelectMany(_ => _.LinkedNodes.Select(conflict.GetNode))
                .Where(_ => (includeOwned || _.Owner != team) && _.NodeStatus != NodeStatus.Base);

            return nodes;
        }

        public static bool OwnedByTeam(this NodeState node, string team)
        {
            return node.NodeStatus == NodeStatus.Base && node.BaseForTeam == team || node.Owner == team;
        }

        public static bool IsGeneralPoi(this PointOfInterest poi)
        {
            return poi.GeneralLevel > 0;
        }

        public static bool IsTeamValid(this PointOfInterest poi, PlayerState player)
        {
            return string.IsNullOrEmpty(poi.OwnerTeam) || player != null && poi.OwnerTeam == player.TeamId;
        }

        public static bool IsGeneralValid(this PointOfInterest poi, PlayerState player)
        {
            return poi.GeneralLevel < 1 || player != null && player.GeneralLevel == poi.GeneralLevel;
        }

        public static bool IsDeployed(this PointOfInterest poi, DateTime? dt = null, int onNode = 0)
        {
            if (!dt.HasValue)
                dt = DateTime.UtcNow;
            return poi.NodeId > 0 && poi.NextDeploy > DateTime.MinValue && poi.NextDeploy > dt && (onNode == 0 || onNode == poi.NodeId);
        }

        public static bool IsDeployable(this PointOfInterest poi, PlayerState player = null, DateTime? dt = null)
        {
            if (poi.Auto)
                return false;
            if (!IsGeneralValid(poi, player))
                return false;
            if (!IsTeamValid(poi, player))
                return false;
            if (!dt.HasValue)
                dt = DateTime.UtcNow;

            return poi.NextDeploy <= dt;
        }
    }
}
