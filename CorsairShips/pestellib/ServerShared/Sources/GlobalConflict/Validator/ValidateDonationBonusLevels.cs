using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerShared.GlobalConflict;

namespace ServerShared.GlobalConflict
{
    public class ValidateDonationBonusLevels : IGlobalConflictValidator
    {
        public bool IsValid(GlobalConflictState state, ValidatorMessageCollection messages)
        {
            if (state.Stages.All(_ => _.Id != StageType.Donation))
            {
                if (state.DonationBonusLevels.Length > 0)
                    messages.Add(MessageLevel.Error, "Conflict dont has 'donation' stage but defines donation bonuses. Remove donations from conflict or switch on donation stage");
                if (state.DonationBonuses.Length > 0)
                    messages.Add(MessageLevel.Error, "Conflict dont has 'donation' stage but defines donation levels. Remove donations from conflict or switch on donation stage");
                return messages.Errors == 0;
            }

            if(state.DonationBonusLevels.Length == 0)
                messages.Add(MessageLevel.Error, "No donation bonus levels defined. Switch off donation stage or define levels.");
            if(state.DonationBonuses.Length == 0)
                messages.Add(MessageLevel.Error, "No donation bonuses defined. Switch off donation stage or define dontion bonuses.");

            var teamLevels = state.DonationBonusLevels.Where(_ => _.Team).Select(_ => _.Level).ToArray();
            var playerLevels = state.DonationBonusLevels.Where(_ => !_.Team).Select(_ => _.Level).ToArray();

            var teamBonusLevels = state.DonationBonuses.Where(_ => _.Team).Select(_ => _.Level).Distinct().ToArray();
            var playerBonusLevels = state.DonationBonuses.Where(_ => !_.Team).Select(_ => _.Level).Distinct().ToArray();

            foreach (var level in teamLevels.Where(_ => !teamBonusLevels.Contains(_)))
            {
                messages.Add(MessageLevel.Error, string.Format("Team donation level {0} dont has bonuses.", level));
            }

            foreach (var level in teamBonusLevels.Where(_ => !teamLevels.Contains(_)))
            {
                messages.Add(MessageLevel.Error, string.Format("Team donation bonus for level {0} defined by has no level definition.", level));
            }

            foreach (var level in playerLevels.Where(_ => !playerBonusLevels.Contains(_)))
            {
                messages.Add(MessageLevel.Error, string.Format("Personal donation level {0} dont has bonuses.", level));
            }

            foreach (var level in playerBonusLevels.Where(_ => !playerLevels.Contains(_)))
            {
                messages.Add(MessageLevel.Error, string.Format("Personal donation bonus for level {0} defined by has no level definition.", level));
            }


            if (state.DonationBonusLevels.Any(_ => _.Level == 0))
            {
                messages.Add(MessageLevel.Error, "Donation level can't have zero level. Start assign levels from 1.");
            }

            if (state.DonationBonuses.Any(_ => _.Level == 0))
            {
                messages.Add(MessageLevel.Error, "Donation bonus can't have zero level. Start assign levels from 1.");
            }

            return messages.Errors == 0;
        }
    }
}
