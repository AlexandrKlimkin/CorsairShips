using System;
using System.Collections.Generic;
using PestelLib.ServerCommon;
using ServerShared;

namespace MessageServer.Server
{
    public class MessageServerStatsGatherer : StatsGathererBase<MessageServerStats>
    {

        public MessageServerStatsGatherer(TimeSpan gatherPeriod, StatsClient statsClient, IStatsProvider<MessageServerStats> statsProvider, Dictionary<int, string> messageIdMap) 
            : base(gatherPeriod, statsClient, statsProvider)
        {
            _messageIdMap = messageIdMap;
            _serverId = Environment.MachineName;
            if (string.IsNullOrEmpty(_serverId))
                _serverId = "dummy";

            FileHelper.WriteFile(nameof(MessageServerStatsGatherer), _serverId);
        }

        protected override void PrepareAndSend(StatsClient statsClient, MessageServerStats stat)
        {
            var prefix = $"pestel.{_serverId}.msgsrv";
            statsClient.SendStat($"{prefix}.curr_contexts", stat.ContextsListSize);
            statsClient.SendStat($"{prefix}.curr_ids", stat.IdMapSize);
            statsClient.SendStat($"{prefix}.curr_rqueue", stat.RequestQueueSize);

            statsClient.SendStat($"{prefix}.bytes_recv", stat.BytesReceived);
            statsClient.SendStat($"{prefix}.bytes_sent", stat.BytesSent);
            statsClient.SendStat($"{prefix}.curr_conn", stat.ConnectionsCurrent);
            statsClient.SendStat($"{prefix}.ccount", stat.ConnectionsTotal);
            statsClient.SendStat($"{prefix}.ncount", stat.NotifyCount);
            statsClient.SendStat($"{prefix}.rcount", stat.RequestCount);
            statsClient.SendStat($"{prefix}.acount", stat.AnswerCount);
            statsClient.SendStat($"{prefix}.mcount", stat.MessagesCount);

            prefix += ".msg";
            foreach (var item in stat.MessageCountByType)
            {
                if (!_messageIdMap.TryGetValue(item.Key, out var name))
                    name = "rq" + item.Key;

                statsClient.SendStat($"{prefix}.{name}", item.Value);
            }

        }

        private readonly string _serverId;
        private readonly Dictionary<int, string> _messageIdMap;
    }
}
