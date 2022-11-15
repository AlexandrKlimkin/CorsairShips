using System;
using System.Collections.Generic;
using System.Linq;
using FriendsClient.Private;
using FriendsClient.Sources;
using log4net;
using PestelLib.Utils;
using S;
using UnityDI;

namespace FriendsClient.Lobby.Concrete
{
    public class FriendsRoom : IFriendsRoom
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FriendsRoom));
        private readonly IFriendsClientPrivate _client;
        private FriendsDelegate.FriendsClientCallback _leaveUserCallback;
        private DateTime _battleStartTime;
        private bool _dontClose;
#pragma warning disable 649
        [Dependency]
        private ITimeProvider _time;
        [Dependency]
        private IExternalFriendsStatusWatch _externalFriendsStatusWatch;
#pragma warning restore 649

        public int PartyLimit { get; private set; }
        public event Action<IFriendsRoom> OnImHost = (c) => { };

        public event Action<IFriendsRoom, FriendBase> OnJoined = (c,f) => { };
        public event Action<IFriendsRoom, MadId> OnLeave = (c, id) => { };
        public event Action<IFriendsRoom, MadId> OnKick = (c, id) => { };
        public event Action<IFriendsRoom> OnCanInviteChanged = (c) => { };
        public event Action<IFriendsRoom> OnPartyChanged = (c) => { };
        public event Action<IFriendsRoom, FriendStatusChangedMessage> OnFriendStatus = (c, evt) => { };
        public event Action<long> OnRoomInfo = r => {};
        public event Action<IFriendsRoom, TimeSpan> OnRoomCountdown = (c,t) => { };
        public event Action<IFriendsRoom> OnStartBattle = (c) => { };
        public event Action<IFriendsRoom> OnBattleAlreadyStarted = c => { };
        public event Action<IFriendsRoom, string> OnGameData = (c,s) => { };
        public event Action<IFriendsRoom> OnClosed = (c) => { };

        private string _gameSpecificData;
        public string GameSpecificData {
            get { return _gameSpecificData; }
            set { UpdateGameData(value);_gameSpecificData = value; }
        }
        public long RoomId { get; private set; }
        public RoomStatus RoomStatus { get; private set; }
        public bool ImHost { get; private set; }

        public TimeSpan BattleCountdown
        {
            get
            {
                var delta = _battleStartTime - _time.Now;
                if(delta < TimeSpan.Zero)
                    return TimeSpan.Zero;
                return delta;
            }
        }

        public IReadOnlyList<InvitedFriend> Party { get { return _party; } }
        public IReadOnlyList<InviteableFriend> CanInvite { get { return _canInvite; } }

        public FriendsRoom(IFriendsClientPrivate client, long roomId, bool dontClose)
        :this(client, roomId, new MadId(0), TimeSpan.FromSeconds(5), 0, dontClose: dontClose)
        {
        }

        public FriendsRoom(IFriendsClientPrivate client, RoomInfoEventMessage evt, bool dontClose)
            :this(client, evt.RoomId, evt.Host, TimeSpan.Zero, evt.PartyLimit, evt.GameSpecificData, dontClose)
        {
            _battleStartTime = evt.Timeout;
            _updateRoomInfo(evt);
        }

        public FriendsRoom(IFriendsClientPrivate client, long roomId, MadId owner, TimeSpan battleTimeout, int partyLimit, string gameSpecificData = null, bool dontClose = false)
        {
            ContainerHolder.Container.BuildUp(this);
            _dontClose = dontClose;
            _gameSpecificData = gameSpecificData;
            _client = client;
            RoomId = roomId;
            _battleStartTime = _time.Now + battleTimeout;
            _client.OnRoomAccept += _accept;
            _client.OnRoomJoin += _joined;
            _client.OnRoomLeave += _leave;
            _client.OnRoomKick += _kick;
            _client.OnFriendStatus += _friendStatusChanged;
            _client.OnRoomInfo += _roomInfo;
            _client.OnRoomStartBattle += _startBattle;
            _client.OnRoomNewHost += _newHost;
            _client.OnRoomCountdown += _roomCountdown;
            _client.OnRoomGameData += _roomGameData;
            _client.OnFriendRemoved += _friendRemoved;
            _client.FriendList.OnNewFriend += _friendAdded;
            ImHost = _client.Id == owner;
            PartyLimit = partyLimit;
            if (ImHost)
            {
                var selft = new InvitedFriend(client, this, client.FriendList.Me);
                _party.Add(selft);
            }

            InitCanInvite();
        }

        private List<InvitedFriend> _party = new List<InvitedFriend>();
        private List<InviteableFriend> _canInvite = new List<InviteableFriend>();

        
        public void Leave(FriendsDelegate.FriendsClientCallback callback = null)
        {
            if (RoomStatus != RoomStatus.Party) return;
            RoomStatus = RoomStatus.Left;
            _close();
            _leaveUserCallback = callback;
            if(Party.Any(_ => _.FriendInfo.Id == _client.Id))
                _client.LeaveRoom(RoomId, _leaveCallback);
        }

        public void StartBattle(FriendsDelegate.StartBattleCallback callback)
        {
            if(RoomStatus != RoomStatus.Party) return;
            if (!ImHost)
            {
                if (callback != null)
                    callback(RoomResult.NotAllowed);
                return;
            }
            _client.StartBattle(RoomId, GameSpecificData, callback);
            if (!_dontClose)
            {
                RoomStatus = RoomStatus.Battle;
                _close();
            }
        }

        public void Close()
        {
            Leave();
        }

        private void UpdateGameData(string data)
        {
            if(!ImHost) return;
            if(data == _gameSpecificData) return;
            _client.RoomUpdate(RoomId, data);
        }

        private void _close()
        {
            foreach (var f in _canInvite) f.Close();
            foreach (var f in _party) f.Close();
            _client.OnRoomAccept -= _accept;
            _client.OnRoomJoin -= _joined;
            _client.OnRoomLeave -= _leave;
            _client.OnRoomKick -= _kick;
            _client.OnFriendStatus -= _friendStatusChanged;
            _client.OnRoomInfo -= _roomInfo;
            _client.OnRoomStartBattle -= _startBattle;
            _client.OnRoomNewHost -= _newHost;
            _client.OnRoomCountdown -= _roomCountdown;
            _client.OnRoomGameData -= _roomGameData;
            _client.OnFriendRemoved -= _friendRemoved;
            _client.FriendList.OnNewFriend -= _friendAdded;
            OnClosed(this);
        }

        private void InitCanInvite()
        {
            if(!ImHost) return;
            if(_canInvite.Count != 0) return;

            var version = _client.FriendList.Me.Profile?.Version;
            var friends = _client.GetFriends();
            if (_externalFriendsStatusWatch != null && _client.RoomInviteAnybody)
                friends = friends.Union(_externalFriendsStatusWatch.GetWatchedPlayers()).Distinct(FriendBaseComparer.Instance).ToArray();
            foreach (var friendBase in friends)
            {
                if(_party.All(_ => _.FriendInfo.Id != friendBase.Id))
                    _canInvite.Add(new InviteableFriend(_client, RoomId, friendBase, friendBase.Profile?.Version != version));
            }
        }

        private void _roomGameData(RoomGameDataMessage evt)
        {
            if (ImHost) return;
            if (RoomId != evt.RoomId) return;
            GameSpecificData = evt.GameSpecificId;
            OnGameData(this, GameSpecificData);
        }

        private void _roomCountdown(RoomCountdownEventMessage evt)
        {
            if (evt.RoomId != RoomId) return;
            _battleStartTime = evt.Timeout;
            var delta = evt.Timeout - _time.Now;
            OnRoomCountdown(this, delta);
        }

        private void _newHost(RoomChangeHostEventMessage evt)
        {
            if (RoomStatus != RoomStatus.Party) return;
            if (evt.RoomId != RoomId) return;

            ImHost = evt.NewHost == _client.Id;
            _battleStartTime = evt.Timeout;
            if (ImHost)
            {
                InitCanInvite();
                OnImHost(this);
            }
        }

        private void _startBattle(RoomInfoEventMessage evt)
        {
            if (evt.RoomId != RoomId) return;
            if (!_dontClose)
                RoomStatus = RoomStatus.Battle;
            _updateRoomInfo(evt);
            if (evt.Timeout < _time.Now || !_time.IsSynced)
                OnBattleAlreadyStarted(this);
            else
                OnStartBattle(this);
            if (!_dontClose)
                _close();
        }

        private void _updateRoomInfo(RoomInfoEventMessage evt)
        {
            if(evt.Host != 0)
                ImHost = evt.Host == _client.Id;
            if(evt.PartyLimit > 0)
                PartyLimit = evt.PartyLimit;
            if (evt.Party.Length > 0)
            {
                foreach (var f in _party) f.Close();
                _party.Clear();
                foreach (var friend in evt.Party)
                {
                    _party.Add(new InvitedFriend(_client, this, friend));
                }
            }

            if(evt.Timeout != default(DateTime))
                _battleStartTime = evt.Timeout - _client.BattleStartDecisionDelay;

            if(!string.IsNullOrEmpty(evt.GameSpecificData))
                GameSpecificData = evt.GameSpecificData;

            OnPartyChanged(this);
        }

        private void _roomInfo(RoomInfoEventMessage evt)
        {
            if(evt.RoomId != RoomId) return;

            _updateRoomInfo(evt);
            OnRoomInfo(RoomId);
        }

        private void _leaveCallback(bool result)
        {
            if (_leaveUserCallback == null)
                return;
            try
            {
                _leaveUserCallback(result);
            }
            catch (Exception e)
            {
                Log.Error("User callback failed.", e);
            }
        }

        private void _accept(RoomInviteEventMessage roomInviteEventMessage)
        {
            if (roomInviteEventMessage.RoomId != RoomId)
                return;

            var canInvite = _canInvite.Find(_ => _.FriendInfo.Id == roomInviteEventMessage.From);
            if (canInvite != null) // if null when friend deleted after room was created
            {
                canInvite.Close();
                _canInvite.Remove(canInvite);
            }

            var friendInfo = canInvite?.FriendInfo ??
                             _client.FriendList.FindFriendOrNull(roomInviteEventMessage.From)?.FriendInfo;
            if (friendInfo == null) throw null; // must be impossible, dev time assert
            _party.Add(new InvitedFriend(_client, this, friendInfo));
            OnJoined(this, friendInfo);
            OnPartyChanged(this);
        }

        private void _joined(RoomJoinEventMessage evt)
        {
            if (evt.RoomId != RoomId)
                return;
            var idx = _canInvite.FindIndex(_ => _.FriendInfo.Id == evt.Friend.Id);
            if (idx < 0)
            {
                if (_party.Any(_ => _.FriendInfo.Id == evt.Friend.Id))
                    return;
                if (ImHost)
                    Log.WarnFormat("Unexpected user '{0}' joined room '{1}'.", evt.Friend.Id.ToString(), RoomId);
            }
            else  if (ImHost) _canInvite.RemoveAt(idx);
            _party.Add(new InvitedFriend(_client, this, evt.Friend));
            _battleStartTime = evt.Timeout;
            OnJoined(this, evt.Friend);
            OnPartyChanged(this);
        }

        private void _leave(RoomLeaveKickEventMessage evt)
        {
            if (RoomId != evt.RoomId)
                return;

            _moveToInvitable(evt.FriendId);
            OnLeave(this, evt.FriendId);
        }

        private void _kick(RoomLeaveKickEventMessage evt)
        {
            if (RoomId != evt.RoomId)
                return;
            if (evt.FriendId == _client.Id)
            {
                _close();
                RoomStatus = RoomStatus.Left;
                OnKick(this, _client.Id);
            }

            _moveToInvitable(evt.FriendId);
            OnKick(this, evt.FriendId);
        }

        private void _friendAdded(IFriendContext ctx)
        {
            if (!ImHost)
                return;

            var version = _client.FriendList.Me.Profile?.Version;
            var friendBase = ctx.FriendInfo;
            if (_party.All(_ => _.FriendInfo.Id != ctx.FriendInfo.Id))
            {
                _canInvite.Add(new InviteableFriend(_client, RoomId, friendBase, friendBase.Profile?.Version != version));
                OnCanInviteChanged(this);
            }
        }

        private void _friendRemoved(FriendEventMessage evt)
        {
            var count = _canInvite.RemoveAll(_ => _.FriendInfo.Id == evt.From);
            if (count > 0)
                OnCanInviteChanged(this);
        }

        private bool _isMovableToInviteable(MadId id)
        {
            return _client.IsMyFriend(id) || (_externalFriendsStatusWatch?.IsWatchedPlayer(id) ?? false);
        }

        private void _moveToInvitable(MadId id)
        {
            var idx = _party.FindIndex(_ => _.FriendInfo.Id == id);
            if(idx < 0) return; // not in room
            var friend = _party[idx];
            friend.Close();
            _party.RemoveAt(idx);
            OnPartyChanged(this);
            if (ImHost && _isMovableToInviteable(id))
            {
                var version = _client.FriendList.Me.Profile?.Version;
                var fb = (FriendBase) friend;
                _canInvite.Add(new InviteableFriend(_client, RoomId, fb, fb.Profile?.Version != version));
                OnCanInviteChanged(this);
            }
        }

        private void _friendStatusChanged(FriendStatusChangedMessage evt)
        {
            FriendBase friend = _party.FirstOrDefault(_ => _.FriendInfo.Id == evt.From);
            if (friend == null)
            {
                var t = _canInvite.FirstOrDefault(_ => _.FriendInfo.Id == evt.From);
                if (t == null)
                    return;
                friend = t.FriendInfo;
            }
            if (evt.StatusCode == FriendStatus.Offline)
                _moveToInvitable(evt.From);
            OnFriendStatus(this, evt);
        }
    }
}