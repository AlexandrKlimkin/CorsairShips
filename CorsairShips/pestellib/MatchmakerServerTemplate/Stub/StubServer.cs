using System;
using PestelLib.MatchmakerServer.Config;
using PestelLib.MatchmakerShared;
using ServiceCommon;

namespace PestelLib.MatchmakerServer.Stub
{
    public class StubServer : IDisposable, PestelCrewService
    {
        MatchmakerConfig _config;
        StubStrategy _strategy;
        MatchmakerServer _server;
        MatchmakerServerProtocol<MatchmakerRequest, StubMatch, StubMatchingStats> _proto;
        TcpMatchmakerServer _tcp;

        public StubServer()
        {
        }

        private void ConfigOnServerPortChanged(int i)
        {
            Stop();
            Start();
        }

        public void Start()
        {
            _config = new MatchmakerConfig();
            _strategy = new StubStrategy(_config);
            // (optional) setup statistics gatherer
            // var gatherer = new MatchmakerStatsGatherer(_strategy, TimeSpan.FromSeconds(15), new ConsoleStatsClient());
            _tcp = new TcpMatchmakerServer(_config.ServerPort);
            _proto = new MatchmakerServerProtocol<MatchmakerRequest, StubMatch, StubMatchingStats>(_tcp.Processor, _config);
            _server = new MatchmakerServer(_proto, _strategy, _config);
        }

        public void Stop()
        {
            Terminate();
        }

        public void Terminate()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_server != null)
            {
                _server.Dispose();
                _server = null;
            }

            if (_proto != null)
            {
                _proto.Dispose();
                _proto = null;
            }

            if (_strategy != null)
            {
                _strategy.Dispose();
                _strategy = null;
            }

            if (_tcp != null)
            {
                _tcp.Dispose();
                _tcp = null;
            }

            _config = null;
        }
    }
}
