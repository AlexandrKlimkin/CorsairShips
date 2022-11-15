using System;
using FriendsClient.Private;

namespace FriendsClient.FriendList
{
    public interface IFriendGift
    {
        long Id { get; }
        FriendBase FriendInfo { get; }
        int GameSpecificId { get; }
        bool Claimed { get; }
        GiftResult ClaimResult { get; }
        void Claim(FriendsDelegate.FriendGiftCallback callback = null);

        event Action<IFriendGift, GiftResult> OnClaimResult;
    }
}