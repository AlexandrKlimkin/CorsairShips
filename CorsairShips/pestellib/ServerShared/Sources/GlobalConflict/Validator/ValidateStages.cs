using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerShared.GlobalConflict
{
    public class ValidateStages : IGlobalConflictValidator
    {
        public bool IsValid(GlobalConflictState state, ValidatorMessageCollection messages)
        {
            if (state.Stages.Length == 0)
            {
                messages.Add(MessageLevel.Error, "Conflict must contain 'battle' and 'final' stages.");
                return false;
            }

            var donationStage = state.Stages.First(_ => _.Id == StageType.Donation);
            var battleStage = state.Stages.First(_ => _.Id == StageType.Battle);
            var finalStage = state.Stages.First(_ => _.Id == StageType.Final);

            if (donationStage.Period <= TimeSpan.Zero)
            {
                messages.Add(MessageLevel.Error, "Donation stage period is invalid.");
            }

            if (battleStage.Period <= TimeSpan.Zero)
            {
                messages.Add(MessageLevel.Error, "Battle stage period is invalid.");
            }

            if (finalStage.Period <= TimeSpan.Zero)
            {
                messages.Add(MessageLevel.Error, "Final stage period is invalid.");
            }

            return messages.Errors == 0;
        }
    }
}
