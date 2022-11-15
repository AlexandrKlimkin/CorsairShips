using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Extensions.Caching.Memory;

namespace Backend.Code.Sync
{
    public class UserLock
    {
        private object _sync = new object();
        private MemoryCache _locks = new MemoryCache(new MemoryCacheOptions());

        public object GetStringLockObject(string k)
        {
            object l;
            if (_locks.TryGetValue(k, out l))
                return l;
            lock (_sync)
            {
                l = _locks.Get(k);
                if (l != null)
                    return l;
                l = new object();
                var e = _locks.CreateEntry(k);
                e.Value = l;
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            }
            return l;
        }

        public object GetUserLockObject(Guid uid)
        {
            return GetStringLockObject(uid.ToString());
        }
    }
}