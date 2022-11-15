using System;
using System.Collections.Generic;
using System.Diagnostics;
using StackExchange.Redis;

namespace LeaderboardTest
{
    class Program
    {
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect("localhost,abortConnect=false,ssl=false");
            //return ConnectionMultiplexer.Connect(CloudConfigurationManager.GetSetting("RedisConnectionString"));
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        public static IDatabase Cache
        {
            get { return Connection.GetDatabase(); }
        }


        static void Main(string[] args)
        {
            AddRandomRecords();
            AddRandomUserNames();
            
            //var uids = GetRandomUserIds();
            //GetUserRanks(uids);

            Console.ReadKey();
        }

        private static void GetUserRanks(string[] uids)
        {
            Stopwatch sw = Stopwatch.StartNew();

            //получаем ранг (место) для этих игроков
            for (var i = 0; i < uids.Length; i++)
            {
                Cache.SortedSetRank(
                    "HonorPoints",
                    uids[i],
                    Order.Descending
                    );

                var userName = Cache.HashGet("UserNames", uids[i], CommandFlags.None);
            }
            Console.WriteLine("Spend time: " + sw.Elapsed + " rank: operations per second: " +
                              uids.Length/sw.Elapsed.TotalSeconds);
        }

        private static string[] GetRandomUserIds()
        {
            Stopwatch sw = Stopwatch.StartNew();

            //получаем случайные айдишники пользователей
            var iterator = Cache.SortedSetScan("HonorPoints", default(RedisValue), 3000);
            const int UidCount = 100000;
            var uids = new string[UidCount];
            var index = 0;
            foreach (var sortedSetEntry in iterator)
            {
                uids[index++] = sortedSetEntry.Element;
                if (index == UidCount)
                {
                    break;
                }
            }

            Console.WriteLine("Time to get random " + UidCount + " uids: " + sw.Elapsed);
            return uids;
        }

        private static void AddRandomRecords()
        {
            var rand = new Random();

            for (var i = 0; i < 1000; i++)
            {
                Cache.SortedSetAdd("HonorPoints", Guid.NewGuid().ToString(), rand.Next());
            }
        }
        
        private static void AddRandomUserNames()
        {
            string[] randomNames =
            {
                "{\"UserName\":\"Alexey D\", \"FacebookId\" : \"0001\"}",
                "{\"UserName\":\"Alexey z\", \"FacebookId\" : \"0002\"}",
                "{\"UserName\":\"Alexey S\", \"FacebookId\" : \"0003\"}",
            };

            var iterator = Cache.SortedSetScan("HonorPoints", default(RedisValue), 3000);

            var index = 0;
            foreach (var sortedSetEntry in iterator)
            {
                Cache.HashSet("UserNames", sortedSetEntry.Element, randomNames[(++index)%randomNames.Length], When.Always,
                    CommandFlags.FireAndForget);
            }
        }
    }
}
