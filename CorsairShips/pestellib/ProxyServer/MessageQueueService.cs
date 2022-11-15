using System;
using System.Threading.Tasks;
using MessagePack;
using PestelLib.ServerCommon.MessageQueue;

namespace ProxyServerApp
{
    class MessageQueueService : IService
    {
        public MessageQueueService(IBroadcastQueue queue)
        {
            Queue = queue;
            Queue.OnIncomingMessage += OnIncomingMessage;
        }

        private void OnIncomingMessage(byte[] obj)
        {
            var message = MessagePackSerializer.Deserialize<Message>(obj);
            OnAnswer(message.Sender, message.Data);
        }

        public event Action<int, byte[]> OnAnswer = (i, bytes) => { };

        public Task<bool> Process(int sender, byte[] data)
        {
            var message = new Message();
            message.Sender = sender;
            message.Data = data;
            var packedData = MessagePackSerializer.Serialize(message);
            Queue.SendMessage(packedData);
            return Task.FromResult(true);
        }

        private IBroadcastQueue Queue;

        [MessagePackObject()]
        struct Message
        {
            [Key(0)]
            public int Sender;
            [Key(1)]
            public byte[] Data;
        }
    }
}