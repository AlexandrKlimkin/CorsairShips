using System;
using System.Net.Sockets;
using System.Threading;
using log4net;

namespace PestelLib.MatchmakerShared
{
    public class TcpMatchmakerClient : MatchmakerMessageProcessor
    {
        private TcpClient _client;

        public TcpMatchmakerClient(string host, int port)
        {
            if (!TryIpv4(host, port))
            {
                _client.Close();
                _client = new TcpClient(AddressFamily.InterNetworkV6);
                _client.Connect(host, port);
            }

            _client.Client.NoDelay = true;
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            AddSource(new MatchmakerMessageStream(_client.GetStream()));
        }

        private bool TryIpv4(string host, int port)
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(host, port);
            }
            catch (SocketException)
            {
                return false;
            }

            return true;
        }
    }

    public class AsyncTcpMatchmakerClient : MatchmakerMessageProcessor
    {
        private static ILog Log = LogManager.GetLogger(typeof(AsyncTcpMatchmakerClient));
        private TcpClient _client;
        private string _host;
        private int _port;

        public AsyncTcpMatchmakerClient(string host, int port)
        {
            _host = host;
            _port = port;
            _client = new TcpClient();
            _client.BeginConnect(host, port, ConnectResult, 5);
        }

        void ConnectResult(IAsyncResult ar)
        {
            var retry = (int)ar.AsyncState;
            try
            {
                _client.EndConnect(ar);
            }
            catch (SocketException e)
            {
                if (retry == 0)
                    throw;
                Log.Warn(retry + ": Connect error", e);

                _client.Close();
                if (retry % 2 == 0)
                {
                    _client = new TcpClient(AddressFamily.InterNetwork);
                }
                else
                {
                    _client = new TcpClient(AddressFamily.InterNetworkV6);
                }

                Thread.Sleep(100);
                _client.BeginConnect(_host, _port, ConnectResult, --retry);
                return;
            }
            _client.Client.NoDelay = true;
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            AddSource(new MatchmakerMessageStream(_client));
        }
    }
}
