using System;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using PestelLib.ServerShared;

namespace ServerShared
{
    public class PestelStatsClient
    {
        public static PestelStatsClient Create(string serviceName)
        {
            return new PestelStatsClient(serviceName);
        }

        private PestelStatsClient(string serviceName)
        {
            _graphiteClient = new GraphiteClient("pestelcrew.com", 2003);
            var machineName = Environment.MachineName;
            var serviceDir = Directory.GetCurrentDirectory();
            var wdHash = Crc32.Compute(Encoding.UTF8.GetBytes(serviceDir));
            _prefix = $"{serviceName}.{machineName}.h{wdHash}";
            Log.Debug($"Machine name: {machineName}, dir: {serviceDir}, stats prefix: {_prefix}.");
        }

        public void SendStat(string statName, object value)
        {
            _graphiteClient.SendStat($"{_prefix}.{statName}", value);
        }

        private string _prefix;
        private GraphiteClient _graphiteClient;
        private static readonly ILog Log = LogManager.GetLogger(typeof(PestelStatsClient));
    }
}
