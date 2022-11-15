using BoltTransport;
using MasterServerProtocol;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Sockets;

namespace BoltGameServerToMasterServerConnector
{
    public class LoadBalancingConnectionImplementation : Connection
    {
        private ConcurrentQueue<Message> _messages;

        public LoadBalancingConnectionImplementation()
        {
            throw new NotImplementedException();
        }

        public LoadBalancingConnectionImplementation(ConcurrentQueue<Message> messages)
        {
            _messages = messages;
        }

        //do nothing, process Inbox queue in external code
        protected override async Task ProcessMessage(Message message)
        {
            _messages.Enqueue(message);
            await Task.CompletedTask;
        }
    }
}