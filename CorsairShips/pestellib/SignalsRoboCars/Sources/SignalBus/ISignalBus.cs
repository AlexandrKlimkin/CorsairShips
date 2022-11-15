using System;

namespace GameServerProtocol.Sources.SignalBus
{
    interface ISignalBus
    {
        void Subscribe<TSignal>(Action<TSignal> callback, object identifier);
        void UnSubscribe<TSignal>(object identifier);
        void UnSubscribeFromAll(object identifier);
        void FireSignal<TSignal>(TSignal signal);      
    }
}
