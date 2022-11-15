using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using PestelLib.MatchmakerShared;
using log4net;

namespace PestelLib.MatchmakerServer
{
    public class TcpMatchmakerServer : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TcpMatchmakerServer));
        private TcpListener _listener;
        private volatile bool _disposed;

        public MatchmakerMessageProcessor Processor { get; }

        public TcpMatchmakerServer(int port)
        {
            _listener = TcpListener.Create(port);
            _listener.Start(10000);
            _listener.Server.NoDelay = true;
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            Processor = new MatchmakerMessageProcessor();
            FetchClient();
        }

        private void OnNewClient(TcpClient client)
        {
            var stream = new MatchmakerMessageStream(client);
            Processor.AddSource(stream);
        }

        private void FetchClient()
        {
            _listener.AcceptTcpClientAsync().ContinueWith(WrapErrors);
        }

        private void WrapErrors(Task<TcpClient> t)
        {
            try
            {
                var r = t.Result;
                OnNewClient(r);
            }
            catch (Exception e)
            {
                var msg = e.ToString();
                if (e.InnerException is SocketException socketError)
                {
                    if (socketError.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        msg = "Connection reset by remote";
                    }
                    else
                    {
                        msg = $"SocketError: {socketError}. " + msg;
                    }
                }
                Log.WarnFormat($"Failed to fetch new client. Error: {msg}");
            }
            finally
            {
                if(!_disposed)
                    FetchClient();
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _listener.Stop();
            Processor?.Dispose();
        }
    }
}
