using System;
using FriendsClient.Private;
using S;

namespace FriendsClient.FriendList.Concrete
{
    public class IncomingFriendInvite : IIncomingFriendInvite
    {
        private readonly IFriendsClientPrivate _client;
        public long InviteId { get; private set; }
        public MadId FriendId { get; private set; }
        public FriendBase FriendInfo { get; private set; }
        public DateTime ExpireTime { get; private set; }
        public InviteFriendResult SendResult { get; private set; }
        public event Action<IIncomingFriendInvite, InviteFriendResult> OnAnswerSent = (c, r) => { };
        public event Action<IIncomingFriendInvite> OnCancel = (c) => { };
        public bool Canceled { get; private set; }
        public bool Accepted { get; private set; }
        public bool Expired { get; private set; }
        private bool _sent;

        public event Action<IIncomingFriendInvite, int> OnStatusChanged = (c,i) => { };
        public event Action<IIncomingFriendInvite> OnExpired = (c) => { };

        public IncomingFriendInvite(IFriendsClientPrivate client, long inviteId, FriendBase friend, DateTime expireTime)
        {
            _client = client;
            InviteId = inviteId;
            FriendId = friend.Id;
            FriendInfo = friend;
            ExpireTime = expireTime;
            _client.OnInviteCanceled += _inviteCanceled;
            _client.OnFriendStatus += _status;
            _client.OnInviteExpired += _expired;
        }

        private void _expired(FriendsInviteEventMessage evt)
        {
            if(evt.InviteId != InviteId || Expired) return;
            Expired = true;
            OnExpired(this);
            Close();
        }

        /// <summary>
        /// Accept friend invitation.
        /// </summary>
        /// <param name="callback"></param>
        public void Accept(FriendsDelegate.InviteFriendCallback callback = null)
        {
            if(_sent) return;
            _sent = true;
            if (SendResult != InviteFriendResult.None) return;
            _client.InviteFriendAnswer(InviteId, true, (id, r) =>_inviteAnswerCallback(id, r, callback, true));
        }

        /// <summary>
        /// Reject friend invitation.
        /// </summary>
        public void Reject(FriendsDelegate.InviteFriendCallback callback = null)
        {
            if (_sent) return;
            _sent = true;
            if (SendResult != InviteFriendResult.None) return;
            _client.InviteFriendAnswer(InviteId, false, (id, r) => _inviteAnswerCallback(id, r, callback, false));
        }

        public void Close()
        {
            _client.OnInviteCanceled -= _inviteCanceled;
            _client.OnFriendStatus -= _status;
            _client.OnInviteExpired -= _expired;
        }

        private void _status(FriendStatusChangedMessage evt)
        {
            if (evt.From != FriendId || evt.StatusCode == FriendInfo.Status)
                return;
            FriendInfo.Status = evt.StatusCode;
            FriendInfo.LastStatus = evt.Time;
            OnStatusChanged(this, evt.StatusCode);
        }

        private void _inviteCanceled(FriendsInviteEventMessage evt)
        {
            if(evt.InviteId != InviteId) return;
            Canceled = true;
            Close();
            OnCancel(this);
        }

        private void _inviteAnswerCallback(long inviteId, InviteFriendResult result, FriendsDelegate.InviteFriendCallback userCallback, bool acceptCallback)
        {
            // answer duplicates due to multiple calls to Accept/Reject
            if (SendResult != InviteFriendResult.None)
            {
                userCallback?.Invoke(inviteId, result);
                return;
            }

            //Reset trigger if invitation not accepted
            if (result != InviteFriendResult.Success)
                _sent = false;

            // in situation of frind limit we can accept invitation again (by design)
            if (result != InviteFriendResult.MyLimit && result != InviteFriendResult.OtherLimit)
                SendResult = result;
            if(acceptCallback)
                Accepted = SendResult == InviteFriendResult.Success;
            OnAnswerSent(this, result);
            userCallback?.Invoke(inviteId, result);
            if (SendResult != InviteFriendResult.None)
            {
                Close();
            }
        }
    }
}
