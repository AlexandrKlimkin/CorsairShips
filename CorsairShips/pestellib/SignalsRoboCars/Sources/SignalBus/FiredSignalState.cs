using System;

namespace GameServerProtocol.Sources.SignalBus {
    public struct FiredSignalState {
        public Action OnComplete;
        public int CurrentFireCount;
    }
}