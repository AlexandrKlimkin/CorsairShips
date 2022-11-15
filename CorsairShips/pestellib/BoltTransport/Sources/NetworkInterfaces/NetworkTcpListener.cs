using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace BoltTransport
{
    /// <summary>   Враппер для TCPListener, добавлен для удобства тестирования. </summary>
    public class NetworkTcpListener : ITcpListener
    {
        TcpListener implementation;

        public NetworkTcpListener(IPAddress address, int port)
        {
            implementation = new TcpListener(address, port);
        }

        public async Task<ITcpClient> AcceptTcpClientAsync()
        {
            var implementationClient = await implementation.AcceptTcpClientAsync();
            return new NetworkTcpClient(implementationClient);
        }

        public void Start()
        {
            implementation.Start();
        }

        public void Stop()
        {
            implementation.Stop();
        }
    }
}
