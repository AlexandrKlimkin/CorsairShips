using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerShared.GlobalConflict
{
    public class ValidateBasics : IGlobalConflictValidator
    {
        public bool IsValid(GlobalConflictState state, ValidatorMessageCollection messages)
        {
            if (string.IsNullOrEmpty(state.Id))
            {
                messages.Add(MessageLevel.Error, "Conflict ID not set");
            }

            if (state.PrizePlaces < 1)
            {
                messages.Add(MessageLevel.Error, "PrizePlaces must be greater than 0");
            }

            if (state.PrizesCount < 1)
            {
                messages.Add(MessageLevel.Error, "PrizesCount must be greater than 0");
            }

            if (state.MaxPointsOfInterestAtNode < 1)
            {
                messages.Add(MessageLevel.Error, "MaxPointsOfInterestAtNode must be greater than 0");
            }

            if (state.MaxSameTypePointsOfInterestAtNode < 1)
            {
                messages.Add(MessageLevel.Error, "MaxSameTypePointsOfInterestAtNode must be greater than 0");
            }

            if (string.IsNullOrEmpty(state.Map.TextureId))
            {
                messages.Add(MessageLevel.Error, "Map ID not set");
            }

            foreach (var nodeState in state.Map.Nodes.Where(_ => Math.Abs(_.PositionX) > 1 || Math.Abs(_.PositionY) > 1))
            {
                messages.Add(MessageLevel.Error, string.Format("{0} position ({1},{2}) is out of the map.", nodeState.Id, nodeState.PositionX, nodeState.PositionY));
            }

            foreach (var nodeState in state.Map.Nodes.Where(_ => _.WinPoints == 0 && _.NodeStatus != NodeStatus.Base))
            {
                messages.Add(MessageLevel.Error, string.Format("{0} WinPoints = 0", nodeState.Id));
            }

            return messages.Errors == 0;
        }
    }
}
