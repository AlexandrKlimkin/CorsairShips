using System;
using FriendsClient.Private;
using S;

namespace FriendsClient.FriendList
{
    public interface IFriendMyGift
    {
        /// <summary>
        /// Gift receiver.
        /// </summary>
        MadId FriendId { get; }

        /// <summary>
        /// Client specific gift id.
        /// </summary>
        int GameSpecificId { get; }

        /// <summary>
        /// Gift already claimed.
        /// </summary>
        bool IsClaimed { get; }

        /// <summary>
        /// Call if you no longer need this object.
        /// </summary>
        void Close();

        void Send(FriendsDelegate.FriendGiftCallback callback = null);
        GiftResult SentResult { get; }
        event Action<IFriendMyGift> OnClaimed;
        event Action<IFriendMyGift, GiftResult> OnSent;
    }
}