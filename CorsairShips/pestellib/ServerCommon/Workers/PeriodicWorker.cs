using System;
using System.Collections.Generic;
using System.Threading;
using log4net;

namespace PestelLib.ServerCommon.Workers
{
    public class PeriodicWorker : IDisposable
    {
        public PeriodicWorker(TimeSpan delay, TimeSpan period)
        {
            _timer = new Timer(OnTimer, null, delay, period);
            _actions = new List<Action>();
        }

        public void RegisterWork(Action work)
        {
            _actions.Add(work);
        }

        public void UnregisterWork(Action work)
        {
            _actions.Remove(work);
        }

        private void OnTimer(object state)
        {
            foreach (var action in _actions)
            {
                try
                { 
                    action();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private List<Action> _actions;
        private Timer _timer;
        private static readonly ILog Log = LogManager.GetLogger(typeof(PeriodicWorker));
    }
}
