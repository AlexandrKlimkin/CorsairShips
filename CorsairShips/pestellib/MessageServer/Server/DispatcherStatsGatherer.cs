using System;
using System.Collections.Generic;
using MessageClient.Sources;
using ServerShared;

namespace MessageServer.Server
{
    public class DispatcherStatsGatherer : StatsGathererBase<BaseMessageDispatcher.DispatcherStatistics>
    {

        public DispatcherStatsGatherer(TimeSpan gatherPeriod, StatsClient statsClient, BaseMessageDispatcher dispatcher, Dictionary<int, string> messageTypeMap) 
            : base(gatherPeriod, statsClient, new StatsProviderFuncWrapper(() => dispatcher.Stats))
        {
            _messageTypeMap = messageTypeMap;
            _serverId = Environment.MachineName;
            if (string.IsNullOrEmpty(_serverId))
                _serverId = "dummy";
        }

        protected override void PrepareAndSend(StatsClient statsClient, BaseMessageDispatcher.DispatcherStatistics stat)
        {
            var prefix = $"pestel.{_serverId}.msgsrv.disp";

            var requestStats = stat.GetRequestsStats();
            statsClient.SendStat($"{prefix}.handled", stat.HandledCount);
            statsClient.SendStat($"{prefix}.unhandled", stat.UnhandledCount);
            statsClient.SendStat($"{prefix}.msg_count", stat.MessagesCount);
            statsClient.SendStat($"{prefix}.avg_time", stat.AverageHandleTime);

            prefix += ".msg";
            foreach (var requestStat in requestStats)
            {
                if (!_messageTypeMap.TryGetValue(requestStat.MessageType, out var name))
                    name = "rq" + requestStat.MessageType;
                statsClient.SendStat($"{prefix}.{name}.max", requestStat.Stats.Max);
                statsClient.SendStat($"{prefix}.{name}.min", requestStat.Stats.Min);
                statsClient.SendStat($"{prefix}.{name}.med", requestStat.Stats.Med);
                statsClient.SendStat($"{prefix}.{name}.perc75", requestStat.Stats.Perc75);
                statsClient.SendStat($"{prefix}.{name}.perc90", requestStat.Stats.Perc90);
                statsClient.SendStat($"{prefix}.{name}.perc95", requestStat.Stats.Perc95);
                statsClient.SendStat($"{prefix}.{name}.pcount", requestStat.Stats.Count);
            }
        }

        private readonly string _serverId;
        private readonly Dictionary<int, string> _messageTypeMap;
    }
}
