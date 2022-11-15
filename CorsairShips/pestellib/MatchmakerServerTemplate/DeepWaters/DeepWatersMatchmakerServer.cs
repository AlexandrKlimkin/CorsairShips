using System;
using ServiceCommon;

namespace PestelLib.MatchmakerServer.DeepWaters
{
    public class DeepWatersMatchmakerServer : IDisposable, PestelCrewService
    {
        int _port;
        DeepWatersMatchmakerConfig _config;
        DeepWatersMatchmakerStrategy _strategy;

        public DeepWatersMatchmakerServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            _config = new DeepWatersMatchmakerConfig();
            _strategy = new DeepWatersMatchmakerStrategy(_config);
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
            _config = null;
            _strategy.Dispose();
        }
    }
}
