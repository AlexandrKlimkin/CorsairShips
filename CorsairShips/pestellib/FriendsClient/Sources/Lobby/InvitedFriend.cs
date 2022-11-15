using System;
using FriendsClient.Private;
using FriendsClient.Sources;
using log4net;

namespace FriendsClient.Lobby
{
    public class InvitedFriend
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InvitedFriend));
        private readonly IFriendsClientPrivate _client;
        private readonly IFriendsRoom _room;

        private FriendsDelegate.RoomAnswerCallback _userCallback;

        public FriendBase FriendInfo { get; private set; }
        public RoomResult KickResult { get; private set; }

        public event Action<InvitedFriend> OnKicked = (c) => { };
        public event Action<InvitedFriend> OnLeave = (c) => { };
        public event Action<InvitedFriend, int> OnStatusChanged = (c, s) => { };
        /// <summary>
        /// FriendInfo.Profile updated.
        /// </summary>
        public event Action<InvitedFriend> OnProfileUpdated = c => { };

        public InvitedFriend(IFriendsClientPrivate client, IFriendsRoom room, FriendBase friend)
        {
            _client = client;
            _room = room;
            var fromCache = client.FriendList.FindFriendOrNull(friend.Id)?.FriendInfo;
            if (fromCache == null && client.Id == friend.Id)
            {
                fromCache = _client.FriendList.Me;
            }
            FriendInfo = fromCache ?? friend;
            _client.OnFriendStatus += _status;
            _client.OnRoomLeave += _leave;
            _client.OnProfileUpdate += _profileUpdated;
        }

        public void Close()
        {
            _client.OnFriendStatus -= _status;
            _client.OnRoomLeave -= _leave;
            _client.OnProfileUpdate -= _profileUpdated;
        }

        public static implicit operator FriendBase(InvitedFriend fi)
        {
            if (fi == null) return null;
            return fi.FriendInfo;
        }

        /// <summary>
        /// Kick invited friend from room. Only host can perform this operation.
        /// 
        /// Room members will get <see cref="FriendEvent.RoomKick"/> notification.
        /// 
        /// posible RoomResult values passed to callback:
        ///     * Success       - room member kicked.
        ///     * NotAllowed    - only host can kick members.
        ///     * NotInvited    - player not in room.
        /// 
        /// </summary>
        /// <param name="callback"></param>
        public void Kick(FriendsDelegate.RoomAnswerCallback callback)
        {
            if (!_room.ImHost)
            {
                KickResult = RoomResult.NotAllowed;
                if (callback != null)
                    callback(_room.RoomId, KickResult);
                return;
            }
            // kick already requested
            if (KickResult != RoomResult.None)
                return;
            _userCallback = callback;
            _client.KickRoom(_room.RoomId, FriendInfo.Id, _kickCallback);
        }

        private void _kickCallback(long roomId, RoomResult result)
        {
            KickResult = result;
            OnKicked(this);
            if (_userCallback == null)
                return;
            try
            {
                _userCallback(roomId, result);
            }
            catch (Exception e)
            {
                Log.Error("User callback failed.", e);
            }
        }

        private void _leave(RoomLeaveKickEventMessage evt)
        {
            if (evt.FriendId != FriendInfo.Id)
                return;
            OnLeave(this);
        }

        private void _profileUpdated(FriendsProfileUpdateMessage evt)
        {
            if(evt.From != FriendInfo.Id)
                return;
            var updatedProfile = _client.FriendList.FindFriendOrNull(evt.From);
            if (updatedProfile == null) // profile update for non-friend
            {
                FriendInfo.Profile = evt.Profile;
            }

            OnProfileUpdated(this);
        }

        private void _status(FriendStatusChangedMessage evt)
        {
            if (evt.From != FriendInfo.Id || evt.StatusCode == FriendInfo.Status)
                return;
            FriendInfo.Status = evt.StatusCode;
            FriendInfo.LastStatus = evt.Time;
            OnStatusChanged(this, evt.StatusCode);
        }
    }
}