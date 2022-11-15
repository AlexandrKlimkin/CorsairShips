using System;
using FriendsClient.Private;
using PestelLib.Utils;
using UnityDI;

namespace FriendsClient.Lobby.Concrete
{
    class IncomingRoomInvite : IIncomingRoomInvite
    {
        private IFriendsClientPrivate _client;
        private IFriendsRoom _room;
#pragma warning disable 649
        [Dependency]
        private ITimeProvider _time;
#pragma warning restore 649
        private bool _expiredByServer;
        public long RoomId { get; private set; }
        public DateTime Timeout { get; }
        public RoomResult SendResult { get; private set; }
        public FriendBase FriendInfo { get; private set; }
        public bool Expired => _expiredByServer || Timeout < _time.Now;

        public IFriendsRoom Room {
            get
            {
                if (_room == null)
                    return _room = _client.Lobby.GetRoom(RoomId);
                return _room;
            }
        }

        public event Action<IIncomingRoomInvite> OnExpired = (_) => { };

        public IncomingRoomInvite(IFriendsClientPrivate client, long roomRoomId, DateTime expiry, FriendBase friend)
        {
            ContainerHolder.Container.BuildUp(this);
            _client = client;
            RoomId = roomRoomId;
            _room = client.Lobby.GetRoom(roomRoomId);
            Timeout = expiry;
            FriendInfo = friend;
            _client.OnRoomAutoReject += _expired;
        }

        public void Close()
        {
            _client.OnRoomAutoReject -= _expired;
        }

        private void _expired(RoomInviteEventMessage roomInviteEventMessage)
        {
            if(roomInviteEventMessage.RoomId != RoomId || roomInviteEventMessage.From != FriendInfo.Id)
                return;

            _expiredByServer = true;
            Close();
            OnExpired(this);
        }

        public void Accept(FriendsDelegate.RoomAnswerCallback callback)
        {
            if(SendResult != RoomResult.None) return;
            _client.InviteRoomAnswer(RoomId, true, (id, result) => _callback(id, result, callback));
        }

        public void Reject(FriendsDelegate.RoomAnswerCallback callback)
        {
            if (SendResult != RoomResult.None) return;
            _client.InviteRoomAnswer(RoomId, false, (id, result) => _callback(id, result, callback));
            _expiredByServer = true;
        }

        private void _callback(long roomId, RoomResult result, FriendsDelegate.RoomAnswerCallback callback)
        {
            SendResult = result;
            if (callback != null)
                callback(roomId, result);
            Close();
        }
    }
}
