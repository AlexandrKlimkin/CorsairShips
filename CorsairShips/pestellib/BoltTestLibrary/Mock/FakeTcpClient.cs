using System.IO;
using System.Net;
using BoltTransport;

namespace BoltTestLibrary.Mock
{
    public class FakeTcpClient : ITcpClient
    {
        private FakeNetworkStream stream = new FakeNetworkStream();

        public IPAddress RemoteIP => IPAddress.Parse("127.0.0.1");

        public Stream GetStream()
        {
            return stream;
        }

        public Stream GetRemoteStream()
        {
            return ((FakeNetworkStream)GetStream()).RemotePCStream;
        }

        public void Dispose()
        {
        }
    }
}
