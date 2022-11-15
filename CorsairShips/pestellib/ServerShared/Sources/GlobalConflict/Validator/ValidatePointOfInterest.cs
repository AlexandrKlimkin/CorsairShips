using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerShared.GlobalConflict
{
    public class ValidatePointOfInterest : IGlobalConflictValidator
    {
        public bool IsValid(GlobalConflictState state, ValidatorMessageCollection messages)
        {
            foreach (var p in state.PointsOfInterest.Where(_ => _.Bonuses.Length == 0))
            {
                messages.Add(MessageLevel.Error, p.Id + " has no bonuses defined");
            }

            foreach (var p in state.PointsOfInterest.Where(_ => _.DeployCooldown <= TimeSpan.Zero))
            {
                messages.Add(MessageLevel.Error, p.Id + " deploy time can't be 0");
            }

            foreach (var p in state.PointsOfInterest.Where(_ => _.BonusTime <= TimeSpan.Zero))
            {
                messages.Add(MessageLevel.Error, p.Id + " deploy time can't be 0");
            }

            var generals = state.PointsOfInterest.Select(_ => _.GeneralLevel).Distinct();
            foreach (var g in Enumerable.Range(1, state.GeneralsCount).Where(_ => !generals.Contains(_)))
            {
                messages.Add(MessageLevel.Error, string.Format("PointOfInterest for general lvl {0} not defined. Remove generals or add PointOfInterest for each general level", g));
            }

            return messages.Errors == 0;
        }
    }
}
