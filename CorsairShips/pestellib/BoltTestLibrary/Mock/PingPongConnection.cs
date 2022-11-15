using System.Threading.Tasks;
using BoltTransport;
using MasterServerProtocol;

namespace BoltTestLibrary.Mock
{
    public class PingPongConnection : Connection
    {
        protected override Task ProcessMessage(Message message)
        {
            if (message is Ping msg)
            {
                SendMessage(new Pong { MessageId = msg.MessageId });
            } 
            return base.ProcessMessage(message);
        }

        public Task<Pong> Ping()
        {
            return SendMessageAsync<Pong>(new Ping());
        }
    }
}