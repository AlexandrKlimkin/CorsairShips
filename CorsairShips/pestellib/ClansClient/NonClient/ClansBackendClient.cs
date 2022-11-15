using System;
using MessagePack;
using PestelLib.ServerCommon.MessageQueue;

namespace ClansClientLib
{
    public class ClansBackendClient
    {
        public ClansBackendClient(string mqConnectionString, string appId)
        {
            _queue = MessageQueueFactory.Instance.CreateWorkerQueuePublisher(mqConnectionString, appId);
        }

        public void NotifyUserStateDelete(Guid userId)
        {
            var msg = new ClansBackendMessage()
            {
                Type = ClansBackendMessageType.UserDelete,
                Data = userId.ToByteArray()
            };
            var msgRaw = MessagePackSerializer.Serialize(msg);
            _queue.SendWork(msgRaw);
        }

        public void NotifyUserBanned(Guid userId)
        {
            var msg = new ClansBackendMessage()
            {
                Type = ClansBackendMessageType.UserBanned,
                Data = userId.ToByteArray()
            };
            var msgRaw = MessagePackSerializer.Serialize(msg);
            _queue.SendWork(msgRaw);
        }

        private IWorkerQueue _queue;
    }
}
