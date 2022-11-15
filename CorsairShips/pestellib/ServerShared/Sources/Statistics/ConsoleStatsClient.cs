using System;

namespace ServerShared
{
    public class ConsoleStatsClient : StatsClient
    {
        public override void SendStat(string counter, object value, long ts)
        {
            Console.WriteLine(string.Format("'{0}': '{1}' ({2})", counter, value, ts));
        }
    }
}
