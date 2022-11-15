using System;
using ServerShared;

namespace PestelLib.MatchmakerServer.Statistics
{
    class MatchmakerStatsProdiver : IStatsProvider<MatchmakerStrategyStats>
    {
        private readonly IMatchmakerStrategy _strategy;
        private readonly MatchmakerStrategyStats _stats = new MatchmakerStrategyStats();

        public MatchmakerStatsProdiver(IMatchmakerStrategy strategy)
        {
            _strategy = strategy;
        }

        public MatchmakerStrategyStats GetStats()
        {
            _stats.PendingRequestsCount = _strategy.PendingRequestsCount;
            _stats.UsersMatched = _strategy.UsersMatched;
            _stats.BotsMatched = _strategy.BotsMatched;
            _stats.Uptime = _strategy.Uptime;
            _stats.AverageFitment = _strategy.AverageFitment;
            _stats.MatchesCount = _strategy.MatchesCount;
            _stats.AverageAwaitTime = _strategy.AverageAwaitTime;
            _stats.PendingMatches = _strategy.PendingMatches;
            _stats.UsersLeft = _strategy.UsersLeft;
            return _stats;
        }
    }

    public class MatchmakerStatsGatherer : StatsGathererBase<MatchmakerStrategyStats>
    {
        private readonly string _serverId;

        public MatchmakerStatsGatherer(IMatchmakerStrategy strategy, TimeSpan gatherPeriod, StatsClient statsClient, string serverId = "dummy")
            :base(gatherPeriod, statsClient, new MatchmakerStatsProdiver(strategy))
        {
            _serverId = serverId;
        }

        protected override void PrepareAndSend(StatsClient statsClient, MatchmakerStrategyStats stat)
        {
            statsClient.SendStat($"pestel.{_serverId}.mm.ccu", stat.PendingRequestsCount);
            statsClient.SendStat($"pestel.{_serverId}.mm.uptime", stat.Uptime);
            statsClient.SendStat($"pestel.{_serverId}.mm.users_matched", stat.UsersMatched);
            statsClient.SendStat($"pestel.{_serverId}.mm.bots_matched", stat.BotsMatched);
            statsClient.SendStat($"pestel.{_serverId}.mm.bot_fraction", stat.BotFraction);
            statsClient.SendStat($"pestel.{_serverId}.mm.avg_fitment", stat.AverageFitment);
            statsClient.SendStat($"pestel.{_serverId}.mm.matches_total", stat.MatchesCount);
            statsClient.SendStat($"pestel.{_serverId}.mm.avg_match_time", stat.AverageAwaitTime);
            statsClient.SendStat($"pestel.{_serverId}.mm.pending_matches", stat.PendingMatches);
            statsClient.SendStat($"pestel.{_serverId}.mm.users_left", stat.UsersLeft);
        }
    }
}
