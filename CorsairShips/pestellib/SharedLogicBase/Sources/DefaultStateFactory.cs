using System;
using S;

namespace PestelLib.SharedLogicBase
{
    public class DefaultStateFactory
    {
        public virtual int StateVersion => 0;

        public virtual UserProfile MakeDefaultState(Guid userId)
        {
            var state = new UserProfile
            {
                UserId = userId.ToByteArray(),
                Version = StateVersion
            };

            return state;
        }
    }
}