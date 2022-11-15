using System;
using FriendsClient.Private;
using S;

namespace FriendsClient.FriendList
{
    public interface IFriendInviteContext
    {
        MadId FriendId { get; }
        long InviteId { get; }
        InviteFriendResult Result { get; }
        InviteStatus Status { get; }
        bool CanResend { get; }
        /// <summary>
        /// Invite was accepted. Fired with <see cref="IFriendList.OnNewFriend"/>.
        /// </summary>
        event Action<IFriendInviteContext> OnAccept;
        /// <summary>
        /// Invate was rejected.
        /// </summary>
        event Action<IFriendInviteContext> OnReject;
        /// <summary>
        /// Invite has expired.
        /// </summary>
        event Action<IFriendInviteContext> OnExpired;
        /// <summary>
        /// Invite send result. Anything except <see cref="InviteFriendResult.Success"/> is abnormal.
        /// </summary>
        event Action<IFriendInviteContext, InviteFriendResult> OnInviteSendResult;

        /// <summary>
        /// Call after you subscribe on events.
        /// </summary>
        void Send();

        /// <summary>
        /// Cancels pending invitation.
        /// </summary>
        /// <returns></returns>
        bool Cancel();
    }
}