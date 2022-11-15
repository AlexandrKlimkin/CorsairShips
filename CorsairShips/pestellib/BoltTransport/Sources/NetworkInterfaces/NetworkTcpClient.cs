using System.IO;
using System.Net;
using System.Net.Sockets;

namespace BoltTransport
{
    /// <summary>
    /// Враппер для TcpClient, появился для удобства тестирования - чтобы можно было тестировать
    /// классы, завязанные на сетевое соединение без сети.
    /// </summary>
    public class NetworkTcpClient : ITcpClient
    {
        private TcpClient implementation;

        public NetworkTcpClient(string hostname, int port)
        {
            implementation = new TcpClient(hostname, port);
        }

        public NetworkTcpClient(TcpClient implementation)
        {
            this.implementation = implementation;
        }

        public IPAddress RemoteIP => ((IPEndPoint)implementation.Client.RemoteEndPoint).Address;

        public Stream GetStream()
        {
            return implementation.GetStream();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    implementation.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
