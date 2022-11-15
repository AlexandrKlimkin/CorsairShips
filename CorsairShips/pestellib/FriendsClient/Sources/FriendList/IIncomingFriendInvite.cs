using System;
using FriendsClient.Private;

namespace FriendsClient.FriendList
{
    public interface IIncomingFriendInvite
    {
        InviteFriendResult SendResult { get; }
        /// <summary>
        /// Send result. Anything except <see cref="InviteFriendResult.Success"/> is abnormal.
        /// </summary>
        event Action<IIncomingFriendInvite, InviteFriendResult> OnAnswerSent;
        /// <summary>
        /// Invite was canceled by sender.
        /// </summary>
        event Action<IIncomingFriendInvite> OnCancel;
        /// <summary>
        /// Invite has expired.
        /// </summary>
        event Action<IIncomingFriendInvite> OnExpired;
        /// <summary>
        /// Invite was canceled by sender.
        /// </summary>
        bool Canceled { get; }
        /// <summary>
        /// Invite was accepted.
        /// </summary>
        bool Accepted { get; }
        /// <summary>
        /// Invite has expired.
        /// </summary>
        bool Expired { get; }

        FriendBase FriendInfo { get; }

        DateTime ExpireTime { get; }

        event Action<IIncomingFriendInvite, int> OnStatusChanged;
        /// <summary>
        /// Accept friend invitation.
        /// </summary>
        /// <param name="callback"></param>
        void Accept(FriendsDelegate.InviteFriendCallback callback = null);

        /// <summary>
        /// Reject friend invitation.
        /// </summary>
        void Reject(FriendsDelegate.InviteFriendCallback callback = null);
    }
}