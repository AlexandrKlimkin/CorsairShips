using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PestelLib.MatchmakerShared;
using PestelLib.MatchmakerServer.Config;
using log4net;

namespace PestelLib.MatchmakerServer
{
    class MatchmakerServerProtocolUser
    {
        public Guid Guid;
        public long Id;

        public override string ToString()
        {
            return string.Format("{0}:{1}", Id, Guid);
        }
    }

    public class MatchmakerServerProtocol<Request, MatchT, StateT> : IMatchmakerRequestProvider, IDisposable where Request : MatchmakerRequest where MatchT : Match<Request> where StateT : MatchingStats
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(IMatchmakerRequestProvider));
        private readonly List<IDisposable> _dispose = new List<IDisposable>();
        private MatchmakerMessageProcessor _messageProcessor;
        private List<MatchmakerServerProtocolUser> _userBinder = new List<MatchmakerServerProtocolUser>();
        private Dictionary<string, TaskCompletionSource<bool>> _matchAcceptAwaiters = new Dictionary<string, TaskCompletionSource<bool>>();
        private List<Request> _requests = new List<Request>();
        private List<TaskCompletionSource<MatchmakerRequest[]>> _awaiters = new List<TaskCompletionSource<MatchmakerRequest[]>>();
        private MatchmakerConfig _config;
        public event Action<Guid> UserLeave = id => { };

        public MatchmakerServerProtocol(MatchmakerMessageProcessor messageProcessor, MatchmakerConfig config)
        {
            _config = config;
            _messageProcessor = messageProcessor;
            messageProcessor.OnClose += OnClientQuit;
            _dispose.Add(
                messageProcessor.RegisterCallback<ClientMatchmakerRegister<Request>>(OnClientRegister));
            _dispose.Add(
                messageProcessor.RegisterCallback<ClientHostMatchAccepted>(OnMatchAccepted));
        }

        MatchmakerMessage OnClientRegister(ClientMatchmakerRegister<Request> request, long from)
        {
            lock (_userBinder)
            {
                var u = new MatchmakerServerProtocolUser() {Guid = request.MatchParams.PlayerId, Id = from};
                var existingUser = _userBinder.FirstOrDefault(_ => _.Guid == u.Guid);
                if (existingUser != null)
                {
                    Log.InfoFormat("User '{0}' stream migration '{1}' -> '{2}'", u.Guid, existingUser.Id, u.Id);
                    _userBinder.Remove(existingUser); // prevent Leave
                    _messageProcessor.Close(existingUser.Id);
                }
                _userBinder.Add(u);
            }

            request.MatchParams.RegTime = DateTime.UtcNow;
            Log.InfoFormat("New request from user '{0}' from stream '{1}'", request.MatchParams.PlayerId, from);
            SendRequests(request.MatchParams);
            return null;
        }

        private void SendRequests(Request newRequest)
        {
            MatchmakerRequest[] result;
            lock (_requests)
            {
                if(newRequest != null)
                    _requests.Add(newRequest);
                result = _requests.ToArray();
                if(_awaiters.Count > 0)
                    _requests.Clear();
            }
            if (result.Length == 0)
                return;
            lock (_awaiters)
            {
                for (var i = 0; i < _awaiters.Count; ++i)
                {
                    _awaiters[i].SetResult(result);
                }
                _awaiters.Clear();
            }
        }

        MatchmakerMessage OnMatchAccepted(ClientHostMatchAccepted request, long from)
        {
            var k = GetMatchAcceptAwaiterKey(request.MatchId);
            TaskCompletionSource<bool> taskCompletionSource;
            lock (_matchAcceptAwaiters)
                if (!_matchAcceptAwaiters.TryGetValue(k, out taskCompletionSource))
                    return null;
            if (!taskCompletionSource.Task.IsCompleted)
            {
                Log.DebugFormat("MatchAccepted received for match '{0}'. {1}", request.MatchId, request.Acceptance ? "Accepted" : "Rejected");
                taskCompletionSource.SetResult(request.Acceptance);
            }
            return null;
        }

        public Task<MatchmakerRequest[]> GetRequestsAsync()
        {
            TaskCompletionSource<MatchmakerRequest[]> tcs = new TaskCompletionSource<MatchmakerRequest[]>();
            lock (_awaiters)
                _awaiters.Add(tcs);
            SendRequests(null);
            return tcs.Task;
        }

        public async Task AnnounceMatch(IMatch match)
        {
            while (match.Master != null && !match.Master.IsBot)
            {
                var master = match.Master;
                MatchmakerServerProtocolUser user = null;
                lock (_userBinder)
                    user = _userBinder.FirstOrDefault(u => u.Guid == master.PlayerId);
                if (user == null)
                    match.Leave(master.PlayerId);
                else
                {
                    Log.DebugFormat("Announcing match '{0}' to user '{1}'", match.Id, user.Guid);
                    var r = _messageProcessor.SendMessage(new ServerHostMatch<Request, MatchT>() { Match = (MatchT)match }, user.Id);
                    if (r < 1)
                    {
                        Log.WarnFormat("Can't send message to match master {0}", user);
                        match.Leave(user.Guid);
                    }
                    else
                    {
                        var accepted = await WaitAccept(match);
                        if (accepted != WaitAcceptResult.AnsweredAccept)
                        {
                            var reason = accepted == WaitAcceptResult.Timeout ? KickReason.Timeout : KickReason.Rebellion;
                            Log.DebugFormat("Match '{0}' not accepted by user '{1}'. Reason is '{2}'", match.Id, user.Guid, reason);
                            match.Leave(user.Guid);
                            match.Id = Guid.NewGuid();
                            _messageProcessor.SendMessage(new ServerClientKick() { Reason = reason }, user.Id);
                        }
                        else
                        {
                            foreach (var req in match.Party.Where(p => !p.IsBot))
                            {
                                MatchmakerServerProtocolUser reqU;
                                lock (_userBinder)
                                    reqU = _userBinder.FirstOrDefault(u => u.Guid == req.PlayerId);
                                if (reqU == null)
                                {
                                    Log.DebugFormat("Can't ask disconnected user '{0}' to join match '{1}'", req.PlayerId, match.Id);
                                    match.Leave(req.PlayerId);
                                }
                                else
                                {
                                    Log.DebugFormat("Requesting user '{0}' to join match '{1}'", reqU.Guid, match.Id);
                                    _messageProcessor.SendMessage(new ServerJoinMatch<Request, MatchT>() { Match = (MatchT)match }, reqU.Id);
                                    _messageProcessor.Close(reqU.Id);
                                }
                            }
                            Log.DebugFormat("Match '{0}' served", match.Id);
                            return;
                        }
                    }
                }
            }
            if (match.Master != null && match.Master.IsBot)
            {
                Log.WarnFormat("Bot '{0}' cant host match", match.Master.PlayerId);
            }
        }

        public Task SendMatchingInfo(IMatch match)
        {
            foreach (var player in match.Party.Where(p => !p.IsBot).ToArray())
            {
                MatchmakerServerProtocolUser reqU;
                lock (_userBinder)
                    reqU = _userBinder.FirstOrDefault(u => u.Guid == player.PlayerId);
                if (reqU == null)
                {
                    Log.DebugFormat("Can't send match {0} status to user '{1}'", match.Id, player.PlayerId);
                    match.Leave(player.PlayerId);
                    continue;
                }

                _messageProcessor.SendMessage(new ServerMatchingStatus<StateT>() {State = (StateT)match.Stats}, reqU.Id);
            }
            return Task.CompletedTask;
        }

        public Task SendServerInfo(ServerStats stats)
        {
            long[] users;
            lock (_userBinder)
                users = _userBinder.Select(_ => _.Id).ToArray();

            var msg = new ServerCurrentStats {Stats = stats};
            _messageProcessor.SendMessage(msg, users);
            return Task.CompletedTask;
        }

        private static string GetMatchAcceptAwaiterKey(Guid matchId)
        {
            return $"matchaccept_{matchId}";
        }

        enum WaitAcceptResult
        {
            Timeout,
            AnsweredAccept,
            AnsweredReject
        }

        private Task<WaitAcceptResult> WaitAccept(IMatch match)
        {
            var k = GetMatchAcceptAwaiterKey(match.Id);
            var tcs = new TaskCompletionSource<bool>();
            lock(_matchAcceptAwaiters)
                _matchAcceptAwaiters.Add(k, tcs);
            var tWait = tcs.Task.ContinueWith(t =>
            {
                lock (_matchAcceptAwaiters)
                    _matchAcceptAwaiters.Remove(k);
                return t.Result ? WaitAcceptResult.AnsweredAccept : WaitAcceptResult.AnsweredReject;
            });
            var tDelay = Task.Delay(_config.MaxAcceptWaitTime).ContinueWith(t => WaitAcceptResult.Timeout);
            return Task.WhenAny(new[] { tWait, tDelay }).ContinueWith(t => t.Result.Result);
        }

        private void OnClientQuit(long id)
        {
            List<MatchmakerServerProtocolUser> users = null;
            lock (_userBinder)
            {
                users = _userBinder.FindAll(u => u.Id == id);
                if (users.Count < 1)
                    return;

                for (var i = 0; i < users.Count; ++i)
                {
                    Log.DebugFormat("User '{0}' disconnected from stream '{1}'", users[i].Guid, users[i].Id);
                    _userBinder.Remove(users[i]);
                }
            }
            for (var i = 0; i < users.Count; ++i)
            {
                UserLeave(users[i].Guid);
            }
        }

        public void Dispose()
        {
            _messageProcessor.OnClose -= OnClientQuit;
            foreach (var d in _dispose)
            {
                d.Dispose();
            }
            _dispose.Clear();
        }
    }
}
