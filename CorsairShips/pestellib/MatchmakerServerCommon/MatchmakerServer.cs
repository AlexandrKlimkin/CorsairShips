using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PestelLib.ServerCommon.Threading;
using PestelLib.MatchmakerShared;
using log4net;
using CaptainLib.Collections;
using PestelLib.MatchmakerServer.Config;
using PestelLib.MatchmakerServerCommon;
using PestelLib.ServerCommon.Extensions;

namespace PestelLib.MatchmakerServer
{
    internal class MatchSendStateContext
    {
        public IMatch Match;
        public DateTime LastSend = DateTime.MinValue;
        public int SentHash;
    }

    public class MatchmakerServer : DisposableLoop
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatchmakerServer));
        private IMatchmakerRequestProvider _requestsProvider;
        private IMatchmakerStrategy _strategy;
        private readonly MatchmakerConfig _config;
        private Task<MatchmakerRequest[]> newRequestsTask;
        private Task<IMatch> newMatchTask;
        private Task<IEnumerable<IMatch>> incompleteMatches;
        private Task _sendIncomplete = Task.CompletedTask;
        private MemCache<Guid, MatchSendStateContext> _stateSendCache;
        private volatile bool _initialized;
        private MatchmakerServerStatsGatherer _statsGatherer;

        public MatchmakerServer(IMatchmakerRequestProvider requestsProvider, IMatchmakerStrategy strategy, MatchmakerConfig config)
        {
            _requestsProvider = requestsProvider;
            _strategy = strategy;
            _config = config;
            _stateSendCache = new MemCache<Guid, MatchSendStateContext>();
            _requestsProvider.UserLeave += RequestsProvider_UserLeave;
            _statsGatherer = new MatchmakerServerStatsGatherer(requestsProvider, config);
            _initialized = true;
        }

        public override void Dispose()
        {
            _requestsProvider.UserLeave -= RequestsProvider_UserLeave;
            base.Dispose();
        }

        private void RequestsProvider_UserLeave(Guid id)
        {
            _strategy.Leave(id);
        }

        protected override void Update(CancellationToken cancellation)
        {
            if (!_initialized)
                return;
            cancellation.ThrowIfCancellationRequested();

            // begin/proceed wait for new matchmaker requests
            newRequestsTask = newRequestsTask ?? _requestsProvider.GetRequestsAsync();
            // begin/proceed wait new match
            newMatchTask = newMatchTask ?? _strategy.GetMatchAsync();
            // begin/proceed wait incomplete matches
            incompleteMatches = incompleteMatches ?? _strategy.GetIncompleteMatches();
            // do we have new requests?
            var processNewRequests = _sendIncomplete.IsCompleted && newRequestsTask.Wait(100, cancellation);

            cancellation.ThrowIfCancellationRequested();

            // push new requests to matchmaker strategy
            if (processNewRequests)
            {
                var requests = newRequestsTask.Result;
                for (var i = 0; i < requests.Length; ++i)
                {
                    Log.DebugFormat("Request from user '{0}' passed to the strategy", requests[i].PlayerId);
                    _strategy.NewRequest(requests[i]);
                }
                newRequestsTask = _requestsProvider.GetRequestsAsync();
            }

            // do we have incomplete matches info to send to the players?
            var processIncomplete = _sendIncomplete.IsCompleted && incompleteMatches.Wait(100, cancellation);

            if (processIncomplete)
            {
                if (_config.SendMatchStatePeriod != TimeSpan.Zero)
                {
                    var incomplete = incompleteMatches.Result;
                    var dt = DateTime.UtcNow;
                    foreach (var match in incomplete)
                    {
                        var cached =
                            _stateSendCache.GetOrCreate(
                                match.Id,
                                () => new MatchSendStateContext {Match = match},
                                TimeSpan.FromMinutes(1));
                        if(match.Stats == null)
                            continue;
                        var newHash = match.Stats.GetHashCode();
                        var doSend = cached.SentHash != newHash;
                        doSend &= dt - cached.LastSend > _config.SendMatchStatePeriod;

                        if (doSend)
                        {
                            cached.SentHash = newHash;
                            cached.LastSend = dt;
                            _sendIncomplete = _requestsProvider.SendMatchingInfo(match).ReportOnFail();
                        }
                    }
                }

                incompleteMatches = _strategy.GetIncompleteMatches();
            }
            else if (!_sendIncomplete.IsCompleted)
            {
                if(!_sendIncomplete.Wait(1000, cancellation))
                    Log.Error("SendMatchingInfo delayed.");
            }

            _statsGatherer.StoreStats(_strategy.Stats);

            // do we have new match?
            var processMatch = newMatchTask.Wait(100, cancellation);

            cancellation.ThrowIfCancellationRequested();

            // push new match to users
            if (processMatch)
            {
                var match = newMatchTask.Result;
                Log.DebugFormat("New match ready to announce '{0}'", match.Id);
                _requestsProvider.AnnounceMatch(match).ReportOnFail();
                newMatchTask = _strategy.GetMatchAsync();
            }
        }
    }
}
