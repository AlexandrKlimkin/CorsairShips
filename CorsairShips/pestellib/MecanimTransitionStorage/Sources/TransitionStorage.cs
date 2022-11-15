using System.Collections.Generic;
using UnityEngine;

namespace PestelLib.MecanimTransitionStorage
{
    public class TransitionStorage : ScriptableObject, ISerializationCallbackReceiver
    {
        public List<TransitionGroup> Transitions = new List<TransitionGroup>();

        public Dictionary<int, Dictionary<string, float>> TransitionsDictionary = new Dictionary<int, Dictionary<string, float>>();
        
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            foreach (var group in Transitions)
            {
                TransitionsDictionary[group.From] = new Dictionary<string, float>();
                foreach (var transition in group.Transitions)
                {
                    TransitionsDictionary[group.From][transition.To] = transition.Time;
                }
            }
        }
    }
}
