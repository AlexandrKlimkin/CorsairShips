using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BoltTransport;

namespace BoltTestLibrary.Mock
{
    public class FakeTcpListener : ITcpListener
    {
        private Queue<FakeTcpClient> clients = new Queue<FakeTcpClient>();

        public async Task<ITcpClient> AcceptTcpClientAsync()
        {
            if (clients.Count == 0)
            {
                await Task.Delay(30);
            }            
            return clients.Dequeue();
        }

        public Stream EmulateClientConnection(out FakeNetworkStream serverStream)
        {
            var client = new FakeTcpClient();
            clients.Enqueue(client);
            serverStream = (FakeNetworkStream)client.GetStream();
            return ((FakeNetworkStream)serverStream).RemotePCStream;
        }
        
        public Stream EmulateClientConnection()
        {
            return EmulateClientConnection(out var serverStream);
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
