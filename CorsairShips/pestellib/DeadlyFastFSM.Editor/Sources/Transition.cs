using System;

namespace DeadlyFast
{
    [System.Serializable]
    public class Transition
    {
        public string Event = "";
        public string TargetState = "";

        [NonSerialized] public bool RenameInProcess = false;
        [NonSerialized] public string EventCandidate = "";
    }
}
