using System;
using System.Collections.Generic;

namespace PestelLib.MecanimTransitionStorage
{
    [Serializable]
    public class TransitionGroup
    {
        public string FromName;
        public int From;
        public List<Transition> Transitions;
    }
}
