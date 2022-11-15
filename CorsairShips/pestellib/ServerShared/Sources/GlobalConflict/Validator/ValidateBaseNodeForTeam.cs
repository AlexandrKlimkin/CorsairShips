using System.Collections.Generic;
using System.Linq;

namespace ServerShared.GlobalConflict
{
    public class ValidateBaseNodeForTeam : IGlobalConflictValidator
    {
        public bool IsValid(GlobalConflictState state, ValidatorMessageCollection messages)
        {
            foreach (var nodeState in state.Map.Nodes.Where(_ => _.NodeStatus == NodeStatus.Base && string.IsNullOrEmpty(_.BaseForTeam)))
            {
                messages.Add(MessageLevel.Error, string.Format("Set BaseForTeam for Node#{0}.", nodeState.Id));
            }
            var teams = state.Teams.ToDictionary(_ => _,t => state.Map.Nodes.Count(_ => _.BaseForTeam == t));
            foreach (var kv in teams)
            {
                if (kv.Value == 0)
                {
                    messages.Add(MessageLevel.Error, kv.Key + " dont have base node (Node Status).");
                }
            }

            return messages.Errors == 0;
        }
    }
}
