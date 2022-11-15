using System;
using System.Collections.Generic;
using FriendsClient.Sources;
using S;

namespace FriendsClient.FriendList
{
    public interface IFriendList
    {
        /// <summary>
        /// Friend accepts your invitation.
        /// </summary>
        event Action<IFriendContext> OnNewFriend;
        /// <summary>
        /// You removed friend from list.
        /// Your friend removed you from his/her friend list.
        /// </summary>
        event Action<IFriendContext> OnFriendRemoved;
        /// <summary>
        /// You were invited to become a friend. (invitation also added to <see cref="FriendInvitations"/>).
        /// </summary>
        event Action<IIncomingFriendInvite> OnInvite;
        /// <summary>
        /// Invite from another player has expired.
        /// </summary>
        event Action<IIncomingFriendInvite> OnInviteExpired;
        /// <summary>
        /// You receive new gift. <see cref="Gifts"/> collection also changed.
        /// </summary>
        event Action<IFriendGift> OnGift;
        /// <summary>
        /// My player has changed status. For game specific statuses its exactly that you pass to <see cref="StatusChanged"/>
        /// </summary>
        event Action<int> OnStatusChanged;
        /// <summary>
        /// <see cref="FriendInvitations"/> collection has changed (invite added or removed). Caused by invite accept or friend remove.
        /// </summary>
        event Action OnInvitesUpdated;
        /// <summary>
        /// <see cref="Friends"/> collection has changed (friend added or removed). Caused by incoming invite or invite answer.
        /// </summary>
        event Action OnFriendsUpdated;
        /// <summary>
        /// <see cref="Gifts"/> collection has changed (gift added or removed). Caused by incoming gift, or gift claim.
        /// </summary>
        event Action OnGiftsUpdated;
        /// <summary>
        /// <see cref="MyGifts"/> collection has changed (gift claimed by target player).
        /// </summary>
        event Action OnMyGiftsUpdated;
        /// <summary>
        /// List of your friends.
        /// List can be is changed due to server events. Subscribe <see cref="OnNewFriend"/> and <see cref="OnFriendRemoved"/> to be updated.
        /// </summary>
        IReadOnlyList<IFriendContext> Friends { get; }
        /// <summary>
        /// Gifts sent by me.
        /// </summary>
        IReadOnlyList<IFriendMyGift> MyGifts { get; }
        /// <summary>
        /// Gifts sent to me.
        /// </summary>
        IReadOnlyList<IFriendGift> Gifts { get; }
        /// <summary>
        /// Incoming friend invitations.
        /// </summary>
        IReadOnlyList<IIncomingFriendInvite> FriendInvitations { get; }
        /// <summary>
        /// Info about my player (owner of this friend list).
        /// </summary>
        FriendBase Me { get; }
        /// <summary>
        /// Tries to find player by id. If not found null is passed to callback.
        /// </summary>
        /// <param name="friendId"></param>
        /// <param name="callback"></param>
        void FindFriend(MadId friendId, Action<IFriendContext> callback);
        /// <summary>
        /// Lookup internal friends cache and immediately returns appropriate IFriendContext or null if not found.
        /// </summary>
        /// <returns></returns>
        IFriendContext FindFriendOrNull(MadId friendId);
        /// <summary>
        /// Lookup internal friends cache and immediately returns appropriate IFriendContext or null if not found.
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        IFriendContext FindFriendOrNull(Guid playerId);
        /// <summary>
        /// Tries to find one+ player(s) by player id. If not found empty array will be passed to the callback.
        /// </summary>
        /// <param name="playerIds"></param>
        /// <param name="callback"></param>
        void FindFriends(Guid[] playerIds, Action<IFriendContext[]> callback);
        /// <summary>
        /// Report status change. You must send any game specific player status. Dont send offline (0), online (1) and in room(2) statuses here they are processed automaticaly by the server.
        /// Each friend and room members (even if they are not friends) will get <see cref="FriendEvent.StatusChanged"/> notification.
        /// </summary>
        /// <param name="statusId">any value >=3 has game specific meaning. Predefined values 0 - offline, 1 - online, 2 - in room.</param>
        void StatusChanged(int statusId);
        /// <summary>
        /// Claim all gifts.
        /// </summary>
        IFriendGift[] ClaimAllGifts(Action<IFriendGift[]> callback);
        /// <summary>
        /// Claim all gifts from specific friend.
        /// </summary>
        IFriendGift[] ClaimAllGiftsFromFriend(IFriendContext friend, Action<IFriendGift[]> callback);
        /// <summary>
        /// Check gift claim state on server. For the case when claim request sent but not properly processed by client (e.g. due crash).
        /// </summary>
        /// <param name="gifts"></param>
        /// <param name="callback"></param>
        void GetGiftClaimState(long[] gifts, Action<GiftStatus[]> callback);
        /// <summary>
        /// Watch status change for non-friend players. (eg. for clan members which are not friends)
        /// </summary>
        /// <param name="playerIds"></param>
        void SetNonFriendsStatusWatch(Guid[] playerIds);

    }
}
