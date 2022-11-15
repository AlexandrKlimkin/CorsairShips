using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict
{
    public partial class Node
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Node));
        private readonly NodeState _nodeState;

        public Node(NodeState nodeState)
        {
            _nodeState = nodeState;
            if (_nodeState.TeamPoints == null)
            {
                _nodeState.TeamPoints = new Dictionary<string, int>();
            }
        }

        public void AddTeamPoints(string team, int points)
        {
            if(_nodeState.NodeStatus == NodeStatus.Base)
                throw new Exception("Can't earn TeamPoint as NodeStatus.Base");
            if (points > 0)
            {
                var opponents = Enumerable.Where<KeyValuePair<string, int>>(_nodeState.TeamPoints, _ => _.Key != team).ToArray();
                var totalOpponents = opponents.Sum(_ => _.Value);
                var share = opponents.Where(_ => _.Value > 0)
                    .Select(_ => new {id = _.Key, share = (float) _.Value / totalOpponents})
                    .ToDictionary(_ => _.id, _ => _.share);
                var total = Enumerable.Sum<KeyValuePair<string, int>>(_nodeState.TeamPoints, _ => _.Value);
                total += points;
                var delta = total - _nodeState.CaptureLimit;
                if (delta > 0 && share.Count > 0)
                {
                    var totalSub = 0;
                    var overflow = false;
                    foreach (var f in share)
                    {
                        var sub = (int) (f.Value * delta);
                        totalSub += sub;
                        var c = _nodeState.TeamPoints[f.Key];
                        c -= sub;
                        overflow |= c < 0;
                        _nodeState.TeamPoints[f.Key] = Math.Max((int) 0, (int) c);
                    }

                    if (!overflow && totalSub < delta)
                        _nodeState.TeamPoints[share.First().Key] -= 1;
                }
            }

            if (!_nodeState.TeamPoints.TryGetValue(team, out var curr))
                curr = 0;

            _nodeState.TeamPoints[team] = Math.Min(curr + points, (int) _nodeState.CaptureLimit);
        }
    }
}