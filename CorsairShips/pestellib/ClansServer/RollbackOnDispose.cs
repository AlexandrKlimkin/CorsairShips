using System;
using System.Collections.Generic;
using log4net;

namespace ClansServerLib
{
    class RollbackOnDispose : IDisposable
    {
        public void AddRollback(Action act)
        {
            _rollbackActions.Add(act);
        }

        public void Clear()
        {
            _rollbackActions.Clear();
        }

        public void Dispose()
        {
            foreach (var rollbackAction in _rollbackActions)
            {
                try
                {
                    rollbackAction();
                }
                catch(Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private List<Action> _rollbackActions = new List<Action>();
        private static readonly ILog Log = LogManager.GetLogger(typeof(RollbackOnDispose));
    }
}
