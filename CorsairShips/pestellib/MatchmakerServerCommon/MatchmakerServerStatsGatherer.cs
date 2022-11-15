using System;
using System.Threading.Tasks;
using PestelLib.MatchmakerServer;
using PestelLib.MatchmakerServer.Config;
using PestelLib.MatchmakerShared;
using PestelLib.ServerCommon.Extensions;

namespace PestelLib.MatchmakerServerCommon
{
    public class MatchmakerServerStatsGatherer
    {
        private readonly MatchmakerConfig _config;
        private ServerStats _stats;
        private DateTime _lastSend = DateTime.MinValue;
        private IMatchmakerRequestProvider _requestProvider;
        private volatile int _sentHash;

        public event Action<ServerStats> OnSend = (s) => { };

        public MatchmakerServerStatsGatherer(IMatchmakerRequestProvider requestProvider, MatchmakerConfig config)
        {
            _requestProvider = requestProvider;
            _config = config;
        }

        public void StoreStats(ServerStats stats)
        {
            _stats = stats;
            TryBroadcast();
        }

        private void TryBroadcast()
        {
            if(_config.SendServerStatsPeriod == TimeSpan.Zero)
                return;
            if(_stats.GetHashCode() == _sentHash)
                return;
            if(DateTime.UtcNow - _lastSend < _config.SendServerStatsPeriod)
                return;
            _requestProvider.SendServerInfo(_stats).ReportOnFail();
            _sentHash = _stats.GetHashCode();
            _lastSend = DateTime.UtcNow;
        }
    }
}
