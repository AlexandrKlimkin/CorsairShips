using System;
using FriendsClient.Private;
using S;

namespace FriendsClient.FriendList.Concrete
{
    public class FriendMyGift : IFriendMyGift
    {
        private readonly IFriendsClientPrivate _client;

        public long Id { get; private set; }
        /// <summary>
        /// Gift receiver.
        /// </summary>
        public MadId FriendId { get; private set; }
        /// <summary>
        /// Gift already claimed.
        /// </summary>
        public bool IsClaimed { get; private set; }
        public int GameSpecificId { get; private set; }
        public GiftResult SentResult { get; private set; }
        private bool _giftSent;
        private FriendsDelegate.FriendGiftCallback _userCallback;

        /// <summary>
        /// Raised when receiver claims gift.
        /// </summary>
        public event Action<IFriendMyGift> OnClaimed = (c) => { };

        public event Action<IFriendMyGift, GiftResult> OnSent = (c,r) => { };

        public FriendMyGift(IFriendsClientPrivate client, long id, MadId friendId, int gameSpecificId, GiftResult prevSendResult = GiftResult.None)
        {
            Id = id;
            _giftSent = Id != 0;
            _client = client;
            FriendId = friendId;
            GameSpecificId = gameSpecificId;
            _client.OnFriendGiftClaimed += _giftClaimed;
            SentResult = prevSendResult;
        }

        public void Send(FriendsDelegate.FriendGiftCallback callback = null)
        {
            if (_giftSent) return;
            _giftSent = true;
            _userCallback = callback;
            _client.SendGift(FriendId, GameSpecificId, _giftSentResult);
        }

        public void Close()
        {
            _client.OnFriendGiftClaimed -= _giftClaimed;
        }

        private void _giftSentResult(long id, GiftResult result, DateTime next)
        {
            Id = id;
            SentResult = result;
            OnSent(this, result);
            if (_userCallback != null)
                _userCallback(id, result, next);
        }

        private void _giftClaimed(FriendGiftEventMessage evt)
        {
            if(evt.From != FriendId || Id != evt.GiftId)
                return;
            IsClaimed = true;
            OnClaimed(this);
            _client.OnFriendGiftClaimed -= _giftClaimed;
        }
    }
}