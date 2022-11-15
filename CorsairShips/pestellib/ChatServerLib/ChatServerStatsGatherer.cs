using System;
using PestelLib.ServerShared;
using System.Timers;
using ServerShared;

namespace PestelLib.ChatServer
{
    class ChatServerStatsGatherer : StatsGathererBase<ChatServiceStats>
    {
        private readonly ChatServerConfig _config;
        Timer _timer;

        public ChatServerStatsGatherer(TimeSpan gatherPeriod, StatsClient statsClient, IStatsProvider<ChatServiceStats> statsProvider, ChatServerConfig config)
            :base(gatherPeriod, statsClient, statsProvider)
        {
            _config = config;
            _timer = new Timer(gatherPeriod.TotalMilliseconds);
        }

        protected override void PrepareAndSend(StatsClient statsClient, ChatServiceStats s)
        {
            var serverId = _config.ChatServerName;
            statsClient.SendStat($"pestel.{serverId}.chat.curr_connections", s.CurrentConnections);
            statsClient.SendStat($"pestel.{serverId}.chat.closed_connections", s.ClosedConnections);
            statsClient.SendStat($"pestel.{serverId}.chat.accepted_connections", s.AcceptedConnections);
            statsClient.SendStat($"pestel.{serverId}.chat.rooms_count", s.RoomsCount);
            statsClient.SendStat($"pestel.{serverId}.chat.bytes_received", s.BytesReceived);
            statsClient.SendStat($"pestel.{serverId}.chat.bytes_sent", s.BytesSent);
            statsClient.SendStat($"pestel.{serverId}.chat.bans_granted", s.FloodBans);
            statsClient.SendStat($"pestel.{serverId}.chat.ban_max_lvl", s.MaxFloodBanLevel);
            var roomBase = $"pestel.{serverId}.chat.rooms.";
            foreach (var kv in s.UsersInRooms)
            {
                var roomCounter = roomBase + kv.Key;
                statsClient.SendStat(roomCounter, kv.Value);
            }
            var banBase = $"pestel.{serverId}.chat.roomsbans.";
            foreach (var kv in s.BansInRooms)
            {
                var roomBanCounter = banBase + kv.Key;
                statsClient.SendStat(roomBanCounter, kv.Value);
            }
        }
    }
}
