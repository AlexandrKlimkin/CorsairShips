using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FriendsClient.FriendList;
using FriendsClient.FriendList.Concrete;
using FriendsClient.Lobby;
using FriendsClient.Sources;
using MessageServer.Sources;
using PestelLib.Utils;
using PlayerProfile.Sources;
using S;
using ServerShared.PlayerProfile;
using UnityDI;

namespace FriendsClient.Private
{
    public partial class FriendsClient : IFriendsClientPrivate, IFriendsClient, IDisposable
    {
        private readonly Guid _playerId;
        private readonly string _authData;
        public MadId Id { get; private set; }
        public bool Initialized { get; private set; }
        private FriendList.Concrete.FriendList _friendList;
        private Lobby.Concrete.Lobby _lobby;
        private HashSet<long> _shownRoomInvites = new HashSet<long>();
#pragma warning disable 649
        [Dependency]
        private ITimeProvider _time;
        [Dependency]
        private PlayerProfileClient _profileClient;
#pragma warning restore 649
        private TaskCompletionSource<bool> _initAwaiter;

        public bool IsConnected {
            get { return _messageProvider.IsConnected; }
        }

        public bool IsMyFriend(MadId id)
        {
            return FriendList.Friends.Any(_ => _.FriendInfo.Id == id);
        }
        public Task WaitInit()
        {
            return _initAwaiter.Task;
        }
        // Notifications
        public event Action<FriendInitResult> OnInitialized = (r) => { };
        public event Action OnConnected
        {
            add
            {
                if (_messageProvider == null) return;
                _messageProvider.OnConnected += value;
            }
            remove
            {
                if(_messageProvider == null) return;
                _messageProvider.OnConnected -= value;
            }
        }

        public event Action OnConnectionError
        {
            add
            {
                if (_messageProvider == null) return;
                _messageProvider.OnConnectionError += value;
            }
            remove
            {
                if (_messageProvider == null) return;
                _messageProvider.OnConnectionError -= value;
            }
        }
        public event Action OnDisconnected
        {
            add
            {
                if (_messageProvider == null) return;
                _messageProvider.OnDisconnected += value;
            }
            remove
            {
                if (_messageProvider == null) return;
                _messageProvider.OnDisconnected -= value;
            }
        }
        public event Action<FriendsInviteEventMessage> OnInvite = evt => { };
        public event Action<FriendsInviteEventMessage> OnInviteAccepted = evt => { };
        public event Action<FriendsInviteEventMessage> OnInviteRejected = evt => { };
        public event Action<FriendsInviteEventMessage> OnInviteCanceled = evt => { };
        public event Action<FriendsInviteEventMessage> OnInviteExpired = evt => { };
        public event Action<FriendStatusChangedMessage> OnFriendStatus = evt => { };
        public event Action<FriendGiftEventMessage> OnFriendGift = evt => { };
        public event Action<FriendGiftEventMessage> OnFriendGiftClaimed = evt => { };
        public event Action<RoomInviteEventMessage> OnRoomInvite = evt => { };
        public event Action<RoomInviteEventMessage> OnRoomAccept = evt => { };
        public event Action<RoomInviteEventMessage> OnRoomReject = evt => { };
        public event Action<RoomInviteEventMessage> OnRoomAutoReject = evt => { };
        public event Action<RoomJoinEventMessage> OnRoomJoin = evt => { };
        public event Action<RoomLeaveKickEventMessage> OnRoomLeave = evt => { };
        public event Action<RoomLeaveKickEventMessage> OnRoomKick = evt => { };
        public event Action<RoomInfoEventMessage> OnRoomInfo = evt => { };
        public event Action<RoomInfoEventMessage> OnRoomStartBattle = evt => { };
        public event Action<RoomChangeHostEventMessage> OnRoomNewHost = evt => { };
        public event Action<RoomCountdownEventMessage> OnRoomCountdown = evt => { };
        public event Action<RoomGameDataMessage> OnRoomGameData = evt => { };
        public event Action<FriendEventMessage> OnNewFriend = evt => { };
        public event Action<FriendsProfileUpdateMessage> OnProfileUpdate = evt => { };
        /// <summary>
        /// Your friend removed you from his friend-list.
        /// </summary>
        public event Action<FriendEventMessage> OnFriendRemoved = evt => { };

        private bool _muteBattleInvites;
        public bool MuteBattleInvitations
        {
            get { return _muteBattleInvites;}
            set
            {
                _muteBattleInvites = value;
                if (!_muteBattleInvites && _postponedInvites.Count > 0)
                {
                    foreach (var invite in _postponedInvites)
                    {
                        if(_time.Now > invite.Expiry)
                            continue;
                        if(SaveNewInvite(invite))
                            OnRoomInvite(invite);
                    }
                    _postponedInvites.Clear();
                }
            }
        }
        public TimeSpan RoomReinviteCooldown { get; set; }
        public TimeSpan BattleStartDecisionDelay { get; set; }

        public IFriendList FriendList { get { return _friendList; } }
        public ILobby Lobby { get { return _lobby; } }
        public SharedConfig Config { get; private set; }

        public FriendsClient(Guid playerId, string authData)
        {
            _initAwaiter = new TaskCompletionSource<bool>();
            _playerId = playerId;
            _authData = authData;

            ContainerHolder.Container.BuildUp(this);

            RoomReinviteCooldown = TimeSpan.FromSeconds(20);
            BattleStartDecisionDelay = TimeSpan.FromSeconds(15);

            var dispatcher = new FriendsMessageClientDispatcher(this);
            _messageProvider = _factory.CreateMessageProvider();
            _messageSender = _factory.CreateMessageSender(_messageProvider);
            _messageClient = new MessageServer.Sources.MessageClient(_messageProvider, dispatcher);

            _messageProvider.OnConnected += Init;
            _messageProvider.OnDisconnected += Deinit;
            if (_profileClient != null)
                _profileClient.OnProfilesUpdated += ProfilesUpdated;
        }

        private void ProfilesUpdated(ProfileDTO[] profileDtos)
        {
            if (_friendList == null)
                return;
            foreach (var profileDto in profileDtos)
            {
                if (FriendList.Me.Profile.PlayerId == profileDto.PlayerId)
                {
                    OnProfileUpdate(new FriendsProfileUpdateMessage()
                    {
                        Event = FriendEvent.ProfileUpdate,
                        From = FriendList.Me.Id,
                        Profile = profileDto
                    });
                    continue;
                }

                var f = FriendList.FindFriendOrNull(profileDto.PlayerId);
                if(f == null)
                    continue;
                OnProfileUpdate(new FriendsProfileUpdateMessage()
                {
                    Profile = profileDto,
                    Event = FriendEvent.ProfileUpdate,
                    From = f.FriendInfo.Id
                });
            }
        }

        private bool SaveNewInvite(RoomInviteEventMessage evt)
        {
            var key = (long) ((uint) evt.From).GetHashCode() << 32;
            key |= (uint)evt.RoomId.GetHashCode();
            key |= evt.Expiry.Ticks;
            if (_shownRoomInvites.Contains(key))
                return false;
            return _shownRoomInvites.Add(key);
        }

        private void _initDone(FriendInitResponse response)
        {
            if (response.Code == FriendInitResult.InvalidAuth)
            {
                OnInitialized(response.Code);
                return;
            }

            if (response.Code != FriendInitResult.Success)
            {
                throw new NotSupportedException();
            }

            Id = response.Info.Id;
            foreach (var friend in response.InvitedFriends)
            {
                _friendInviteContexts.Add(new FriendInviteContext(this, friend, null));
            }

            if(_friendList != null)
                _friendList.Reinit(response.Info, response.Friends, response.PendingFriendInvites, response.Gifts);
            else
                _friendList = new FriendList.Concrete.FriendList(this, response.Info, response.Friends, response.PendingFriendInvites, response.Gifts);

            Config = response.Config;
            if (_lobby != null)
                _lobby.Reinit(Config.DontCloseRoomOnBattleStart);
            else
                _lobby = new Lobby.Concrete.Lobby(this, Config.DontCloseRoomOnBattleStart);

            if (Config.MadIdMixed)
                MadId.Mode = MadIdMode.Mixed;
            Initialized = true;
            _initAwaiter.SetResult(true);
            OnInitialized(response.Code);
            Status = FriendStatus.Online;
        }

        public void Start()
        {
            var alreadyConnected = _messageProvider.IsConnected;
            _messageProvider.Start();
            if(alreadyConnected) Init();
        }

        public void Stop()
        {
            _messageProvider.Stop();
        }

        #region State

        public FriendBase[] GetFriends()
        {
            return FriendList.Friends.Select(_ => _.FriendInfo).ToArray();
        }

        #endregion

        #region  friend invites

        public IFriendInviteContext CreateInvitaion(MadId friendId, FriendsDelegate.InviteFriendCallback callback)
        {
            var ctx = new FriendInviteContext(this, friendId, callback);
            _friendInviteContexts.RemoveAll(_ => _.FriendId == friendId);
            _friendInviteContexts.Add(ctx);
            return ctx;
        }

        public IFriendInviteContext GetInvitationByFriend(MadId friendId)
        {
            return _friendInviteContexts.FirstOrDefault(_ => _.FriendId == friendId);
        }

        public int RemoveInvitationByFriend(MadId friendId)
        {
            var toRemove = _friendInviteContexts.Where(_ => _.FriendId == friendId).ToArray();
            foreach (var context in toRemove)
            {
                context.Close();
                _friendInviteContexts.Remove(context);
            }

            return toRemove.Length;
        }

        #endregion

        public DateTime GetRoomInviteCooldown(MadId friendId)
        {
            DateTime result;
            _roomReinviteCooldown.TryGetValue(friendId, out result);
            return result;
        }

        private void RoomInviteReject(RoomInviteEventMessage evt)
        {
            if(RoomReinviteCooldown > TimeSpan.Zero)
                _roomReinviteCooldown[evt.From] = _time.Now + RoomReinviteCooldown;
            OnRoomReject(evt);
        }

        public void Dispose()
        {
            _messageProvider.OnConnected -= Init;
            var disp = _messageProvider as IDisposable;
            if(disp != null) disp.Dispose();
            disp = _messageSender as IDisposable;
            if (disp != null) disp.Dispose();
            disp = _messageClient as IDisposable;
            if (disp != null) disp.Dispose();

            if(_friendList != null) _friendList.Dispose();
            if(_lobby != null) _lobby.Dispose();

            _friendList = null;
            _lobby = null;

            foreach (var context in _friendInviteContexts)
            {
                context.Close();
            }
            _friendInviteContexts.Clear();

            _messageProvider = null;
            _messageSender = null;
            _messageClient = null;
        }

        private List<FriendInviteContext> _friendInviteContexts = new List<FriendInviteContext>();
#pragma warning disable 649
        [Dependency]
        private IFriendsClientTransportFactory _factory;
#pragma warning restore 649

        private Dictionary<MadId, DateTime> _roomReinviteCooldown = new Dictionary<MadId, DateTime>();
        private IMessageProviderEvents _messageProvider;
        private IMessageSender _messageSender;
        private MessageServer.Sources.MessageClient _messageClient;
    }
}