using System;
using System.Collections.Generic;
using System.Text;
using log4net;

namespace PestelLib.MatchmakerShared
{
    public class JoinMatchPromise<MatchT> where MatchT : IMatch
    {
        private MatchmakerMessageProcessor _messageProcessor;
        public MatchT Match;
        public bool Answered { get; private set; }

        public JoinMatchPromise(MatchT match, MatchmakerMessageProcessor messageProcessor)
        {
            Match = match;
            _messageProcessor = messageProcessor;
        }

        public bool TryAnswer(bool doAcceptMatch)
        {
            if (Answered)
                return false;
            var response = new ClientHostMatchAccepted() { MatchId = Match.Id };
            response.Acceptance = doAcceptMatch;
            Answered = true;
            return _messageProcessor.SendMessage(response) > 0;
        }
    }

    public class MatchmakerClientProtocol<Request, MatchT, StateT> : IDisposable where Request : MatchmakerRequest where MatchT : Match<Request> where StateT : MatchingStats
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatchmakerClientProtocol<Request, MatchT, StateT>));
        private readonly List<IDisposable> _dispose = new List<IDisposable>();
        private readonly MatchmakerMessageProcessor _messageProcessor;

        public event Func<JoinMatchPromise<MatchT>, bool?> OnHostMatch = (p) => null;
        public event Func<MatchT, bool> OnJoinMatch = (m) => false;
        public event Action<KickReason> OnKicked = (k) => { };
        public event Action<StateT> OnMatchingInfo = (s) => { };
        public event Action<ServerStats> OnServerInfo = (s) => { };
        public bool CanWrite { get { return _messageProcessor.CanWrite; } }

        public MatchmakerClientProtocol(MatchmakerMessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor;
            _messageProcessor.OnClose += StreamClosed;
            _dispose.Add(
                _messageProcessor.RegisterCallback<ServerHostMatch<Request, MatchT>>(HostMatch));
            _dispose.Add(
                _messageProcessor.RegisterCallback<ServerJoinMatch<Request, MatchT>>(JoinMatch));
            _dispose.Add(
                _messageProcessor.RegisterCallback<ServerClientKick>(ClientKick));
            _dispose.Add(
                _messageProcessor.RegisterCallback<ServerMatchingStatus<StateT>>(MatchingStatus));
            _dispose.Add(
                _messageProcessor.RegisterCallback<ServerCurrentStats>(ServerStats));
        }

        public int Register(ClientMatchmakerRegister<Request> request)
        {
            return _messageProcessor.SendMessage(request);
        }

        private MatchmakerMessage ClientKick(ServerClientKick kickInfo, long from)
        {
            OnKicked(kickInfo.Reason);
            return null;
        }

        private void StreamClosed(long from)
        {
            OnKicked(KickReason.Disconnect);
        }

        private MatchmakerMessage HostMatch(ServerHostMatch<Request, MatchT> match, long from)
        {
            if (OnHostMatch == null)
                throw new Exception("No handler for HostMatch");

            var r = OnHostMatch(new JoinMatchPromise<MatchT>(match.Match, _messageProcessor));

            if (!r.HasValue)
                return null;

            var response = new ClientHostMatchAccepted() { MatchId = match.Match.Id };
            response.Acceptance = r.Value;
            return response;
        }

        private MatchmakerMessage JoinMatch(ServerJoinMatch<Request, MatchT> match, long from)
        {
            if (OnJoinMatch == null)
                throw new Exception("No handler for JoinMatch");

            var b = OnJoinMatch((MatchT)match.Match);
            if (!b)
                throw new NotSupportedException("Can't decline joining match");
            return null;
        }

        private MatchmakerMessage ServerStats(ServerCurrentStats stats, long from)
        {
            if(OnServerInfo == null)
                throw new Exception("No handler for ServerStats");

            OnServerInfo(stats.Stats);
            return null;
        }

        private MatchmakerMessage MatchingStatus(ServerMatchingStatus<StateT> status, long from)
        {
            if(OnMatchingInfo == null)
                throw new Exception("No handler for MatchingInfo");

            var info = status.State;
            OnMatchingInfo(info);
            return null;
        }

        public void Dispose()
        {
            foreach (var d in _dispose)
            {
                d.Dispose();
            }
            _messageProcessor.OnClose -= StreamClosed;
            _messageProcessor.Dispose();
            _dispose.Clear();
        }
    }
}
