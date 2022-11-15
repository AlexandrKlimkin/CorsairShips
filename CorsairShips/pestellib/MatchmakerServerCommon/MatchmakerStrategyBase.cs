using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using PestelLib.MatchmakerShared;
using PestelLib.MatchmakerServer.Config;
using log4net;
using CaptainLib.Collections;
using PestelLib.ServerCommon.Extensions;

namespace PestelLib.MatchmakerServer
{
    public abstract class MatchmakerStrategyBase<Request, MatchT> : IMatchmakerStrategy, IDisposable where Request : MatchmakerRequest where MatchT : Match<Request>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(IMatchmakerStrategy));
        private List<Request> _incomings = new List<Request>();
        private Queue<TaskCompletionSource<MatchT>> _matchSeekers = new Queue<TaskCompletionSource<MatchT>>();
        private Queue<TaskCompletionSource<List<MatchT>>> _incompleteMatchProcessors = new Queue<TaskCompletionSource<List<MatchT>>>();
        private volatile bool _disposed;
        private volatile int _pendingRequests;
        private long _leaversCount;
        private long _usersMatched;
        private long _botsMatched;
        private long _matchesCount;
        private double _fitmentsSum;
        private long _fitmentsCount;
        private long _awaitTimeTotal;
        private long _discardedMatches;
        private volatile int _pendingMatches;
        private DateTime _startTime = DateTime.UtcNow;
        private AutoResetEvent _newRequestsEvent = new AutoResetEvent(false);
        private List<Guid> _leavers = new List<Guid>();
        private MatchmakerConfig _config;
        private object _sync = new object();
        private ServerStats _stats;
        private DateTime _lastMatchingInfoSent = DateTime.MinValue;

        public int PendingRequestsCount => _pendingRequests;
        public long UsersMatched => _usersMatched;
        public long BotsMatched => _botsMatched;
        public long Uptime => (long)(DateTime.UtcNow - _startTime).TotalMilliseconds;
        public long MatchesCount => _matchesCount;
        public float AverageFitment => (float)(_fitmentsSum / _fitmentsCount);
        public long AverageAwaitTime => MatchesCount > 0 ? _awaitTimeTotal / MatchesCount : 0;
        public int PendingMatches => _pendingMatches;
        public long UsersLeft => _leaversCount;
        public Action<bool> OnComplete = b => { };

        public ServerStats Stats
        {
            get
            {
                lock (_sync)
                {
                    return _stats;
                }
            }
        }

        public MatchmakerStrategyBase(MatchmakerConfig config)
        {
            _config = config;
            var t = Task.Run(() => GeneratorLoop());
            t.ReportOnFail().ContinueWith(b => OnComplete(!b.IsFaulted && b.Result));
        }

        public virtual void NewRequest(Request request)
        {
            lock(_incomings)
                _incomings.Add(request);
            _newRequestsEvent.Set();
        }

        public void NewRequest(MatchmakerRequest request)
        {
            var r = request as Request;
            if(r == null)
                throw new NotSupportedException();
            NewRequest(r);
        }

        public Task<MatchT> GetMatchAsync()
        {
            TaskCompletionSource<MatchT> tcs = new TaskCompletionSource<MatchT>();
            lock (_matchSeekers)
            {
                _matchSeekers.Enqueue(tcs);
            }
            return tcs.Task;
        }

        public Task<List<MatchT>> GetIncompleteMatchesAsync()
        {
            TaskCompletionSource<List<MatchT>> tcs = new TaskCompletionSource<List<MatchT>>();
            lock (_incompleteMatchProcessors)
            {
                _incompleteMatchProcessors.Enqueue(tcs);
            }
            return tcs.Task;
        }

        public void Leave(Guid guid)
        {
            lock (_leavers)
                _leavers.Add(guid);
        }

        Task<IMatch> IMatchmakerStrategy.GetMatchAsync()
        {
            return GetMatchAsync().ContinueWith(t => (IMatch)t.Result);
        }

        Task<IEnumerable<IMatch>> IMatchmakerStrategy.GetIncompleteMatches()
        {
            return GetIncompleteMatchesAsync().ContinueWith(t => t.Result.OfType<IMatch>());
        }

        protected abstract MatchT GenerateMatch(MatchT match, HashSet<Request> pool);

        private List<Request> GetNewRequests()
        {
            List<Request> r = new List<Request>();
            lock (_incomings)
            {
                r.AddRange(_incomings);
                _incomings.Clear();
            }
            return r;
        }

        private void ProcessIncompleteMatches(List<MatchT> matches)
        {
            if(matches.Count == 0)
                return;
            if(DateTime.UtcNow - _lastMatchingInfoSent < _config.SendMatchStatePeriod)
                return;

            matches = new List<MatchT>(matches);
            if (_incompleteMatchProcessors.Count > 0)
            {
                _lastMatchingInfoSent = DateTime.UtcNow;
                lock (_incompleteMatchProcessors)
                {
                    while (_incompleteMatchProcessors.Count > 0)
                    {
                        var processor = _incompleteMatchProcessors.Dequeue();
                        processor.SetResult(matches);
                    }
                }
            }
        }

        private void GeneratorLoop()
        {
            var configLastReload = DateTime.UtcNow;
            var pool = new HashSet<Request>();
            var incompleteMatches = new List<MatchT>();
            var readyMatches = new List<MatchT>();
            var waitTimes = new LimitedQueue<TimeSpan>(10);
            while (!_disposed)
            {
                var poolSize = pool.Count;
                var readyMatchesOld = readyMatches.Count;
                var newRequests = GetNewRequests();
                // add new mm resuests to general pool
                Guid[] leavers;
                lock (_leavers)
                {
                    leavers = _leavers.ToArray();
                    _leavers.Clear();
                }
                var leaversCount = leavers.Length;
                if (newRequests.Count > 0)
                    Log.DebugFormat("{0} new requests", newRequests.Count);
                for (var i = 0; i < newRequests.Count; ++i)
                    pool.Add(newRequests[i]);
                var leaversRemoved = pool.RemoveWhere(r => leavers.Contains(r.PlayerId));
                Interlocked.Exchange(ref _leaversCount, _leaversCount + leaversRemoved);
                _pendingRequests = pool.Count;
                if(leaversRemoved > 0 || leaversCount > 0)
                    Log.DebugFormat("{0} disconnects, {1} matched quit", leaversRemoved, leaversCount - leaversRemoved);
                // try add mm requests (mostly new) to incomplete matches
                foreach (var m in incompleteMatches)
                {
                    LeaveMatch(m, leavers);

                    GenerateMatch(m, pool);
                    if (m.IsFull)
                        readyMatches.Add(m);
                }
                if (pool.Count != poolSize)
                {
                    Log.DebugFormat("Pool size changed: " + (pool.Count - poolSize) + " total: " + pool.Count);
                }
                // remove matches what was completed in current iteration
                for (var i = readyMatchesOld; i < readyMatches.Count; ++i)
                {
                    incompleteMatches.Remove(readyMatches[i]);
                }
                ProcessIncompleteMatches(incompleteMatches);
                // remove empty matches
                var invalidMatches = incompleteMatches.RemoveAll(m => m.CountBots == m.Party.Count());
                Interlocked.Exchange(ref _discardedMatches, _discardedMatches + invalidMatches);
                // no ready matches => wait for new requests
                if (readyMatches.Count < 1)
                {
                    _newRequestsEvent.WaitOne(1000);
                    if (_disposed)
                        break;
                }

                // create new match (depends on specific strategy)
                if (pool.Count > 0)
                {
                    var newMatch = GenerateMatch(null, pool);
                    if (newMatch != null)
                    {
                        if (newMatch.IsFull)
                        {
                            readyMatches.Add(newMatch);
                            Log.DebugFormat("{0} ready matches", readyMatches.Count);
                        }
                        else
                        {
                            incompleteMatches.Add(newMatch);
                            Log.DebugFormat("{0} incomplete matches", incompleteMatches.Count);
                        }
                    }
                }

                // send ready matches to awaiters
                var sent = 0;
                _pendingMatches = readyMatches.Count;
                while (sent < readyMatches.Count && _matchSeekers.Count > 0)
                {
                    var m = readyMatches[sent++];
                    var waitTime = DateTime.UtcNow - m.CreateTime;
                    waitTimes.Enqueue(waitTime);
                    lock (_leavers)
                        foreach (var l in _leavers)
                        {
                            m.Leave(l);
                        }
                    // discard bots only matches
                    if(m.CountBots == m.Party.Count)
                        continue;

                    TaskCompletionSource<MatchT> tcs;
                    lock(_matchSeekers)
                        tcs = _matchSeekers.Dequeue();

                    var playersCount = m.Party.Count;
                    var botsCount = m.CountBots;
                    Interlocked.Increment(ref _matchesCount);
                    Interlocked.Exchange(ref _awaitTimeTotal, _awaitTimeTotal + (long)(DateTime.UtcNow - m.Master.RegTime).TotalMilliseconds);
                    Interlocked.Exchange(ref _fitmentsSum, _fitmentsSum + m.Fitment);
                    Interlocked.Increment(ref _fitmentsCount);
                    Interlocked.Exchange(ref _usersMatched, _usersMatched + playersCount - botsCount);
                    Interlocked.Exchange(ref _botsMatched, _botsMatched + botsCount);
                    tcs.SetResult(m);
                }

                lock (_sync)
                {
                    _stats.PendingMatches = incompleteMatches.Count;
                    if (waitTimes.Count > 0)
                        _stats.AverageWaitTime = TimeSpan.FromMilliseconds(waitTimes.Average(t => t.TotalMilliseconds));
                    _stats.PendingRequests = pool.Count;
                    _stats.MatchesServed += sent;
                }
                readyMatches.RemoveRange(0, sent);
            }
        }

        private void LeaveMatch(IMatch m, Guid[] leavers)
        {
            foreach (var l in leavers)
            {
                m.Leave(l);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _newRequestsEvent.Set();
        }
    }
}
