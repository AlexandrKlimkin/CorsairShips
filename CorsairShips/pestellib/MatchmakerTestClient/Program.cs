using System;
using System.Linq;
using System.Threading;
using PestelLib.MatchmakerShared;
using PestelLib.ServerCommon;
using PestelLib.ServerCommon.Threading;
using log4net;
using Newtonsoft.Json;

namespace MatchmakerTestClient
{
    class StubMatchmakingClient : DisposableLoop
    {
        private ILog Log = LogManager.GetLogger(typeof(StubMatchmakingClient));
        private MatchmakerClientProtocol<MatchmakerRequest, StubMatch, StubMatchingStats> _Client;
        private JoinMatchPromise<StubMatch> _CreateRoomPromise;
        private MatchmakerRequest _CurrentRequest;

        protected override void Update(CancellationToken cancellationToken)
        {
            if (_Client == null || !_Client.CanWrite)
                return;

            if (_CurrentRequest != null)
            { // Send request task
                Log.Debug("SendRequest");
                _Client.Register(new ClientMatchmakerRegister<MatchmakerRequest>() { MatchParams = _CurrentRequest });
                _CurrentRequest = null;
            }

            if (_CreateRoomPromise != null)
            { // Create room task
                var p = _CreateRoomPromise;
                ThreadPool.QueueUserWorkItem((o) => HostMatchTask(p));
                _CreateRoomPromise = null;
            }
        }

        public void Connect()
        {
            Log.Debug("Connect");
            if (_Client != null)
            {
                Log.Error("Repeated connection attempt");
                return;
            }

            // подключаемся к серверу ММ
            //var address = "37.187.159.168";
            var address = "localhost";
            var port = 8500;
            _Client = MatchmakerClientFactory.CreateTcp<MatchmakerRequest, StubMatch, StubMatchingStats>(address, port);

            // вешаем обработчики
            _Client.OnHostMatch += _Client_OnHostMatch;
            _Client.OnJoinMatch += _Client_OnJoinMatch;
            _Client.OnKicked += _Client_OnKicked;
            _Client.OnMatchingInfo += _Client_OnMatchingInfo;
            _Client.OnServerInfo += _Client_OnServerInfo;
        }

        private void _Client_OnServerInfo(ServerStats serverStats)
        {
            Log.Debug("Server info: " + JsonConvert.SerializeObject(serverStats));
        }

        private void _Client_OnMatchingInfo(StubMatchingStats stubMatchingStats)
        {
            Log.Debug($"Matching info: players={stubMatchingStats.PlayersInRoom}, waitTime={stubMatchingStats.WaitTime}");
        }

        private void _Client_OnKicked(KickReason obj)
        {
            Log.Debug("Kicked. Reason: " + obj);
            Disconnect();
        }

        private bool _Client_OnJoinMatch(StubMatch arg1)
        {
            Log.DebugFormat("Join match {0} request 1. Bots {1}", arg1.Id, arg1.Party.Count(p => p.IsBot));
            Disconnect();
            return true;
        }

        public void Disconnect()
        {
            if (_Client != null)
            {
                _Client.Dispose();
            }
            _Client = null;
            _CurrentRequest = null;
        }

        public override void Dispose()
        {
            Disconnect();
        }

        public void RequestMatch(MatchmakerRequest request)
        {
            if (_CurrentRequest != null)
            {
                Log.Error("Repeated request attempt");
                return;
            }
            if (_Client == null)
            {
                Log.Error("Request match attempt without connection");
                return;
            }
            _CurrentRequest = request;
        }

        private bool? _Client_OnHostMatch(JoinMatchPromise<StubMatch> response)
        {
            var m = response.Match;
            Log.DebugFormat("Host match {0} request 1. Bots {1}", m.Id, m.Party.Count(p => p.IsBot));
            _CreateRoomPromise = response;
            return null; // можно сразу вернуть true/false, если клиент синхронно обрабатывает запрос
        }

        private void HostMatchTask(JoinMatchPromise<StubMatch> response)
        {
            Log.DebugFormat("Host match {0} request 2", response.Match.Id);
            var matchId = response.Match.Id.ToString();
            Thread.Sleep(100);
            response.TryAnswer(true);
            //Disconnect();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Log.Init();

            Console.Write("Number of clients: ");
            var clientsCount = int.Parse(Console.ReadLine());

            var clients = new StubMatchmakingClient[clientsCount];
            try
            {
                for(var i = 0; i < clients.Length; ++i)
                {
                    clients[i] = new StubMatchmakingClient();
                    clients[i].Connect();
                    clients[i].RequestMatch(new MatchmakerRequest() { PlayerId = Guid.NewGuid() });
                }
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
            finally
            {
                /*
                foreach (var c in clients)
                {
                    c.Dispose();
                }*/
            }
        }
    }
}
