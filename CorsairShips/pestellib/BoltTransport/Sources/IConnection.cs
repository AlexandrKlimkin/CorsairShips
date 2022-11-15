using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MasterServerProtocol;

namespace BoltTransport
{
    public interface IConnection
    {
        Task MainLoop(Stream stream);
        void SendMessage(Message message);
        Task<T> SendMessageAsync<T>(Message message) where T : Message;
        IPAddress RemoteIP { get; set; }
        CancellationToken CancellationToken { get; set; }
    }
}