using System;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using log4net;

namespace ServerShared
{
    public class GraphiteClient : StatsClient, IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private static ILog Log = LogManager.GetLogger(typeof(GraphiteClient));
        private readonly LimitedQueue<string> _messageQueue =  new LimitedQueue<string>(50000);
        private TcpClient _client;
        private volatile bool connecting;
        private object _sync = new object();
        private DateTime _retryTime = DateTime.MinValue;

        public GraphiteClient(string host, int port)
        {
            _host = host;
            _port = port;

            ValidateConnection();
        }

        public override void SendStat(string counter, object value, long ts)
        {
            string data;
            if (value is float || value is double)
            {
                var valueString = Convert.ToDouble(value).ToString(CultureInfo.InvariantCulture);
                data = string.Format("{0} {1} {2}", counter, valueString, ts);
            }
            else
                data = string.Format("{0} {1} {2}", counter, value, ts);
            if (_client == null)
            {
                Log.WarnFormat("Can't send stat {0}. Client disposed", data);
                return;
            }
            if (!ValidateConnection())
            {
                lock (_messageQueue)
                {
                    _messageQueue.Enqueue(data);
                }
                return;
            }

            try
            {
                var s = new StreamWriter(_client.GetStream());

                lock (_messageQueue)
                {
                    while (_messageQueue.Any())
                    {
                        var pendingData = _messageQueue.Peek();
                        s.WriteLine(pendingData);
                        _messageQueue.Dequeue();
                    }
                }

                s.WriteLine(data);
                s.Flush();
            }
            catch (Exception e)
            {
                Log.WarnFormat("Can't send stat. Error: " + e);
                lock (_messageQueue)
                {
                    _messageQueue.Enqueue(data);
                }
            }
        }

        private bool ValidateConnection()
        {
            if (_client != null && _client.Connected) return true;
            if (!connecting && _retryTime <= DateTime.UtcNow)
            {
                lock (_sync)
                {
                    if (connecting || _retryTime > DateTime.UtcNow) return false;
                    connecting = true;
                    _retryTime = DateTime.UtcNow.AddSeconds(10);
                    if (_client != null)
                    {
                        try
                        {
                            _client.Close();
                        }
                        catch {}
                    }
                    _client = new TcpClient();
                    ThreadPool.QueueUserWorkItem(state => Connect());
                }
            }

            return false;
        }

        private void Connect()
        {
            try
            {
                _client.Connect(_host, _port);
            }
            catch (Exception e)
            {
                Log.Warn(_host + ":" + _port + " connection error: " + e);
            }
            finally
            {
                lock (_sync)
                    connecting = false;
            }
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }
    }
}
