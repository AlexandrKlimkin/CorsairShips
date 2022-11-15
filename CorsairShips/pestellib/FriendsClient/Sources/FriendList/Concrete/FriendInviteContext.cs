using System;
using FriendsClient.Private;
using S;

namespace FriendsClient.FriendList.Concrete
{
    public class FriendInviteContext : IFriendInviteContext
    {
        private IFriendsClientPrivate _client;
        private FriendsDelegate.InviteFriendCallback _userCallback;
        private bool _isexpired;

        public MadId FriendId { get; private set; }
        public long InviteId { get; private set; }
        public InviteFriendResult Result { get; private set; }
        public InviteStatus Status { get; private set; }

        public event Action<IFriendInviteContext> OnAccept = (c) => { };
        public event Action<IFriendInviteContext> OnReject = (c) => { };
        public event Action<IFriendInviteContext> OnExpired = (c) => { };
        public event Action<IFriendInviteContext, InviteFriendResult> OnInviteSendResult = (c,r) => { };

        public bool CanResend {
            get { return Result != InviteFriendResult.Success || Status.CanResend() || _isexpired; }
        }

        public FriendInviteContext(IFriendsClientPrivate client, MadId friendId, FriendsDelegate.InviteFriendCallback callback)
        {
            _client = client;
            FriendId = friendId;
            _userCallback = callback;
            _client.OnInviteAccepted += _accepted;
            _client.OnInviteRejected += _rejected;
            _client.OnInviteExpired += _expired;
        }

        /// <summary>
        /// Call after you subscribe on events.
        /// </summary>
        public void Send()
        {
            Status = InviteStatus.Pending;
            _client.InviteFriend(FriendId, _callback);
        }

        /// <summary>
        /// Cancels pending invitation.
        /// </summary>
        /// <returns></returns>
        public bool Cancel()
        {
            if (InviteId == 0)
                return false;
            if (Status != InviteStatus.Pending)
                return false;
            _client.InviteFriendCancel(InviteId);
            Close();
            return true;
        }

        public void Close()
        {
            _client.OnInviteAccepted -= _accepted;
            _client.OnInviteRejected -= _rejected;
            _client.OnInviteExpired -= _expired;
        }

        private void _expired(FriendsInviteEventMessage evt)
        {
            if(evt.InviteId != InviteId) return;
            _isexpired = true;
            Close();
            OnExpired(this);
        }

        private void _rejected(FriendsInviteEventMessage evt)
        {
            if (evt.InviteId != InviteId || evt.From != FriendId)
                return;
            Status = InviteStatus.Rejected;
            Close();
            OnReject(this);
        }

        private void _accepted(FriendsInviteEventMessage evt)
        {
            if (evt.InviteId != InviteId || evt.From != FriendId)
                return;
            Status = InviteStatus.Accepted;
            Close();
            OnAccept(this);
        }

        private void _callback(long inviteId, InviteFriendResult result)
        {
            if (result != InviteFriendResult.Success)
                Status = InviteStatus.Error;
            InviteId = inviteId;
            Result = result;
            OnInviteSendResult(this, result);
            if (_userCallback != null)
                _userCallback(inviteId, result);
        }
    }
}