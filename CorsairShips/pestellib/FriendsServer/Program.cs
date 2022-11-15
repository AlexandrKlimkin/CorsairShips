using System;
using System.Collections.Generic;
using FriendsClient;
using FriendsClient.Sources;
using MessageServer.Server;
using PestelLib.ServerCommon;
using ServerShared;

namespace FriendsServer
{
    class Program
    {
        private static Server _server;
        private delegate void CmsCall(params string[] args);

        private static Dictionary<string, CmsCall> _cmdMap = new Dictionary<string, CmsCall>()
        {
            ["stats"] = CmdStats
        };

        static void Main(string[] args)
        {
            Log.Init();
            var config = ServerConfigCache.Get();
            var statAddr = HostAddressHelper.GetAddressPort(config.StatsServerAddr);
            var factory = new StorageFactory();
            var graphiteClient = new GraphiteClient(statAddr.host, statAddr.port);

            Dictionary<int, string> firendRequestMap = new Dictionary<int, string>();
            firendRequestMap[0] = "Ping";
            foreach (FriendRequest value in Enum.GetValues(typeof(FriendRequest)))
            {
                firendRequestMap[(int) value] = value.ToString();
            }

            Dictionary<int, string> firendEventMap = new Dictionary<int, string>();
            foreach (FriendEvent value in Enum.GetValues(typeof(FriendEvent)))
            {
                firendEventMap[(int)value] = value.ToString();
            }

            using (_server = new Server())
            {
                var statsGatherer = config.EnableStats ? new MessageServerStatsGatherer(TimeSpan.FromSeconds(15), graphiteClient, _server, firendEventMap) : null;
                var dispStatsGatherer = config.EnableStats ? new DispatcherStatsGatherer(TimeSpan.FromSeconds(15), graphiteClient, _server.MessageDispatcher, firendRequestMap) : null;
                while (true)
                {
                    var command = Console.ReadLine();
                    CmsCall call;
                    if (_cmdMap.TryGetValue(command, out call))
                    {
                        call();
                    }

                    if (command == "exit") break;
                }
                statsGatherer?.Dispose();
                dispStatsGatherer?.Dispose();
            }
        }

        static void CmdStats(params string[] args)
        {
            var s = _server.Stats;

            Console.WriteLine($"{nameof(_server.Stats.ConnectionsCurrent)}: {_server.Stats.ConnectionsCurrent}");
            Console.WriteLine($"{nameof(_server.Stats.MessagesCount)}: {_server.Stats.MessagesCount}");
            Console.WriteLine($"{nameof(_server.Stats.RequestCount)}: {_server.Stats.RequestCount}");
            Console.WriteLine($"{nameof(_server.Stats.NotifyCount)}: {_server.Stats.NotifyCount}");
            Console.WriteLine($"{nameof(_server.Stats.AnswerCount)}: {_server.Stats.AnswerCount}");
            Console.WriteLine($"{nameof(_server.Stats.BytesReceived)}: {_server.Stats.BytesReceived}");
            Console.WriteLine($"{nameof(_server.Stats.BytesSent)}: {_server.Stats.BytesSent}");
        }
    }
}
