using System;
using FriendsClient.Private;

namespace FriendsClient.FriendList.Concrete
{
    public class FriendGift : IFriendGift
    {
        private readonly IFriendsClientPrivate _client;

        public long Id { get; private set; }
        public FriendBase FriendInfo { get; private set; }
        public int GameSpecificId { get; private set; }
        public GiftResult ClaimResult { get; private set; }
        public bool Claimed => ClaimResult == GiftResult.Success;

        public event Action<IFriendGift, GiftResult> OnClaimResult = (c, g) => { };

        public FriendGift(IFriendsClientPrivate client, long giftId, FriendBase friendInfo, int gameSpecificId)
        {
            Id = giftId;
            _client = client;
            FriendInfo = friendInfo;
            GameSpecificId = gameSpecificId;
        }

        public void Claim(FriendsDelegate.FriendGiftCallback callback = null)
        {
            _client.ClaimGift(Id, (gid, r, n) => _claim(r, n, callback));
        }

        // public, т.к. используется при батч-клейме
        public void _claim(GiftResult result, DateTime next, FriendsDelegate.FriendGiftCallback callback)
        {
            ClaimResult = result;
            OnClaimResult(this, result);
            if (callback != null)
                callback(Id, result, next);
        }
    }
}
