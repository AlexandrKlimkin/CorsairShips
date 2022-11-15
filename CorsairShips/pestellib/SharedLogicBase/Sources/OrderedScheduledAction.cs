using System;
using System.Collections.Generic;

namespace PestelLib.SharedLogicBase {


    public interface IOrderedScheduledActionCaller {
        void Schedule(Action action);

        void Execute();
    }

    public class OrderedScheduledActionCaller : IOrderedScheduledActionCaller {

        private const int _MaxActionsCount = 100000;
        private Queue<Action> _ActionQueue;

        public OrderedScheduledActionCaller() {
            _ActionQueue = new Queue<Action>();
        }

        public void Schedule(Action action) {
            if (action == null)
                return;
            _ActionQueue.Enqueue(action);
        }
        
        public void Execute() {
            var delegateCounter = 0; // Heavy recursion defence
            while(_ActionQueue.Count > 0 && delegateCounter < _MaxActionsCount) {
                delegateCounter++;
                var action = _ActionQueue.Dequeue();
                action.Invoke();
            }
        }
    }

    public abstract class OrderedScheduledActionBase {
        protected IOrderedScheduledActionCaller _Caller;

        public OrderedScheduledActionBase(IOrderedScheduledActionCaller caller) {
            _Caller = caller;
        }
    }

    public class OrderedScheduledAction : OrderedScheduledActionBase {
        private Action _callbacks = () => { };

        public OrderedScheduledAction(IOrderedScheduledActionCaller caller) : base(caller) {
        }

        public void Subscribe(Action method) {
            _callbacks += method;
        }

        public void Unsubscribe(Action method) {
            _callbacks -= method;
        }

        public void Schedule() {
            _Caller.Schedule(_callbacks);
        }
    }

    public class OrderedScheduledAction<T> : OrderedScheduledActionBase {
        private Action<T> _callbacks = (x) => { };

        public OrderedScheduledAction(IOrderedScheduledActionCaller caller) : base(caller) {
        }

        public void Subscribe(Action<T> method) {
            _callbacks += method;
        }

        public void Unsubscribe(Action<T> method) {
            _callbacks -= method;
        }

        public void Schedule(T param) {
            _Caller.Schedule(() => _callbacks(param));
        }
    }

    public class OrderedScheduledAction<T1, T2> : OrderedScheduledActionBase {
        private Action<T1, T2> _callbacks = (x, y) => { };

        public OrderedScheduledAction(IOrderedScheduledActionCaller caller) : base(caller) {
        }

        public void Subscribe(Action<T1, T2> method) {
            _callbacks += method;
        }

        public void Unsubscribe(Action<T1, T2> method) {
            _callbacks -= method;
        }

        public void Schedule(T1 param1, T2 param2) {
            _Caller.Schedule(() => _callbacks(param1, param2));
        }
    }

    public class OrderedScheduledAction<T1, T2, T3> : OrderedScheduledActionBase {
        private Action<T1, T2, T3> _callbacks = (x, y, z) => { };

        public OrderedScheduledAction(IOrderedScheduledActionCaller caller) : base(caller) {
        }

        public void Subscribe(Action<T1, T2, T3> method) {
            _callbacks += method;
        }

        public void Unsubscribe(Action<T1, T2, T3> method) {
            _callbacks -= method;
        }

        public void Schedule(T1 param1, T2 param2, T3 param3) {
            _Caller.Schedule(() => _callbacks(param1, param2, param3));
        }
    }
}