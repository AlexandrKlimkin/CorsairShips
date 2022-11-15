using System;
using System.Collections.Generic;
using System.Text;
using ServerShared;
using System.Threading;

namespace PestelLib.ServerCommon.Extensions
{
    public static class ObjectScopeGuardExtensions
    {
        /// <summary>
        /// Locks calling thread until event rised
        /// </summary>
        /// <param name="autoResetEvent"></param>
        /// <returns></returns>
        public static ObjectScopeGuard<AutoResetEvent> ScopeGuard(this AutoResetEvent autoResetEvent)
        {
            autoResetEvent.WaitOne();
            return new ObjectScopeGuard<AutoResetEvent>(autoResetEvent, (e) => e.Set());
        }
    }
}
