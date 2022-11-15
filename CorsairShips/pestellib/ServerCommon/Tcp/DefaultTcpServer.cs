using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using log4net;

namespace PestelLib.ServerCommon.Tcp
{
    public class DefaultTcpServer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DefaultTcpServer));
        private TcpListener _listener;
        private volatile bool _disposed;

        public DefaultTcpServer(int port)
        {
            _listener = TcpListener.Create(port);
            _listener.Server.NoDelay = true;
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
        }

        protected virtual void OnNewClient(TcpClient client)
        {
            
        }

        public void Start()
        {
            Log.Debug($"Start listening at {_listener.LocalEndpoint}");
            _listener.Start(100);
            FetchClient();
        }

        public void Stop()
        {
            _listener.Stop();
        }

        private void FetchClient()
        {
            _listener.AcceptTcpClientAsync().ContinueWith(WrapErrors);
        }

        private void WrapErrors(Task<TcpClient> t)
        {
            /*
            uint dummy = 0;
            byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)1000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
            BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);*/

            try
            {
                var r = t.Result;
                r.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                //r.Client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
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
                if(t.IsCompleted)
                    t.Result.Client.Close();
                Log.WarnFormat($"Failed to fetch new client. Error: {msg}");
            }
            finally
            {
                if (!_disposed)
                    FetchClient();
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _listener.Stop();
        }
    }
}
