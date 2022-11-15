using System.Linq;
using ServerShared.GlobalConflict;
using ServerShared.Sources.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict
{
    partial class Conflict
    {
        public NodeState GetNode(int id)
        {
            return State.GetNode(id);
        }

        public float SupportBonus(int nodeId, string team)
        {
            var node = GetNode(nodeId);
            return node.LinkedNodes.Sum(_ =>
            {
                var n = GetNode(_);
                if (n.Owner != team)
                    return 0f;
                return n.SupportBonus;
            });
        }

        public bool IsNodeReachable(int nodeId, string team)
        {
            var node = GetNode(nodeId);
            if (node.Owner == team)
                return true;
            for (var i = 0; i < node.LinkedNodes.Length; ++i)
            {
                var snId = node.LinkedNodes[i];
                var sn = GetNode(snId);
                if (sn.OwnedByTeam(team))
                    return true;
            }

            return false;
        }
    }
}