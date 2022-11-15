using System;
using FriendsClient.FriendList;
using FriendsClient.Lobby.Concrete;
using FriendsClient.Private;
using FriendsClient.Sources;
using log4net;
using PestelLib.Utils;
using ServerShared;
using  UnityDI;

namespace FriendsClient.Lobby
{
    public class InviteableFriend
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InviteableFriend));
        private readonly IFriendsClientPrivate _client;
        public bool VersionMissmatch { get; private set; }
        private FriendsDelegate.RoomAnswerCallback _userCallback;
#pragma warning disable 649
        [Dependency]
        private ITimeProvider _time;
#pragma warning restore 649
        /// <summary>
        /// Information about a friend.
        /// </summary>
        public FriendBase FriendInfo { get; private set; }
        /// <summary>
        /// Room id of <see cref="FriendsRoom"/> which had created this object.
        /// </summary>
        public long RoomId { get; private set; }
        /// <summary>
        /// Your friend's answer on your invitation (see <see cref="InviteStatus"> for details).
        /// Makes sense when InviteResult == Success. Otherwise it's an error, see <see cref="InviteResult"/> for details. 
        /// </summary>
        public InviteStatus InviteStatus { get; private set; }
        /// <summary>
        /// Invitation request status (that was passed to <see cref="Invite"/>'s callback argument).
        /// </summary>
        public RoomResult InviteResult { get; private set; }
        /// <summary>
        /// Check if you able to invite that friend. e.g. you cant invite offline friends.
        /// </summary>
        public bool CanInvite {
            get
            {
                if (VersionMissmatch)
                    return false;
                if (FriendInfo == null)
                    return false;
                if ((InviteStatus == InviteStatus.Rejected || InviteStatus == InviteStatus.AutoRejected) && HasInviteCooldown)
                    return false;
                if (InviteStatus == InviteStatus.Accepted || InviteStatus == InviteStatus.Pending)
                    return false;
                var friendStatus = FriendInfo.Status == FriendStatus.Online;
                return friendStatus;
            }}

        /// <summary>
        /// Your invitation is accepted.
        /// </summary>
        public event Action OnAccepted = () => { };
        /// <summary>
        /// Your invitation is rejected.
        /// </summary>
        public event Action OnRejected = () => { };
        /// <summary>
        /// Friend status has changed. For int argument values <see cref="FriendStatus"/>.
        /// You also can check friend status through FriendInfo frield.
        /// </summary>
        public event Action<int> OnStatusChanged = (s) => { };
        /// <summary>
        /// FriendInfo.Profile updated.
        /// </summary>
        public event Action<InviteableFriend> OnProfileUpdated = c => { };

        /// <summary>
        /// Reinvite cooldown delta.
        /// </summary>
        public TimeSpan InviteCooldown {
            get
            {
                var delta = _client.GetRoomInviteCooldown(FriendInfo.Id) - _time.Now;
                if(delta < TimeSpan.Zero)
                    return TimeSpan.Zero;
                return delta;
            }
        }

        /// <summary>
        /// If user rejects invite. You can't reinvite him for 20 seconds.
        /// </summary>
        public bool HasInviteCooldown
        {
            get { return InviteCooldown > TimeSpan.Zero; }
        }

        public InviteableFriend(IFriendsClientPrivate client, long roomId, FriendBase friendBase, bool versionMissmatch)
        {
            ContainerHolder.Container.BuildUp(this);
            FriendInfo = friendBase;
            _client = client;
            VersionMissmatch = versionMissmatch;
            RoomId = roomId;
            _client.OnRoomAccept += _accept;
            _client.OnRoomReject += _reject;
            _client.OnRoomAutoReject += _auto_reject;
            _client.OnFriendStatus += _status;
            _client.OnProfileUpdate += _profileUpdated;
        }

        public void Close()
        {
            _client.OnRoomAccept -= _accept;
            _client.OnRoomReject -= _reject;
            _client.OnRoomAutoReject -= _auto_reject;
            _client.OnFriendStatus -= _status;
            _client.OnProfileUpdate -= _profileUpdated;
        }

        public static implicit operator FriendBase(InviteableFriend cif)
        {
            if (cif == null) return null;
            return cif.FriendInfo;
        }

        /// <summary>
        /// Invite friend to room.
        /// 
        /// Target user will get <see cref="FriendEvent.RoomInvite"/> notification.
        /// 
        /// posible RoomResult values passed to callback:
        ///     * Success       - invite sent.
        ///     * NotFriend     - can't invite player if it's not friend-listed. Invite not sent.
        ///     * NotAllowed    - only host can invite friends.
        ///     * InvalidStatus - target friend is offline or in another room (check <see cref="FriendBase.Status"> for details).
        ///     * RoomIdFull    - room capacity reached, can't invite more players
        /// </summary>
        public void Invite(FriendsDelegate.RoomAnswerCallback callback)
        {
            if (!CanInvite)
            {
                if (callback != null)
                    callback(RoomId, RoomResult.InvalidStatus);
                return;
            }
            _userCallback = callback;
            InviteStatus = InviteStatus.Pending;
            _client.InviteRoom(RoomId, FriendInfo.Id, _callback);
        }

        private void _callback(long roomId, RoomResult result)
        {
            InviteResult = result;
            if (InviteResult != RoomResult.Success)
                InviteStatus = InviteStatus.Error;

            if (_userCallback != null)
            {
                try
                {
                    _userCallback(roomId, result);
                }
                catch (Exception e)
                {
                    Log.Error("User callback failed.", e);
                }
            }
        }

        private void _accept(RoomInviteEventMessage roomInviteEventMessage)
        {
            if (roomInviteEventMessage.From != FriendInfo.Id || roomInviteEventMessage.RoomId != RoomId)
                return;

            InviteStatus = InviteStatus.Accepted;
            OnAccepted();
        }

        private void _reject(RoomInviteEventMessage roomInviteEventMessage)
        {
            if (roomInviteEventMessage.From != FriendInfo.Id || roomInviteEventMessage.RoomId != RoomId)
                return;

            InviteStatus = InviteStatus.Rejected;
            OnRejected();
        }

        private void _auto_reject(RoomInviteEventMessage roomInviteEventMessage)
        {
            if(roomInviteEventMessage.From != FriendInfo.Id || roomInviteEventMessage.RoomId != RoomId)
                return;

            InviteStatus = InviteStatus.AutoRejected;
            OnRejected();
        }

        private void _status(FriendStatusChangedMessage evt)
        {
            if (evt.From != FriendInfo.Id || evt.StatusCode == FriendInfo.Status)
                return;
            FriendInfo.Status = evt.StatusCode;
            FriendInfo.LastStatus = evt.Time;
            OnStatusChanged(evt.StatusCode);
        }

        private void _profileUpdated(FriendsProfileUpdateMessage evt)
        {
            if (evt.From != FriendInfo.Id)
                return;
            var updatedProfile = _client.FriendList.FindFriendOrNull(evt.From);
            if (updatedProfile == null) // profile update for non-friend
            {
                FriendInfo.Profile = evt.Profile;
            }

            OnProfileUpdated(this);
        }
    }
}