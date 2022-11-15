using System;

namespace UTPLib.SignalBus {
    public class FiredSignalState {
        public Action OnComplete = null;
        public int CurrentFireCount;
    }
}