using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BackendCommon.Code;
using PestelLib.ServerCommon.Threading;
using S;
using ServerLib;
using ServerLib.Modules;

namespace Backend.Code.Statistics
{
    class OpContext
    {
        private int _pos;
        private long _count;
        private long[] _time = new long[20];
        private object _sync = new object();

        public long Count => _count;

        public OpContext Add(Stopwatch sw)
        {
            lock (_sync)
            {
                _time[_pos] = sw.ElapsedMilliseconds;
                if (++_pos > 19) _pos = 0;
                ++_count;
            }

            return this;
        }

        public long Average()
        {
            var p = _count > 19 ? 20 : _pos;
            var avg = 0L;
            for (var i = 0; i < p; ++i)
            {
                avg += _time[i];
            }
            return avg / p;
        }
    }

    public class MainHandlerBaseStats : DisposableLoop
    {
        private static long _requestCount;
        private static long _failedRequestCount;
        private static long _processedRequestCount; // failed + successful
        private const string statCategory = "backend";
        private const string statRcCategory = "backend.rc";
        private const string statOpTimeCategory = "backend.optime";
        private const string statOpsCategory = "backend.ops";
        private static readonly Lazy<MainHandlerBaseStats> _instance = new Lazy<MainHandlerBaseStats>(() => new MainHandlerBaseStats(MainHandlerBase.ServiceProvider));
        private DefaultStatisticsClient _stats;
        private CustomRequestContextBuffer _requestContextBuffer = new CustomRequestContextBuffer();
        private ConcurrentDictionary<string, long> _rcMap = new ConcurrentDictionary<string, long>();
        private ConcurrentDictionary<string, OpContext> _opMap = new ConcurrentDictionary<string, OpContext>();
        private ConcurrentQueue<Guid> _recentPlayers = new ConcurrentQueue<Guid>();
        private Dictionary<Guid, DateTime> _playersOnline = new Dictionary<Guid, DateTime>();
        private ManualResetEventSlim _newPlayerEvent = new ManualResetEventSlim();

        public static MainHandlerBaseStats Instance => _instance.Value;

        private MainHandlerBaseStats(IServiceProvider serviceProvider)
        {
            if (!AppSettings.Default.ProfileMainHandler)
                return;

            _stats = serviceProvider.GetService(typeof(DefaultStatisticsClient)) as DefaultStatisticsClient;
            foreach (var value in Enum.GetValues(typeof(ResponseCode)))
            {
                _rcMap[value.ToString()] = 0;
            }
        }

        public bool IsValid => _stats != null;

        public void NewRequest()
        {
            if(!IsValid) return;
            var val = Interlocked.Increment(ref _requestCount);
            _stats?.SendAsync(statCategory, "req_count", val);
        }

        public CustomRequestContextBuffer.CustomRequestContext NewCustomRequest(string operation)
        {
            return _requestContextBuffer.Allocate(operation);
        }

        public void NewPlayer(Guid playerId)
        {
            _recentPlayers.Enqueue(playerId);
            _newPlayerEvent.Set();
        }

        public void NewResponse(Response response, Stopwatch sw, IModule module)
        {
            long val;
            if (!IsValid) return;
            if (response == null) return;
            if (response.ResponseCode == ResponseCode.OK)
            {
                val = Interlocked.Increment(ref _processedRequestCount);
                _stats?.SendAsync(statCategory, "proccessed", val);
            }
            else
            {
                val = Interlocked.Increment(ref _failedRequestCount);
                _stats?.SendAsync(statCategory, "failed", val);
            }

            if (module != null && sw != null)
            {
                var opName = module.GetType().Name;
                StatResponse(sw, opName);
            }

            var key = response.ResponseCode.ToString();
            val = _rcMap.AddOrUpdate(key, s => 1, (s, l) => l + 1);
            _stats?.SendAsync(statRcCategory, key, val);
        }

        private void StatResponse(Stopwatch sw, string operation)
        {
            var ctx = _opMap.AddOrUpdate(operation, new OpContext().Add(sw), (s, context) => context.Add(sw));
            _stats?.SendAsync(statOpsCategory, operation, ctx.Count);
            _stats?.SendAsync(statOpTimeCategory, operation, ctx.Average());
        }

        private Guid[] _buffer = new Guid[10];
        private DateTime _lastSent = DateTime.MinValue;
        protected override void Update(CancellationToken cancellationToken)
        {
            try
            {
                _newPlayerEvent.Wait();
            }
            catch (ThreadAbortException)
            {
                return;
            }

            var dt = DateTime.UtcNow;
            while (!_recentPlayers.IsEmpty)
            {
                if(!_recentPlayers.TryDequeue(out var playerId))
                    continue;
                _playersOnline[playerId] = dt;
            }

            var pos = 0;
            if(_playersOnline.Count > _buffer.Length)
                _buffer = new Guid[_playersOnline.Count];
            foreach (var kv in _playersOnline)
            {
                if (dt - kv.Value > TimeSpan.FromSeconds(60))
                {
                    _buffer[pos++] = kv.Key;
                }
            }

            for (var i = 0; i < pos; i++)
            {
                _playersOnline.Remove(_buffer[i]);
            }

            if (dt - _lastSent > TimeSpan.FromSeconds(10))
            {
                var ccu = _playersOnline.Count;
                _stats?.SendAsync(statCategory, "ccu", ccu);
                _lastSent = DateTime.UtcNow;
            }

            _newPlayerEvent.Reset();
        }

        public class CustomRequestContextBuffer
        {
            private ConcurrentDictionary<string, ConcurrentBag<CustomRequestContext>> _requestContexts = new ConcurrentDictionary<string, ConcurrentBag<CustomRequestContext>>();

            public CustomRequestContext Allocate(string requestName)
            {
                var pool = _requestContexts.GetOrAdd(requestName, s => new ConcurrentBag<CustomRequestContext>());
                CustomRequestContext item;
                while (pool.TryTake(out item))
                {
                    Thread.SpinWait(1);
                }

                if (item == null)
                {
                    item = new CustomRequestContext(this, requestName);
                }
                else
                {
                    item.Reset();
                }

                return item;
            }

            void Free(CustomRequestContext ctx)
            {
                var pool = _requestContexts.GetOrAdd(ctx.Operation, s => new ConcurrentBag<CustomRequestContext>());
                pool.Add(ctx);
            }

            public class CustomRequestContext : IDisposable
            {
                private readonly CustomRequestContextBuffer _parent;
                private readonly string _operation;
                private readonly Stopwatch _sw;

                public string Operation => _operation;
                public bool Error;

                public CustomRequestContext(CustomRequestContextBuffer parent, string operation)
                {
                    _parent = parent;
                    _operation = operation;
                    _sw = Stopwatch.StartNew();
                    Reset();
                }

                public void Reset()
                {
                    _sw.Restart();
                }

                public void Dispose()
                {
                    Instance.StatResponse(_sw, !Error ? _operation : $"{_operation}_Error");
                    _parent.Free(this);
                }
            }
        }
    }
}