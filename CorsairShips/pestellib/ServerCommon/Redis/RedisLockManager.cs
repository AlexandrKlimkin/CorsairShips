using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using ServerLib;

namespace PestelLib.ServerCommon.Redis
{
    public interface ILockManager
    {
        Task<IDisposable> LockAsync(string resource, int timeout, bool dontThrow);
        IDisposable Lock(string resource, int timeout, bool dontThrow);
    }

    public class RedisLockManager : ILockManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RedisLock));
        public const long LockTimeThreshold = 20;
        public const bool DefaultDontThrow = false;

        class RedisLock : IDisposable
        {
            private static readonly TimeSpan ttl = TimeSpan.FromMinutes(1);
            private static readonly TimeSpan httl = TimeSpan.FromMinutes(0.5);
            private string _resource;
            private string _token;
            private Timer _timer;
            RedisLock(string resource, string token)
            {
                _resource = resource;
                _token = token;
                _timer = new Timer(RenewLock, null, httl, httl);
            }

            public static IDisposable Lock(string resource)
            {
                var token = Guid.NewGuid().ToString("N");
                var b = RedisUtils.Cache.LockTake(resource, token, ttl);
                if (!b)
                    return null;
                return new RedisLock(resource, token);
            }

            public void Dispose()
            {
                _timer.Dispose();
                var r = RedisUtils.Cache.LockRelease(_resource, _token);
            }

            private void RenewLock(object state)
            {
                RedisUtils.Cache.LockExtend(_resource, _token, ttl);
            }
        }

        public Task<IDisposable> LockAsync(string resource, int timeout = 0, bool dontThrow = DefaultDontThrow)
        {
            return Task.Run(() => Lock(resource, timeout));
        }

        public IDisposable Lock(string resource, int timeout = 0, bool dontThrow = DefaultDontThrow)
        {
            IDisposable r;
            var sw = Stopwatch.StartNew();
            var overtime = 0;
            do
            {
                if (timeout > 0 && sw.ElapsedMilliseconds >= timeout)
                {
                    if (dontThrow)
                        return null;
                    throw new Exception($"Lock {resource} aquire failed due to timeout");
                }

                if (sw.ElapsedMilliseconds > LockTimeThreshold)
                {
                    var ovt = (int)(sw.Elapsed.TotalSeconds / LockTimeThreshold);
                    if (ovt > overtime)
                    {
                        overtime = ovt;
                        Log.WarnFormat($"Storage lock overtime {sw.ElapsedMilliseconds} ms. Stack: {Environment.StackTrace}");
                    }
                }

                r = RedisLock.Lock(resource);
                if (r != null)
                    break;
                Thread.Sleep(100);
            } while (true);

            if (overtime > 0)
            {
                Log.WarnFormat($"Storage lock taken in {sw.ElapsedMilliseconds} ms");
            }

            return r;
        }
    }
}
