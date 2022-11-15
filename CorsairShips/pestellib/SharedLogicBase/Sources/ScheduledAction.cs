using System;
using System.Collections.Generic;

namespace PestelLib.SharedLogicBase
{
    public interface IScheduledActionCaller
    {
        Action OnExecuteScheduled { get; set; }
    }

    public class ScheduledAction
    {
        private Action _callbacks = () => { };
        private int _calls;
        private bool _inExec;

        public ScheduledAction(IScheduledActionCaller manager)
        {
            manager.OnExecuteScheduled += Execute;
        }

        public void Schedule()
        {
            _calls++;
        }

        private void Execute()
        {
            // если _callbacks() вызовут другой метод ШЛ, после обработки метода вызовется _sharedLogic.OnExecuteScheduled,
            // что приведет к тому что колбэк отработает снова (даже если не был вызван Schedule для текущего экземпляра, т.к. _calls еще не сброшен в 0), 
            // что в итоге закончится StackOverflow
            // поэтому далее странный код
            if (_inExec || _calls == 0) return;
            try
            {
                _inExec = true;
                for (var i = 0; i < _calls; i++)
                {
                    _callbacks();
                }
            }
            finally 
            {
                _calls = 0;
                _inExec = false;
            }
        }

        public void Subscribe(Action method)
        {
            _callbacks += method;
        }

        public void Unsubscribe(Action method)
        {
            _callbacks -= method;
        }
    }

    public class ScheduledAction<T>
    {
        private Action<T> _callbacks = (x) => { };
        private readonly List<T> _calls = new List<T>();
        private bool _inExec;
        public ScheduledAction(IScheduledActionCaller manager)
        {
            manager.OnExecuteScheduled += Execute;
        }
        public void Schedule(T param)
        {
            _calls.Add(param);
        }

        private void Execute()
        {
            if (_inExec || _calls.Count == 0) return;
            try
            {
                _inExec = true;
                for (var i = 0; i < _calls.Count; i++)
                {
                    _callbacks(_calls[i]);
                }
            }
            finally
            {
                _calls.Clear();
                _inExec = false;
            }
        }

        public void Subscribe(Action<T> method)
        {
            _callbacks += method;
        }

        public void Unsubscribe(Action<T> method)
        {
            _callbacks -= method;
        }

        public void Subscribe() {
            throw new NotImplementedException();
        }
    }

    public class ScheduledAction<T1, T2>
    {
        private Action<T1, T2> _callbacks = (x, y) => { };
        private bool _inExec;

        private readonly List<ParamsContainer> _calls = new List<ParamsContainer>();

        private struct ParamsContainer
        {
            public T1 A;
            public T2 B;
        }
        public ScheduledAction(IScheduledActionCaller manager)
        {
            manager.OnExecuteScheduled += Execute;
        }
        public void Schedule(T1 param1, T2 param2)
        {
            _calls.Add(new ParamsContainer
            {
                A = param1,
                B = param2
            }
            );
        }

        private void Execute()
        {
            if (_inExec || _calls.Count == 0) return;
            try
            {
                _inExec = true;
                for (var i = 0; i < _calls.Count; i++)
                {
                    _callbacks(_calls[i].A, _calls[i].B);
                }
            }
            finally
            {
                _calls.Clear();
                _inExec = false;
            }
        }

        public void Subscribe(Action<T1, T2> method)
        {
            _callbacks += method;
        }

        public void Unsubscribe(Action<T1, T2> method)
        {
            _callbacks -= method;
        }
    }

    public class ScheduledAction<T1, T2, T3>
    {
        private Action<T1, T2, T3> _callbacks = (x, y, z) => { };
        private bool _inExec;

        private readonly List<ParamsContainer> _calls = new List<ParamsContainer>();
        public ScheduledAction(IScheduledActionCaller manager)
        {
            manager.OnExecuteScheduled += Execute;
        }
        private struct ParamsContainer
        {
            public T1 A;
            public T2 B;
            public T3 C;
        }

        public void Schedule(T1 param1, T2 param2, T3 param3)
        {
            _calls.Add(new ParamsContainer
            {
                A = param1,
                B = param2,
                C = param3
            }
            );
        }

        private void Execute()
        {
            if (_inExec || _calls.Count == 0) return;
            try
            {
                _inExec = true;
                for (var i = 0; i < _calls.Count; i++)
                {
                    _callbacks(_calls[i].A, _calls[i].B, _calls[i].C);
                }
            }
            finally
            {
                _calls.Clear();
                _inExec = false;
            }
        }

        public void Subscribe(Action<T1, T2, T3> method)
        {
            _callbacks += method;
        }

        public void Unsubscribe(Action<T1, T2, T3> method)
        {
            _callbacks -= method;
        }
    }
    
    
    public class ScheduledAction<T1, T2, T3, T4>
    {
        private Action<T1, T2, T3, T4> _callbacks = (x, y, z, w) => { };
        private bool _inExec;

        private readonly List<ParamsContainer> _calls = new List<ParamsContainer>();
        public ScheduledAction(IScheduledActionCaller manager)
        {
            manager.OnExecuteScheduled += Execute;
        }
        private struct ParamsContainer
        {
            public T1 A;
            public T2 B;
            public T3 C;
            public T4 D;
        }

        public void Schedule(T1 param1, T2 param2, T3 param3, T4 param4)
        {
            _calls.Add(new ParamsContainer
                       {
                           A = param1,
                           B = param2,
                           C = param3,
                           D = param4
                       }
                      );
        }

        private void Execute()
        {
            if (_inExec || _calls.Count == 0) return;
            try
            {
                _inExec = true;
                for (var i = 0; i < _calls.Count; i++)
                {
                    _callbacks(_calls[i].A, _calls[i].B, _calls[i].C, _calls[i].D);
                }
            }
            finally
            {
                _calls.Clear();
                _inExec = false;
            }
        }

        public void Subscribe(Action<T1, T2, T3, T4> method)
        {
            _callbacks += method;
        }

        public void Unsubscribe(Action<T1, T2, T3, T4> method)
        {
            _callbacks -= method;
        }
    }
}