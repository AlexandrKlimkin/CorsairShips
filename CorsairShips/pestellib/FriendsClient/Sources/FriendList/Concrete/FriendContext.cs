using System;
using System.Collections.Generic;
using System.Linq;
using FriendsClient.Private;
using FriendsClient.Sources;
using PestelLib.Utils;
using UnityDI;

namespace FriendsClient.FriendList.Concrete
{
    public class FriendContext : IFriendContext
    {
        private IFriendsClientPrivate _client;
#pragma warning disable 649
        [Dependency]
        private ITimeProvider _time;
#pragma warning restore 649
        private List<FriendMyGift> _friendMyGifts = new List<FriendMyGift>();

        /// <summary>
        /// Information about the user.
        /// </summary>
        public FriendBase FriendInfo { get; private set; }
        /// <summary>
        /// Checks gift send cooldown.
        /// </summary>
        public bool CanSendGift {
            get
            {
                if (_client.GiftsStackSize > 0 && _friendMyGifts.Count >= _client.GiftsStackSize)
                    return false;
                return _time.Now > FriendInfo.NextGift;
            } }
        /// <summary>
        /// Gifts sent by me.
        /// </summary>
        public IReadOnlyList<IFriendMyGift> MyGifts { get { return _friendMyGifts; } }
        /// <summary>
        /// Gifts sent by FriendInfo.Id to me.
        /// </summary>
        public IReadOnlyList<IFriendGift> Gifts { get { return _client.FriendList.Gifts.Where(_ => _.FriendInfo.Id == FriendInfo.Id).ToList(); } }
        /// <summary>
        /// true if player FriendInfo.Id is your friend.
        /// </summary>
        public bool IsMyFriend { get; private set; }
        /// <summary>
        /// Returns null if there is no active invite.
        /// </summary>
        public IFriendInviteContext InviteContext { get; private set; }
        /// <summary>
        /// Friend info changed redraw appropriate UI.
        /// </summary>
        public event Action<IFriendContext> OnFriendInfoChanged = (c) => { };
        /// <summary>
        /// Friend status has changed.
        /// </summary>
        public event Action<IFriendContext, int> OnStatusChanged = (c, status) => { };
        /// <summary>
        /// Your friend removed you from his friend-list.
        /// </summary>
        public event Action<IFriendContext> OnRemovedFromFriends = (c) => { };
        /// <summary>
        /// Player just send you a gift.
        /// </summary>
        public event Action<IFriendContext, IFriendGift> OnGift = (c,g) => { };

        public event Action<IFriendContext> OnProfileUpdated = c => { };

        public event Action OnMyGiftsUpdated = () => { };

        public FriendContext(IFriendsClientPrivate client, FriendBase info, bool? myFriend = null, List<GiftDescription> outGifts = null)
        {
            ContainerHolder.Container.BuildUp(this);

            _client = client;
            FriendInfo = info;
            InviteContext = _client.GetInvitationByFriend(info.Id);

            if (myFriend == null)
                Reinit();
            else
                IsMyFriend = myFriend.Value;

            if (outGifts != null)
            {
                _friendMyGifts = outGifts.Select(_ => new FriendMyGift(_client, _.Id, _.To, _.GameSpecificId, GiftResult.Success)).ToList();
                foreach (var outGift in _friendMyGifts)
                {
                    outGift.OnClaimed += _claimedOutGiftCallback;
                }
            }

            _client.OnFriendRemoved += _friendRemoved;
            _client.OnInviteAccepted += _inviteAccept;
            _client.OnFriendStatus += _statusChanged;
            _client.OnFriendGift += _incomingGift;
            _client.OnProfileUpdate += _profileUpdate;
        }

        private void _profileUpdate(FriendsProfileUpdateMessage evt)
        {
            if(evt.From != FriendInfo.Id)
                return;
            OnProfileUpdated(this);
        }

        public void Reinit()
        {
            IsMyFriend = _client.GetFriends().Any(f => f.Id == FriendInfo.Id);
        }

        public void Close()
        {
            _client.OnFriendRemoved -= _friendRemoved;
            _client.OnInviteAccepted -= _inviteAccept;
            _client.OnFriendStatus -= _statusChanged;
            _client.OnFriendGift -= _incomingGift;
            _client.OnProfileUpdate -= _profileUpdate;
        }

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
        public IFriendInviteContext InviteFriend(FriendsDelegate.InviteFriendCallback callback = null)
        {
            if (InviteContext != null && !InviteContext.CanResend)
                return InviteContext;
            var r = InviteContext = _client.CreateInvitaion(FriendInfo.Id, callback);
            return r;
        }

        /// <summary>
        /// Remove player from friend list.
        /// </summary>
        public void RemoveFriend()
        {
            _client.RemoveFriend(FriendInfo.Id, _removeCallback);
        }

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
        public IFriendMyGift SendGift(int giftId = 0, FriendsDelegate.FriendGiftCallback callback = null)
        {
            if (!CanSendGift)
            {
                if (callback != null)
                {
                    callback(0, GiftResult.NotAllowed, FriendInfo.NextGift);
                }
                return null;
            }

            
            var gift = new FriendMyGift(_client, 0, FriendInfo.Id, giftId);
            _friendMyGifts.Add(gift);
            gift.OnClaimed += _claimedOutGiftCallback;
            gift.Send((id, r,n) => _gifSent(id, r, n, callback));
            OnMyGiftsUpdated();
            return gift;
        }

        public IFriendGift[] ClaimAllGifts(Action<IFriendGift[]> callback)
        {
            return _client.FriendList.ClaimAllGiftsFromFriend(this, callback);
        }

        private void _inviteAccept(FriendsInviteEventMessage evt)
        {
            if(evt.From != FriendInfo.Id)
                return;

            IsMyFriend = true;
        }

        private void _removeCallback(bool success)
        {
            if(!success)
                return;

            _client.RemoveInvitationByFriend(FriendInfo.Id);
            IsMyFriend = false;
        }

        private void _claimedOutGiftCallback(IFriendMyGift source)
        {
            var claimedGifts = _friendMyGifts.Where(_ => _.IsClaimed).ToArray();
            if (claimedGifts.Length == 0) return;
            foreach (var gift in claimedGifts)
            {
                gift.OnClaimed -= _claimedOutGiftCallback;
                _friendMyGifts.Remove(gift);
            }
            OnMyGiftsUpdated();
        }

        private void _friendRemoved(FriendEventMessage evt)
        {
            _client.RemoveInvitationByFriend(evt.From);
			InviteContext = null;
            if (FriendInfo.Id != evt.From)
                return;

            IsMyFriend = false;
            OnRemovedFromFriends(this);
        }

        private void _statusChanged(FriendStatusChangedMessage evt)
        {
            if (evt.From != FriendInfo.Id || evt.StatusCode == FriendInfo.Status)
                return;

            FriendInfo.Status = evt.StatusCode;
            FriendInfo.LastStatus = evt.Time;
            OnStatusChanged(this, evt.StatusCode);
        }

        private void _gifSent(long giftId, GiftResult result, DateTime next, FriendsDelegate.FriendGiftCallback userCallback)
        {
            if(result != GiftResult.Success)
                return;
            FriendInfo.NextGift = next;
            if (userCallback != null)
                userCallback(giftId, result, next);
        }

        private void _incomingGift(FriendGiftEventMessage evt)
        {
            if(FriendInfo.Id != evt.From) return;
            var gift = _client.FriendList.Gifts.First(_ => _.Id == evt.GiftId);
            OnGift(this, gift);
        }
    }
}
