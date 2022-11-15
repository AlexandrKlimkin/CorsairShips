using System.Linq;
using PestelLib.Utils;
using ServerShared.GlobalConflict;
using ServerShared.Sources.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.Server.Stages
{
    public class DefaultPointOfInterestNodePicker : IPointOfInterestNodePicker
    {
        public NodeState PickNode(GlobalConflictState conflictState, string team, PointOfInterest point, DeployedPointsOfInterest occupiedPoints)
        {
            var conflict = new Conflict(conflictState);
            var winning = conflict.GetWinningTeam();
            var deployed = Enumerable.ToArray<int>(occupiedPoints.GetTeamPois(team).Select(t => t.NodeId));
            if (winning.Id == team)
                return PickRandomReachableNode(conflict, team, deployed);
            else
                return PickEasyReachableNode(conflict, team, deployed);
        }

        // дефолтная стратегия для проигрывающей команды, сосредотачиваем силы на точке с максимальным WinPoints
        public NodeState PickEasyReachableNode(Conflict conflict, string team, params int[] skip)
        {
            var notMy = conflict.State.GetReachableNodes(team)
                .Where(_ => !skip.Contains(_.Id))
                .OrderByDescending(_ => _.WinPoints)
                .FirstOrDefault();
            if (notMy != null)
                return notMy;

            return conflict.State.GetReachableNodes(team, true)
                .Where(_ => !skip.Contains(_.Id))
                .OrderByDescending(_ => _.WinPoints)
                .FirstOrDefault();
        }

        // дефолтная стратегия для выигрывающей команды, сосредотачиваем силы на любой точке
        public NodeState PickRandomReachableNode(Conflict conflict, string team, params int[] skip)
        {
            var notMy = conflict.State.GetReachableNodes(team)
                .Where(_ => !skip.Contains(_.Id))
                .ToArray()
                .Shuffle()
                .FirstOrDefault();
            if (notMy != null)
                return notMy;
            return conflict.State.GetReachableNodes(team, true)
                .Where(_ => !skip.Contains(_.Id))
                .ToArray()
                .Shuffle()
                .FirstOrDefault();
        }
    }
}