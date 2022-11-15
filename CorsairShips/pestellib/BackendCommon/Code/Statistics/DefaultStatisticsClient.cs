using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PestelLib.ServerCommon.Extensions;
using PestelLib.ServerShared;
using ServerLib;
using ServerShared;

namespace Backend.Code.Statistics
{
    public class DefaultStatisticsClient : IDisposable
    {
        public readonly bool BatchingEnabled = true;
        public readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(15);

        private readonly StatsClient _client;
        private Dictionary<string, (double sum, int count)> _average = new Dictionary<string, (double sum, int count)>();
        private ConcurrentQueue<(string counter, object value, long ts)> _sendQueue = new ConcurrentQueue<(string counter, object value, long ts)>();
        private Timer _sendTimer;
        private readonly string _serverPrefix;

        public DefaultStatisticsClient(StatsClient client)
        {
            _serverPrefix = AppSettings.Default.ServerId.Replace('.', '_');
            _client = client;
            if(BatchingEnabled)
                _sendTimer = new Timer(Send, null, SendTimeout, SendTimeout);
        }

        private void Send(object s)
        {
            var ts = TimeUtils.ConvertToUnixTimestamp(DateTime.UtcNow);
            lock (_average)
            {
                var ms = _average.Select(_ => ($"{_.Key}.avg", _.Value.sum / _.Value.count, ts));
                var msc = _average.Select(_ => ($"{_.Key}.count", _.Value.count, ts));
                foreach (var tuple in ms)
                {
                    _sendQueue.Enqueue(tuple);
                }

                foreach (var tuple in msc)
                {
                    _sendQueue.Enqueue(tuple);
                }
                _average.Clear();
            }

            while (_sendQueue.Count > 0)
            {
                if(!_sendQueue.TryDequeue(out var m))
                    continue;

                _client.SendStat(m.counter, m.value, m.ts);
            }
        }

        public void Send(string category, string counter, object value, bool calcAvg = false)
        {
            if (string.IsNullOrEmpty(category))
                category = "none";
            var fullName = $"{_serverPrefix}.{category}.{counter}";
            
            if (BatchingEnabled)
            {
                if (calcAvg)
                {
                    lock (_average)
                    {
                        try
                        {
                            var val = Convert.ToDouble(value);
                            _average.TryGetValue(fullName, out var p);
                            p.sum += val;
                            ++p.count;
                            _average[fullName] = p;
                        }
                        catch
                        { }
                    }
                }
                else
                {
                    _sendQueue.Enqueue((fullName, value, TimeUtils.ConvertToUnixTimestamp(DateTime.UtcNow)));
                }
            }
            else
            {
                _client.SendStat(fullName, value);
            }
        }

        public void SendAsync(string category, string counter, object value, bool calcAvg = false)
        {
            Task.Run(() => Send(category, counter, value, calcAvg)).ReportOnFail();
        }

        public void Dispose()
        {
            _sendTimer?.Dispose();
        }
    }
}