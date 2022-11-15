using System.Collections.Generic;
using System.Linq;

namespace ServerShared.GlobalConflict
{
    public class ValidateTeams : IGlobalConflictValidator
    {
        public bool IsValid(GlobalConflictState state, ValidatorMessageCollection messages)
        {
            if (state.Teams.Length == 0)
            {
                messages.Add(MessageLevel.Error, "Conflict must have at least one team");
                return false;
            }

            var err = state.Map.Nodes.Where(_ => !string.IsNullOrEmpty(_.BaseForTeam) && !state.Teams.Contains(_.BaseForTeam));
            foreach (var node in err)
            {
                messages.Add(MessageLevel.Error, string.Format("Node#{0}.BaseForTeam has invalid value '{1}'.", node.Id, node.BaseForTeam));
            }

            return messages.Errors == 0;
        }
    }
}