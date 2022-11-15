using System;
using System.Collections.Generic;

namespace ServerShared.GlobalConflict
{
    public class GlobalConflictValidatorDummy : IGlobalConflictValidator
    {
        private readonly List<IGlobalConflictValidator> _validator = new List<IGlobalConflictValidator>();
        public GlobalConflictValidatorDummy()
        {
            _validator.AddRange(
                new IGlobalConflictValidator[]
                {
                    new ValidateBasics(), 
                    new ValidateBaseNodeForTeam(),
                    new ValidateTeams(),
                    new ValidateDonationBonusLevels(), 
                    new ValidateStages(), 
                    new ValidatePointOfInterest(),
                    new ValidateQuests(),
                }
            );
        }

        public bool IsValid(GlobalConflictState state, ValidatorMessageCollection messages)
        {
            var result = true;
            foreach (var v in _validator)
            {
                try
                {
                    result &= v.IsValid(state, messages);
                }
                catch (Exception e)
                {
                    result = false;
                    messages.Add(MessageLevel.Error, v.GetType().Name + " validator failed with error " + e + "\n");
                }
            }

            return result;
        }
    }
}
