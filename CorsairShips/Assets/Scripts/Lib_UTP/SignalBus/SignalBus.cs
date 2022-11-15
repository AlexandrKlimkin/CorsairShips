using System;
using System.Collections.Generic;
//using GameServerProtocol.Sources.Logger;
using UnityDI;
using UnityEngine;

namespace UTPLib.SignalBus {
    public sealed class SignalBus {
#if UNITY_EDITOR && !DISABLE_SIGNAL_BUS_LOGS
        [Dependency]
        private readonly ILogger _Logger;
#endif
        private readonly Dictionary<Type, Dictionary<object, SignalSubscriptionWrapper>> _SubscriptionsMap =
            new Dictionary<Type, Dictionary<object, SignalSubscriptionWrapper>>();

        private readonly Dictionary<Type, FiredSignalState> _CurrentFiredSignals =
            new Dictionary<Type, FiredSignalState>();

        public void Subscribe<TSignal>(Action<TSignal> callback, object identifier) {
            RunOrSchedule(typeof(TSignal), () => {
                if (!_SubscriptionsMap.TryGetValue(typeof(TSignal), out var subscriptions)) {
                    subscriptions = new Dictionary<object, SignalSubscriptionWrapper>();
                    _SubscriptionsMap.Add(typeof(TSignal), subscriptions);
                }

                subscriptions.Add(identifier, new SignalSubscription<TSignal>(callback, identifier));
            });
        }

        public void UnSubscribe<TSignal>(object identifier) {
            RunOrSchedule(typeof(TSignal), () => {
                if (!_SubscriptionsMap.TryGetValue(typeof(TSignal), out var subscriptions)) {
//#if UNITY_EDITOR && !DISABLE_SIGNAL_BUS_LOGS
//                    _Logger?.ConditionalLogWarning($"{identifier} not subscribed to signal {typeof(TSignal)}");
//#endif
                    return;
                }

                subscriptions.Remove(identifier);
            });
        }

        public void UnSubscribeFromAll(object identifier) {
            foreach (var subscriptions in _SubscriptionsMap) {
                RunOrSchedule(subscriptions.Key, () => { subscriptions.Value.Remove(identifier); });
            }
        }

        /// avoid circular fire (firing signal from it's listener)
        public void FireSignal<TSignal>(TSignal signal) {
            if (!_CurrentFiredSignals.TryGetValue(typeof(TSignal), out var state)) {
                state = new FiredSignalState();
                _CurrentFiredSignals.Add(typeof(TSignal), state);
            }

            state.CurrentFireCount++;
            var subscriptions = GetSignalSubscriptions<TSignal>();
            if (subscriptions != null) {
                foreach (var subscription in subscriptions) {
                    ((SignalSubscription<TSignal>)subscription).Callback.Invoke(signal);
                }
            }

            state.OnComplete?.Invoke();
            state.CurrentFireCount--;
            state.OnComplete = null;
            if (state.CurrentFireCount == 0)
                _CurrentFiredSignals.Remove(typeof(TSignal));
        }

        private Dictionary<object, SignalSubscriptionWrapper>.ValueCollection GetSignalSubscriptions<TSignal>() {
            if (!_SubscriptionsMap.TryGetValue(typeof(TSignal), out var subscriptions)) {
//#if UNITY_EDITOR && !DISABLE_SIGNAL_BUS_LOGS
//                _Logger?.ConditionalLogWarning($"no subscriptions to signal {typeof(TSignal)}");
//#endif
                return null;
            }

            return subscriptions.Values;
        }

        private void RunOrSchedule(Type signalType, Action operation) {
            if (_CurrentFiredSignals.TryGetValue(signalType, out var signal)) {
                signal.OnComplete += operation;
            }
            else {
                operation.Invoke();
            }
        }
    }
}