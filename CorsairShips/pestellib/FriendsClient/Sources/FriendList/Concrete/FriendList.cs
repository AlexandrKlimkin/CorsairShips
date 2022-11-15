using System;
using System.Collections.Generic;
using System.Linq;
using FriendsClient.Private;
using FriendsClient.Sources;
using S;
using log4net;
using UnityDI;

namespace FriendsClient.FriendList.Concrete
{
    

    public class FriendList : IFriendList, IDisposable
    {
        private readonly IFriendsClientPrivate _client;
        private List<FriendContext> _playerContexts = new List<FriendContext>();
        private List<FriendContext> _batchFindCache = new List<FriendContext>();
        private List<IncomingFriendInvite> _invites = new List<IncomingFriendInvite>();
        private List<FriendGift> _friendGifts = new List<FriendGift>();

        public event Action<IFriendContext> OnNewFriend;
        public event Action<IFriendContext> OnFriendRemoved;
        public event Action<IIncomingFriendInvite> OnInvite;
        public event Action<IIncomingFriendInvite> OnInviteExpired;
        public event Action<IFriendGift> OnGift;
        public event Action<int> OnStatusChanged;
        public event Action OnInvitesUpdated;
        public event Action OnFriendsUpdated;
        public event Action OnGiftsUpdated;
        public event Action OnMyGiftsUpdated;

        public IReadOnlyList<IFriendMyGift> MyGifts { get { return _playerContexts.SelectMany(_ => _.MyGifts).ToList(); } }
        public IReadOnlyList<IFriendGift> Gifts { get { return _friendGifts; } }
        public FriendBase Me { get; private set; }
        private List<FriendBase> _nonFriends = new List<FriendBase>();
#pragma warning disable 649
        [Dependency]
        private IExternalFriendsStatusWatch _externalFriendsStatusWatch;
#pragma warning restore 649
        private static readonly ILog Log = LogManager.GetLogger(typeof(FriendList));

        private void ResetEvents()
        {
            OnNewFriend = ctx => { };
            OnFriendRemoved = ctx => { };
            OnInvite = inv => { };
            OnInviteExpired = inv => { };
            OnGift = g => { };
            OnStatusChanged = s => { };
            OnInvitesUpdated = () => { };
            OnFriendsUpdated = () => { };
            OnGiftsUpdated = () => { };
            OnMyGiftsUpdated = () => { };
        }

        public void Reinit(FriendBase me, List<FriendBase> myFriends, IEnumerable<InviteFriendDescription> friendInvites, IEnumerable<GiftDescription> gifts)
        {
            Me = me;

            foreach (var context in _playerContexts)
            {
                context.Close();
            }
            _playerContexts.Clear();
            foreach (var inv in _invites)
            {
                inv.Close();
            }
            _invites.Clear();
            _friendGifts.Clear();
            _nonFriends.Clear();

            var friendsMap = myFriends.ToDictionary(_ => _.Id);
            if (gifts != null)
            {
                var giftsList = gifts.Where(_ => _.To == _client.Id);
                foreach (var friendGift in giftsList)
                {
                    var giftContext = new FriendGift(_client, friendGift.Id, friendGift.FriendInfo ?? friendsMap[friendGift.From], friendGift.GameSpecificId);
                    _friendGifts.Add(giftContext);
                    giftContext.OnClaimResult += _claimInGiftCallback;
                    if (friendGift.FriendInfo != null)
                        _nonFriends.Add(friendGift.FriendInfo);
                }
            }

            if (myFriends != null)
            {
                foreach (var friend in myFriends)
                {
                    FriendContext ctx;
                    if (gifts == null)
                    {
                        ctx = new FriendContext(_client, friend, true);
                    }
                    else
                    {
                        var outGifts = gifts.Where(_ => _.From == _client.Id && _.To == friend.Id).ToList();
                        ctx = new FriendContext(_client, friend, true, outGifts);
                    }
                    ctx.OnMyGiftsUpdated += OnPlayerMyGiftsUpdated;
                    _playerContexts.Add(ctx);
                }
            }

            if (friendInvites != null)
            {
                foreach (var invite in friendInvites)
                {
                    var inviteCtx = new IncomingFriendInvite(_client, invite.InviteId, invite.FriendInfo, invite.ExpireTime);
                    inviteCtx.OnAnswerSent += _inviteAnswered;
                    _invites.Add(inviteCtx);
                }
            }
        }

        public FriendList(IFriendsClientPrivate client, FriendBase me, List<FriendBase> myFriends, IEnumerable<InviteFriendDescription> friendInvites, IEnumerable<GiftDescription> gifts)
        {
            ContainerHolder.Container.BuildUp(this);
            ResetEvents();
            _client = client;
            _client.OnFriendRemoved += _friendRemoved;
            _client.OnInvite += _incomingInvite;
            _client.OnInviteAccepted += _inviteAccepted;
            _client.OnFriendGift += _gift;
            _client.OnFriendStatus += _status;
            _client.OnInviteExpired += _invExpired;
            _client.OnNewFriend += _newFriend;
            _client.OnProfileUpdate += _profileUpdate;

            Reinit(me, myFriends, friendInvites, gifts);
        }

        public void Dispose()
        {
            Close();
        }

        private void OnPlayerMyGiftsUpdated()
        {
            OnMyGiftsUpdated();
        }

        /// <summary>
        /// If you no longer need this object call Close.
        /// </summary>
        public void Close()
        {
            ResetEvents();
            OnInvitesUpdated = () => { };
            _client.OnFriendRemoved -= _friendRemoved;
            _client.OnInvite -= _incomingInvite;
            _client.OnInviteAccepted -= _inviteAccepted;
            _client.OnFriendGift -= _gift;
            _client.OnFriendStatus -= _status;
            _client.OnInviteExpired -= _invExpired;
            _client.OnNewFriend -= _newFriend;
            _client.OnProfileUpdate -= _profileUpdate;

        }

        public IReadOnlyList<IIncomingFriendInvite> FriendInvitations { get { return _invites; } }

        /// <summary>
        /// List of your friends.
        /// List can be is changed due to server events. Subscribe <see cref="OnNewFriend"/> and <see cref="OnFriendRemoved"/> to be updated.
        /// </summary>
        public IReadOnlyList<IFriendContext> Friends
        {
            get { return _playerContexts; }
        }

        /// <summary>
        /// Tries to find player by id. If not found null is passed to callback.
        /// </summary>
        /// <param name="friendId"></param>
        /// <param name="callback"></param>
        public void FindFriend(MadId friendId, Action<IFriendContext> callback)
        {
            var ctx = FindFriendOrNull(friendId);
            if (ctx != null)
            {
                callback(ctx);
                return;
            }

            _client.FindFriend(friendId, (r) => _findFriend(r, callback));
        }

        public IFriendContext FindFriendOrNull(MadId friendId)
        {
            return FindFriendOrNull(_ => _.FriendInfo.Id == friendId);
        }

        public IFriendContext FindFriendOrNull(Guid playerId)
        {
            return FindFriendOrNull(_ => _.FriendInfo.Profile.PlayerId == playerId);
        }

        private IFriendContext FindFriendOrNull(Func<IFriendContext, bool> pred)
        {
            var ctx = _playerContexts.FirstOrDefault(pred);
            if (ctx != null)
            {
                return ctx;
            }
            ctx = _batchFindCache.FirstOrDefault(pred);
            if (ctx != null)
            {
                return ctx;
            }

            return null;
        }

        public void FindFriends(Guid[] playerIds, Action<IFriendContext[]> callback)
        {
            var idHash = new HashSet<Guid>(playerIds);
            var result = new List<IFriendContext>();
            for (var i = 0; i < _playerContexts.Count; ++i)
            {
                if(!idHash.Contains(_playerContexts[i].FriendInfo.Profile.PlayerId))
                    continue;

                idHash.Remove(_playerContexts[i].FriendInfo.Profile.PlayerId);
                result.Add(_playerContexts[i]);
            }

            for (var i = 0; i < _batchFindCache.Count; ++i)
            {
                if(!idHash.Contains(_batchFindCache[i].FriendInfo.Profile.PlayerId))
                    continue;

                idHash.Remove(_batchFindCache[i].FriendInfo.Profile.PlayerId);
                result.Add(_batchFindCache[i]);
            }

            if (idHash.Count == 0)
            {
                callback(result.ToArray());
                return;
            }

            _client.FindFriends(idHash.ToArray(), friends =>
            {
                foreach (var friend in friends)
                {
                    var ctx = new FriendContext(_client, friend);
                    result.Add(ctx);
                    _batchFindCache.Add(ctx);
                }
                callback(result.ToArray());
            });
        }

        public void StatusChanged(int statusId)
        {
            // dont set server defined statuses they are figured automatically
            if(statusId < FriendStatus.Count) return;
            _client.StatusChanged(statusId);
            OnStatusChanged(statusId);
        }

        public IFriendGift[] ClaimAllGifts(Action<IFriendGift[]> callback)
        {
            return ClaimGifts(_friendGifts, callback);
        }

        public IFriendGift[] ClaimAllGiftsFromFriend(IFriendContext friend, Action<IFriendGift[]> callback)
        {
            var it = _friendGifts.Where(_ => _.FriendInfo.Id == friend.FriendInfo.Id);
            return ClaimGifts(it, callback);
        }

        public void GetGiftClaimState(long[] gifts, Action<GiftStatus[]> callback)
        {
            _client.GetGiftClaimState(gifts, callback);
        }

        public void SetNonFriendsStatusWatch(Guid[] playerIds)
        {
            _client.SetNonFriendsStatusWatch(playerIds);
        }

        private FriendGift[] ClaimGifts(IEnumerable<FriendGift> targetGifts, Action<IFriendGift[]> callback)
        {
            var giftsMap = targetGifts.ToDictionary(_ => _.Id);
            var giftIds = giftsMap.Keys.ToArray();
            var gifts = giftsMap.Values.ToArray();
            _client.ClaimGifts(giftIds, (ids, results, nextGift) => {
                for (var i = 0; i < ids.Length; ++i)
                {
                    try
                    {
                        giftsMap[ids[i]]._claim(results[i], nextGift, null);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
                callback?.Invoke(giftsMap.Values.ToArray());
            });
            return gifts;
        }

        private void _invExpired(FriendsInviteEventMessage evt)
        {
            var invite = _invites.FirstOrDefault(_ => _.InviteId == evt.InviteId);
            if (invite != null)
            {
                _invites.Remove(invite);
                OnInviteExpired(invite);
                OnInvitesUpdated();
            }
        }

        private void _nonFriendStatus(FriendStatusChangedMessage evt)
        {
            var nonFriends = _nonFriends.FindAll(_ => _.Id == evt.From);
            if(_externalFriendsStatusWatch != null)
                nonFriends.AddRange(_externalFriendsStatusWatch.GetWatchedPlayers());
            if(nonFriends.Count == 0)
                return;
            foreach (var friend in nonFriends)
            {
                if (friend.Id == evt.From)
                {
                    friend.Status = evt.StatusCode;
                    friend.LastStatus = evt.Time;
                }
            }
        }

        private void _status(FriendStatusChangedMessage evt)
        {
            if (evt.From != _client.Id)
            {
                _nonFriendStatus(evt);
                return;
            }

            Me.Status = evt.StatusCode;
            Me.LastStatus = evt.Time;
            OnStatusChanged(Me.Status);
        }

        private void _claimInGiftCallback(IFriendGift source, GiftResult result)
        {
            var claimedGifts = _friendGifts.Where(_ => _.Claimed).ToArray();
            if(claimedGifts.Length == 0) return;
            foreach (var gift in claimedGifts)
            {
                gift.OnClaimResult -= _claimInGiftCallback;
                _friendGifts.Remove(gift);
            }
            OnGiftsUpdated();
        }

        private void _gift(FriendGiftEventMessage evt)
        {
            var from = _playerContexts.FirstOrDefault(_ => _.FriendInfo.Id == evt.From);
            var inGift = new FriendGift(_client, evt.GiftId, from.FriendInfo, evt.GameSpecificId);
            _friendGifts.Add(inGift);
            inGift.OnClaimResult += _claimInGiftCallback;
            OnGift(inGift);
            OnGiftsUpdated();
        }

        private void _findFriend(FriendBase friend, Action<FriendContext> callback)
        {
            if (friend == null)
            {
                callback(null);
                return;
            }

            var friendCtx = new FriendContext(_client, friend);
            _batchFindCache.Add(friendCtx);
            callback(friendCtx);
        }

        private void _friendRemoved(FriendEventMessage evt)
        {
            var idx = _playerContexts.FindIndex(_ => _.FriendInfo.Id == evt.From);
            if (idx < 0) return;
            var ctx = _playerContexts[idx];
            ctx.OnMyGiftsUpdated -= OnPlayerMyGiftsUpdated;
            _playerContexts.RemoveAt(idx);
            _batchFindCache.Add(ctx);
            OnFriendRemoved(ctx);
            OnFriendsUpdated();
        }

        private void _profileUpdate(FriendsProfileUpdateMessage evt)
        {
            // self update
            if (evt.From == Me.Id)
            {
                Me.Profile = evt.Profile;
                return;
            }

            var friend = FindFriendOrNull(evt.From);
            if(friend == null)
                return;
            friend.FriendInfo.Profile = evt.Profile;
        }

        private void _newFriend(FriendEventMessage evt)
        {
            var invite = _invites.FirstOrDefault(_ => _.FriendInfo.Id == evt.From);
            if(invite == null) return;
            _invites.Remove(invite);
            var friendInfo = invite.FriendInfo;
            var friendCtx = new FriendContext(_client, friendInfo, true);
            if (_playerContexts.All(_ => _.FriendInfo.Id != friendInfo.Id))
            {
                friendCtx.OnMyGiftsUpdated += OnPlayerMyGiftsUpdated;
                _playerContexts.Add(friendCtx);
                OnNewFriend(friendCtx);
                OnFriendsUpdated();
                OnInvitesUpdated();
            }
        }

        private void _inviteAccepted(FriendsInviteEventMessage evt)
        {
            var friendCtx = new FriendContext(_client, evt.FriendInfo, true);
            friendCtx.OnMyGiftsUpdated += OnPlayerMyGiftsUpdated;
            _playerContexts.Add(friendCtx);
            OnNewFriend(friendCtx);
            OnFriendsUpdated();
        }

        private void _inviteAnswered(IIncomingFriendInvite incoming, InviteFriendResult result)
        {
            var removeList = _invites.Where(_ => _.SendResult != InviteFriendResult.None).ToArray();
            foreach (var invite in removeList)
            {
                invite.OnAnswerSent -= _inviteAnswered;
                if (invite.SendResult == InviteFriendResult.Success)
                {
                    if (invite.Accepted)
                    {
                        var friendCtx = _batchFindCache.FirstOrDefault(_ => _.FriendInfo.Id == invite.FriendInfo.Id);
                        bool reinit = friendCtx != null;
                        if (friendCtx != null)
                        {
                            _batchFindCache.Remove(friendCtx);
                        }
                        else
                        {
                            friendCtx = new FriendContext(_client, invite.FriendInfo, true);
                        }
                        friendCtx.OnMyGiftsUpdated += OnPlayerMyGiftsUpdated;
                        _playerContexts.Add(friendCtx);
                        if(reinit) friendCtx.Reinit();
                        OnNewFriend(friendCtx);
                        OnFriendsUpdated();
                    }
                }
                if(invite.SendResult != InviteFriendResult.None)
                    _invites.Remove(invite);
                OnInvitesUpdated();
            }
        }

        private void _incomingInvite(FriendsInviteEventMessage evt)
        {
            var invite = new IncomingFriendInvite(_client, evt.InviteId, evt.FriendInfo, evt.ExpireTime);
            invite.OnAnswerSent += _inviteAnswered;
            _invites.Add(invite);
            OnInvite(invite);
            OnInvitesUpdated();
        }
    }
}
