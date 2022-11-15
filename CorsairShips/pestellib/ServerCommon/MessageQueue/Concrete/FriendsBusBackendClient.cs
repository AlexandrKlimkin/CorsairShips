using System;
using PestelLib.ServerCommon.MessageQueue;
using ServerShared.PlayerProfile;
using MessagePack;
using log4net;

namespace FriendsServer.Bus
{
    public class FriendsBusBackendClient
    {
        public bool Enabled { get; }

        public FriendsBusBackendClient(string connectionString)
        {
            Enabled = false;
            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    _broadcastQueue = MessageQueueFactory.Instance.CreateBroadcastQueue(connectionString, QueueName.Friends);
                    _workerQueue = MessageQueueFactory.Instance.CreateWorkerQueuePublisher(connectionString, QueueName.FriendsWorker);
                    MessageQueueFactory.Instance.CreateWorkerQueuePublisher(connectionString, QueueName.Friends);
                    Enabled = true;
                }
                catch (Exception e)
                {
                    Log.Warn("Can't create broadcast queue." + e);
                }
            }
        }

        private void Notify(int type, byte[] data)
        {
            data = CreateMessage(type, data);
            _broadcastQueue.SendMessage(data);
        }

        public void NotifyProfileChange(ProfileDTO evt)
        {
            if (!Enabled)
                return;
            var data = MessagePackSerializer.Serialize(evt);
            Notify((int)BusMessageType.ProfileUpdate, data);
        }

        public void NotifyPlayerRemoved(Guid playerId)
        {
            var data = MessagePackSerializer.Serialize(playerId);
            data = CreateMessage((int)BusMessageType.PlayerDelete, data);
            _workerQueue.SendWork(data);
        }

        private byte[] CreateMessage(int type, byte[] data)
        {
            var message = new FriendsBusMessage()
            {
                Type = type,
                Data = data
            };
            data = MessagePackSerializer.Serialize(message);
            return data;
        }

        private readonly IBroadcastQueue _broadcastQueue;
        private readonly IWorkerQueue _workerQueue;
        private static readonly ILog Log = LogManager.GetLogger(typeof(FriendsBusBackendClient));
    }
}
