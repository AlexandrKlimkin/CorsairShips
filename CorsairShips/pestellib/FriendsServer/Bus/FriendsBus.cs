using System;
using FriendsClient.Private;
using FriendsClient.Sources.Serialization;
using PestelLib.ServerCommon.MessageQueue;
using ServerShared.PlayerProfile;
using log4net;
using System.Threading.Tasks;

namespace FriendsServer.Bus
{
    /// <summary>
    /// Связывает все инстансы друзей между собой прокидывая ивенты между юзерами на разных инстансах
    /// Т.е. теперь можно слать подарки, приглашать в друзья, наблюдать изменения в профиле/статусе игрока 
    /// и это будет ощущаться как будь-то все на одном инстансе сервера
    /// </summary>
    class FriendsBus : IWorker
    {
        public bool Enabled { get; }
        public bool IsAlive => true;

        private FriendsBus(ServerConfig config)
        {
            Enabled = false;
            if (!string.IsNullOrEmpty(config.MessageQueueConnectionString))
            {
                try
                {
                    _broadcastQueue = MessageQueueFactory.Instance.CreateBroadcastQueue(config.MessageQueueConnectionString, QueueName.Friends);
                    _workerQueue = MessageQueueFactory.Instance.CreateWorkerQueueConsumer(config.MessageQueueConnectionString, QueueName.FriendsWorker, this);
                    _broadcastQueue.OnIncomingMessage += OnIncomingMessage;
                    Enabled = true;
                }
                catch
                {
                    Log.Warn("Can't create broadcast queue.");
                }
            }
        }

        private void OnIncomingMessage(byte[] bytes)
        {
            var message = FriendServerSerializer.Deserialize<FriendsBusMessage>(bytes);

            var type = (BusMessageType) message.Type;
            Log.Debug($"Incoming message {type}.");
            switch (type)
            {
                case BusMessageType.StatusChanged:
                    OnStatusChanged(FriendServerSerializer.Deserialize<FriendStatusChangedMessage>(message.Data));
                    break;
                case BusMessageType.FriendInvite:
                    OnFriendInvite(FriendServerSerializer.Deserialize<FriendsInviteEventGlobal>(message.Data));
                    break;
                case BusMessageType.FriendInviteAnswered:
                    OnFriendInviteAnswered(FriendServerSerializer.Deserialize<FriendsInviteEventGlobal>(message.Data));
                    break;
                case BusMessageType.FriendInviteCanceled:
                    OnFriendInviteCanceled(FriendServerSerializer.Deserialize<FriendsInviteEventGlobal>(message.Data));
                    break;
                case BusMessageType.FriendRemove:
                    OnFriendRemoved(FriendServerSerializer.Deserialize<FriendEventMessageGlobal>(message.Data));
                    break;
                case BusMessageType.Gift:
                    OnGiftSent(FriendServerSerializer.Deserialize<FriendGiftEventMessageGlobal>(message.Data));
                    break;
                case BusMessageType.GiftClaim:
                    OnGiftClaim(FriendServerSerializer.Deserialize<FriendGiftEventMessageGlobal>(message.Data));
                    break;
                case BusMessageType.ProfileUpdate:
                    OnProfileUpdate(FriendServerSerializer.Deserialize<ProfileDTO>(message.Data));
                    break;
                case BusMessageType.PlayerDelete:
                    OnPlayerDelete(FriendServerSerializer.Deserialize<Guid>(message.Data));
                    break;
                default:
                    Log.Error($"Invalid message type {type}.");
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Notify(int type, byte[] data)
        {
            var message = new FriendsBusMessage()
            {
                Type = type,
                Data = data
            };
            data = FriendServerSerializer.Serialize(message);
            _broadcastQueue.SendMessage(data);
        }

        public void NotifyStatusChange(FriendStatusChangedMessage evt)
        {
            var data = FriendServerSerializer.Serialize(evt);
            Notify((int)BusMessageType.StatusChanged, data);
        }

        public void NotifyFriendRemoved(FriendEventMessageGlobal evt)
        {
            var data = FriendServerSerializer.Serialize(evt);
            Notify((int)BusMessageType.FriendRemove, data);
        }

        public void NotifyFriendInvite(FriendsInviteEventGlobal evt)
        {
            var data = FriendServerSerializer.Serialize(evt);
            Notify((int)BusMessageType.FriendInvite, data);
        }

        public void NotifyFriendInviteCanceled(FriendsInviteEventGlobal evt)
        {
            var data = FriendServerSerializer.Serialize(evt);
            Notify((int)BusMessageType.FriendInviteCanceled, data);
        }

        public void NotifyFriendInviteAnswered(FriendsInviteEventGlobal evt)
        {
            var data = FriendServerSerializer.Serialize(evt);
            Notify((int)BusMessageType.FriendInviteAnswered, data);
        }

        public void NotifyGiftSent(FriendGiftEventMessageGlobal evt)
        {
            var data = FriendServerSerializer.Serialize(evt);
            Notify((int)BusMessageType.Gift, data);
        }

        public void NotifyGiftClaim(FriendGiftEventMessageGlobal evt)
        {
            var data = FriendServerSerializer.Serialize(evt);
            Notify((int)BusMessageType.GiftClaim, data);
        }

        public Task<bool> ProcessWork(byte[] data)
        {
            OnIncomingMessage(data);
            return Task.FromResult(true);
        }

        public event Action<FriendStatusChangedMessage> OnStatusChanged = _ => { };
        public event Action<FriendEventMessageGlobal> OnFriendRemoved = _ => { };
        public event Action<FriendsInviteEventGlobal> OnFriendInvite = _ => { };
        public event Action<FriendsInviteEventGlobal> OnFriendInviteCanceled = _ => { };
        public event Action<FriendsInviteEventGlobal> OnFriendInviteAnswered = _ => { };
        public event Action<FriendGiftEventMessageGlobal> OnGiftSent = _ => { };
        public event Action<FriendGiftEventMessageGlobal> OnGiftClaim = _ => { };
        public event Action<ProfileDTO> OnProfileUpdate = _ => { };
        public event Action<Guid> OnPlayerDelete = _ => { };

        public static FriendsBus Instance => Inst.Value;

        public bool Alive => IsAlive;

        private readonly IBroadcastQueue _broadcastQueue;
        private readonly IWorkerQueue _workerQueue;

        private static readonly Lazy<FriendsBus> Inst = new Lazy<FriendsBus>(() => new FriendsBus(ServerConfigCache.Get()));
        private static readonly ILog Log = LogManager.GetLogger(typeof(FriendsBus));
    }
}
