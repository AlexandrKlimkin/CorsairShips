using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BoltTransport;
using MasterServerProtocol;

namespace BoltTestLibrary.Mock
{
    public class ConnectionWithMessagesQueue : Connection
    {
        private Guid instanceId = Guid.NewGuid();

        public int OnConnectionEstablishedCounter { get; private set; }
        public int OnConnectionLostCounter { get; private set; }

        public readonly Queue<Message> ReceivedMessages = new Queue<Message>();

        protected override async Task OnConnectionEstablished(Stream stream)
        {
            await base.OnConnectionEstablished(stream);
            OnConnectionEstablishedCounter++;
        }

        protected override async Task OnConnectionLost(Exception reason)
        {
            await base.OnConnectionLost(reason);
            OnConnectionLostCounter++;
        }

        protected override async Task ProcessMessage(Message message)
        {
            await base.ProcessMessage(message);
            ReceivedMessages.Enqueue(message);
        }
    }
}
