using System;
using UnityEngine;

namespace GameServerProtocol.Sources.SignalBus
{
    /*
     * такая шина удобна в качестве "локальной" - для передачи сообщений внутри
     * одного gameobject'а
     */
    public class MonoSignalBus : MonoBehaviour, ISignalBus
    {
        private SignalBus _signalBus = new SignalBus();

        public void FireSignal<TSignal>(TSignal signal)
        {
            _signalBus.FireSignal(signal);
        }

        public void Subscribe<TSignal>(Action<TSignal> callback, object identifier)
        {
            _signalBus.Subscribe(callback, identifier);
        }

        public void UnSubscribe<TSignal>(object identifier)
        {
            _signalBus.UnSubscribe<TSignal>(identifier);
        }

        public void UnSubscribeFromAll(object identifier)
        {
            _signalBus.UnSubscribeFromAll(identifier);
        }
    }
}
