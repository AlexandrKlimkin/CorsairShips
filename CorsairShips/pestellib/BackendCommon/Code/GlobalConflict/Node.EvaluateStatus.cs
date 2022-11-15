using System.Collections.Generic;
using System.Linq;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict
{
    partial class Node
    {
        private int OwnerTeamPoints()
        {
            return Enumerable.First<KeyValuePair<string, int>>(_nodeState.TeamPoints, _ => _.Key == _nodeState.Owner).Value;
        }

        private bool EvaluateNodeCapturedStatus()
        {
            var ownerPoints = OwnerTeamPoints();
            var top =
                Enumerable.Where<KeyValuePair<string, int>>(_nodeState.TeamPoints, _ => _.Value > ownerPoints && _.Value - _nodeState.CaptureThreshold >= 0)
                    .OrderByDescending(_ => _.Value)
                    .ToArray();
            if (top.Length < 1)
                return false;
            var capturer = top.First();
            var oldOwner = _nodeState.Owner;
            TakeOwnership(capturer.Key);
            Log.Debug($"Node {_nodeState.Id} ownership change {oldOwner} ({ownerPoints} pts.)->{capturer.Key} ({capturer.Value} pts.)");
            return true;
        }

        private void TakeOwnership(string team)
        {
            _nodeState.NodeStatus = NodeStatus.Captured;
            _nodeState.Owner = team;
            AddTeamPoints(team, _nodeState.CaptureBonus);
        }

        private bool EvaluateNodeNeutralStatus()
        {
            var top = Enumerable.Where<KeyValuePair<string, int>>(_nodeState.TeamPoints, _ => _.Value >= _nodeState.NeutralThreshold)
                .OrderByDescending(_ => _.Value).ToArray();
            if (top.Length == 0)
                return false;

            var capturer = top.First();
            TakeOwnership(capturer.Key);
            Log.Debug($"Neutral node {_nodeState.Id} captured by {capturer.Key} ({capturer.Value} pts.)");
            return true;
        }

        public bool EvaluateNodeStatus()
        {
            if (_nodeState.NodeStatus == NodeStatus.Neutral)
            {
                return EvaluateNodeNeutralStatus();
            }

            if (_nodeState.NodeStatus == NodeStatus.Captured)
            {
                return EvaluateNodeCapturedStatus();
            }

            return false;
        }
    }
}