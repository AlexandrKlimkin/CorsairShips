using System;
using System.Collections.Generic;
using System.Linq;
using FriendsClient.Private;
using FriendsClient.Sources;
using log4net;
using UnityDI;

namespace FriendsClient.Lobby.Concrete
{
    public class Lobby : ILobby, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Lobby));
        private IFriendsClientPrivate _client;
        private FriendsRoom _room;
        private Dictionary<long, FriendsRoom> _roomCache = new Dictionary<long, FriendsRoom>();
        private bool _dontClose;
#pragma warning disable 649
        [Dependency]
        private IExternalFriendsStatusWatch _externalFriendsStatusWatch;
#pragma warning restore 649
        public bool MuteBattleInvitations {
            get { return _client.MuteBattleInvitations; }
            set { _client.MuteBattleInvitations = value; }
        }
        public event Action<IIncomingRoomInvite> OnRoomInvite;
        public event Action<IFriendsRoom> OnJoinRoom;
        public event Action<IFriendsRoom> OnRoomCreated;
        public IFriendsRoom Room
        {
            get { return _room; }
        }

        private void ResetEvents()
        {
            OnRoomInvite = inv => { };
            OnJoinRoom = room => { };
            OnRoomCreated = r => { };
        }

        public void Reinit(bool dontCloseRoom)
        {
            _dontClose = dontCloseRoom;
            if(_room == null) return;
            _room.Leave();
            _room = null;
        }

        public Lobby(IFriendsClientPrivate client, bool dontCloseRoom)
        {
            ContainerHolder.Container.BuildUp(this);
            ResetEvents();
            _dontClose = dontCloseRoom;
            _client = client;
            _client.OnRoomInvite += _roomInvite;
            _client.OnRoomJoin += _roomJoin;
            _client.OnRoomInfo += _roomInfo;
        }

        public void Dispose()
        {
            ResetEvents();
            _close();
        }

        public void CreateRoom(TimeSpan autostartDelay, string gameSpecificData, Action<RoomResult, IFriendsRoom> callback)
        {
            _client.CreateRoom(autostartDelay, gameSpecificData, (id, limit, result) =>
            {
                FriendsRoom room = null;
                if (result == RoomResult.Success)
                    _room = room =new FriendsRoom(_client, id, _client.Id, autostartDelay, limit, gameSpecificData, _dontClose);
                if (callback != null)
                    callback(result, room);
                OnRoomCreated(_room);
            });
        }

        public IFriendsRoom GetRoom(long roomId)
        {
            return GetRoomInternal(roomId);
        }

        private FriendsRoom GetRoomInternal(long roomId, bool cleanCache = false)
        {
            if (_roomCache.TryGetValue(roomId, out var room) && cleanCache)
            {
                _roomCache.Remove(roomId);
            }
            return room;
        }

        private void _roomInfo(RoomInfoEventMessage evt)
        {
            if (_room != null && _room.RoomId == evt.RoomId && _room.RoomStatus == RoomStatus.Party)
                return;

            // in case server sent room updates for users having pending invites
            var room = new FriendsRoom(_client, evt, _dontClose);
            _roomCache[evt.RoomId] = room;
        }

        private void _close()
        {
            _client.OnRoomInvite -= _roomInvite;
            _client.OnRoomJoin -= _roomJoin;
            _client.OnRoomInfo -= _roomInfo;
        }

        private void _roomJoin(RoomJoinEventMessage evt)
        {
            // we need only self join event here
            if(evt.Friend.Id != _client.Id) return;
            // get room from cache (if ServerConfig.RoomUpdatesForInvited == true) or create partially inited room (wait for OnRoomInfo for party info)
            var room = GetRoomInternal(evt.RoomId, true) ?? new FriendsRoom(_client, evt.RoomId, _dontClose);
            if (_room != null)
            {
                _room.OnRoomInfo -= _roomInfo;
                if(room.RoomId != _room.RoomId)
                    _room.Leave();
            }
            _room = room;
            _room.OnRoomInfo += _roomInfo;
        }

        private void _roomInfo(long roomId)
        {
            if(_room.RoomId != roomId) return;
            OnJoinRoom(_room);
        }

        private void _roomInvite(RoomInviteEventMessage evt)
        {
            var friend = _client.GetFriends().FirstOrDefault(_ => _.Id == evt.From);
            if (friend == null && _externalFriendsStatusWatch != null)
                friend = _externalFriendsStatusWatch.GetWatchedPlayers().FirstOrDefault(_ => _.Id == evt.From);

            var invite = new IncomingRoomInvite(_client, evt.RoomId, evt.Expiry, friend);
            if (_client.Status != FriendStatus.Online)
            {
                _roomCache.Remove(evt.RoomId);
                invite.Reject(null);
            }
            else
                OnRoomInvite(invite);
        }
    }
}
