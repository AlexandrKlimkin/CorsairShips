using System;
using MessagePack;
using MessageServer.Sources;

namespace MessageClient.Sources
{
    public class IncomingMessageNotifyHandler<NotifyT> : IMessageHandler
    {
        public IncomingMessageNotifyHandler(Action<NotifyT> handler)
        {
            _handler = handler;
        }

        public void Handle(int sender, byte[] data, int tag, IAnswerContext answerContext)
        {
            NotifyT msg = MessagePackSerializer.Deserialize<NotifyT>(data);
            _handler(msg);
        }

        public void Error(int tag)
        {
        }

        private Action<NotifyT> _handler;
    }
}
