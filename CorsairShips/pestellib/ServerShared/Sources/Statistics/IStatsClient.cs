using System;
using PestelLib.ServerShared;

namespace ServerShared
{
    public abstract class StatsClient
    {
        public void SendStat(string counter, object value)
        {
            SendStat(counter, value, TimeUtils.ConvertToUnixTimestamp(DateTime.UtcNow));
        }

        public abstract void SendStat(string counter, object value, long ts);
    }
}
