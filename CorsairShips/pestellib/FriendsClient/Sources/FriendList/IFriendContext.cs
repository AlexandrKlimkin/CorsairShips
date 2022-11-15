using System;
using System.Collections.Generic;
using FriendsClient.FriendList;
using FriendsClient.Private;
using FriendsClient.Sources;

namespace FriendsClient
{
    public interface IFriendContext
    {
        /// <summary>
        /// Information about the user.
        /// </summary>
        FriendBase FriendInfo { get; }

        /// <summary>
        /// Checks gift send cooldown.
        /// </summary>
        bool CanSendGift { get; }

        /// <summary>
        /// Gift sent by me.
        /// </summary>
        IReadOnlyList<IFriendMyGift> MyGifts { get; }

        /// <summary>
        /// Gift send by FriendInfo.Id to me.
        /// </summary>
        IReadOnlyList<IFriendGift> Gifts { get; }

        /// <summary>
        /// true if player FriendInfo.Id is your friend.
        /// </summary>
        bool IsMyFriend { get; }

        /// <summary>
        /// Outgoing invite to become friend (Sent by you). For incoming invites see <see cref="IFriendList.FriendInvitations"/>.
        /// Returns null if there is no active invite.
        /// </summary>
        IFriendInviteContext InviteContext { get; }

        /// <summary>
        /// Friend info changed redraw appropriate UI.
        /// </summary>
        event Action<IFriendContext> OnFriendInfoChanged;

        /// <summary>
        /// Friend status has changed.
        /// </summary>
        event Action<IFriendContext, int> OnStatusChanged;

        /// <summary>
        /// Your friend removed you from his friend-list.
        /// </summary>
        event Action<IFriendContext> OnRemovedFromFriends;

        /// <summary>
        /// Player just send you a gift.
        /// </summary>
        event Action<IFriendContext, IFriendGift> OnGift;

        /// <summary>
        /// <see cref="FriendBase.Profile"/> updated (FriendInfo.Profile).
        /// </summary>
        event Action<IFriendContext> OnProfileUpdated;

        /// <summary>
        /// Invites FriendInfo.Id to become a friend.
        /// 
        /// Target user will get <see cref="FriendEvent.FriendInvite"/> notification.
        /// 
        /// posible InviteFriendResult values passed to callback:
        ///     * Success       - invite sent
        ///     * MyLimit       - my friends list reached cap MaxFriends (invite not sent).
        ///     * OtherLimit    - target player's friends list reached cap MaxFriends (invite not sent).
        ///     * InviteLimit   - target player's pending invites list reached limit MaxInvites (invite not sent).
        ///     * AlreadyFriend
        ///     * AlreadySent
        /// </summary>
        IFriendInviteContext InviteFriend(FriendsDelegate.InviteFriendCallback callback = null);

        /// <summary>
        /// Remove player from friend list.
        /// </summary>
        void RemoveFriend();

        /// <summary>
        /// Sends gift to FriendInfo.Id user.
        /// 
        /// Possible <see cref="GiftResult"/>passed to callback:
        ///     * <see cref="GiftResult.Success"/>
        ///     * <see cref="GiftResult.NotAllowed"/> - cooldown.
        /// </summary>
        /// <param name="giftId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        IFriendMyGift SendGift(int giftId = 0, FriendsDelegate.FriendGiftCallback callback = null);

        /// <summary>
        ///  Claims all gifts from friend.
        /// </summary>
        /// <param name="callback"></param>
        IFriendGift[] ClaimAllGifts(Action<IFriendGift[]> callback);
    }
}