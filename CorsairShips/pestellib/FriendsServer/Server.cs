using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FriendsClient;
using FriendsClient.Lobby;
using FriendsClient.Private;
using FriendsClient.Sources;
using FriendsClient.Sources.Serialization;
using FriendsServer.Bus;
using FriendsServer.Db;
using FriendsServer.Helpers;
using log4net;
using MessageClient;
using MessageClient.Sources;
using MessageServer;
using MessageServer.Server.Tcp;
using MessageServer.Sources;
using MessageServer.Sources.Tcp;
using PestelLib.ServerCommon.Extensions;
using PestelLib.ServerCommon.Threading;
using ServerShared.PlayerProfile;
using Microsoft.Extensions.Caching.Memory;
using S;
using ServerShared;
using UnityDI;

namespace FriendsServer
{
    class Ids
    {
        public MadId FriendId;
        public Guid PlayerId;

        public Ids(MadId friendId, Guid playerId)
        {
            FriendId = friendId;
            PlayerId = playerId;
        }
    }

    struct LockGuard : IDisposable
    {
        private readonly LockContext _ctx;

        public LockGuard(LockContext ctx)
        {
            _ctx = ctx;
        }

        public void Dispose()
        {
            _ctx.Unlock();
        }
    }

    class LockContext
    {
        private object _sync = new object();
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);
        private DateTime _lastAccess = DateTime.UtcNow;

        public TimeSpan Age => DateTime.UtcNow - _lastAccess;

        public async Task<LockGuard> LockAsync()
        {
            await _semaphore.WaitAsync();
            _lastAccess = DateTime.UtcNow;
            return new LockGuard(this);
        }

        public LockGuard Lock()
        {
            _semaphore.Wait();
            _lastAccess = DateTime.UtcNow;
            return new LockGuard(this);
        }

        public void Unlock()
        {
            _semaphore.Release();
        }
    }

    class IdLocker
    {
        private readonly ConcurrentDictionary<long, LockContext> _senderLocks = new ConcurrentDictionary<long, LockContext>();

        public IDisposable Lock(long key)
        {
            var ctx = _senderLocks.GetOrAdd(key, i => new LockContext());
            return ctx.Lock();
        }

        public async Task<IDisposable> LockAsync(long key)
        {
            var ctx = _senderLocks.GetOrAdd(key, i => new LockContext());
            return await ctx.LockAsync();
        }
    }

    partial class Server : DisposableLoop, IStatsProvider<MessageServerStats>
    {
#pragma warning disable 649
        [Dependency]
        private IFriendStorage _friendStorage;
        [Dependency]
        private IInvitationStorage _invitationStorage;
        [Dependency]
        private IProfileStorage _profileStorage;
        [Dependency]
        private IGiftStorage _giftStorage;
        [Dependency]
        private IRoomStorage _roomStorage;
        [Dependency]
        private IRoomInviteStorage _roomInviteStorage;
        [Dependency]
        private INonFriendsStatusWatch _nonFriendsStatusWatchStorage;
#pragma warning restore 649

        private static readonly ILog Log = LogManager.GetLogger(typeof(Server));
        private TcpMessageProvider _messageProvider;
        private IMessageSender _messageSender;
        private Dispatcher _dispatcher;
        private MessageServer.Server.MessageServer _messageServer;
        public MessageServerStats Stats => _messageProvider.Stats;
        private readonly ConcurrentDictionary<int, Ids> _authedSenders = new ConcurrentDictionary<int, Ids>();
        private readonly ConcurrentDictionary<uint, int> _madIdToSenderMap = new ConcurrentDictionary<uint, int>();
        private readonly ConcurrentQueue<FriendBase> _statusCheck = new ConcurrentQueue<FriendBase>();
        private IdLocker _senderLocker = new IdLocker();
        private IdLocker _friendsLocker = new IdLocker();
        private readonly ProfileWatcher _profileWatcher;
        private bool _initialized;
        private ServerConfig _config;
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public MessageServerStats GetStats() => Stats;
        public BaseMessageDispatcher.DispatcherStatistics DispatcherStatistics => _dispatcher.Stats;

        internal BaseMessageDispatcher MessageDispatcher => _dispatcher;

        public Server()
        {
            ContainerHolder.Container.BuildUp(this);
            _config = ServerConfigCache.Get();

            if (_config.MadIdMixed)
                MadId.Mode = MadIdMode.Mixed;

            _profileWatcher = new ProfileWatcher(_profileStorage, _config);
            _messageSender = _messageProvider = new TcpMessageProvider(_config.Port);
            _messageProvider.Start();
            _messageProvider.OnSenderDisconnect += _senderDisconnect;
            _dispatcher = new Dispatcher(this);
            _messageServer = new MessageServer.Server.MessageServer(_messageProvider, _dispatcher);
            InitBus();
            _initialized = true;
        }

        private void _senderDisconnect(int sender)
        {
            if (!_authedSenders.TryRemove(sender, out var v))
                return;
            using (_friendsLocker.Lock(v.FriendId))
            {
                if (!_madIdToSenderMap.TryGetValue(v.FriendId, out var s))
                    return;
                if (s != sender)
                {
                    Log.Debug($"Player disconnect duplicate socket {v.FriendId}:{v.PlayerId}. Id={sender}.");
                    return;
                }
                if (!_madIdToSenderMap.TryRemove(v.FriendId, out s))
                    return;
                Log.Debug($"Player disconnect {v.FriendId}:{v.PlayerId}. Id={sender}.");
                _profileWatcher.UnwatchProfile(v.FriendId);
                NotifyFriendsStatusChangedNoLock(v.FriendId, FriendStatus.Offline).ReportOnFail();
            }
            // leave room on disconnect
            _roomStorage.Get(v.FriendId, false).ContinueWith(_ =>
            {
                var room = _.Result;
                ChangeHostLeave(v.FriendId, room).GetAwaiter().GetResult();
            }).ReportOnFail();
        }

        private async Task<int[]> GetNotifyListForStatusChange(MadId changedId)
        {
            var room = await _roomStorage.Get(changedId, false);
            var friend = await _friendStorage.Get(changedId);
            var watchersGuids = await _nonFriendsStatusWatchStorage.GetWatchersOfObservable(friend.PlayerId);
            var watchersFriends = await _friendStorage.GetMany(watchersGuids);
            var notifyList = new HashSet<int>();
            if (room != null)
            {
                var roomParty = await GetRoomNotifyMadId(room, changedId);
                var roomNotifyList = MapFriendsToSenders(false, roomParty);
                foreach (var n in roomNotifyList)
                {
                    notifyList.Add(n);
                }
            }
            var invites = await _invitationStorage.GetOutgoing(changedId);
            var giftsAll = await _giftStorage.Get(changedId);
            var giftsTargets = giftsAll.Where(_ => _.From == changedId).Select(_ => _.To).ToArray();
            var friendsNotifyList = MapFriendsToSenders(false, friend
                .Friends
                .Union(invites.Select(_ => _.To))
                .Union(giftsTargets)
                .Union(watchersFriends.Select(_ => _.Id))
                .Distinct()
                .ToArray());
            foreach (var n in friendsNotifyList)
            {
                notifyList.Add(n);
            }

            return notifyList.ToArray();
        }

        private async Task NotifyFriendsStatusChangedNoLock(MadId id, int status, int? fromStatus = null)
        {
            var dt = DateTime.UtcNow;
            bool r;
            if (fromStatus == null)
                r = await _friendStorage.ChangeStatus(id, status);
            else
                r = await _friendStorage.CheckChangeStatus(id, fromStatus.Value, status);
            if (!r) return;

            _cache.Set(id, status, TimeSpan.FromMinutes(1));

            var notifyList = await GetNotifyListForStatusChange(id);
            var evt = new FriendStatusChangedMessage();
            evt.From = id;
            evt.StatusCode = status;
            evt.Event = FriendEvent.StatusChanged;
            evt.Time = dt;
            var data = FriendServerSerializer.Serialize(evt);
            _messageSender.Notify(notifyList.ToArray(), (int)evt.Event, data);
            if (_friendsBus.Enabled)
            {
                _friendsBus.NotifyStatusChange(evt);
            }
        }

        private async Task NotifyFriendsStatusChanged(MadId id, int status, int? fromStatus = null)
        {
            using (await _friendsLocker.LockAsync(id))
            {
                await NotifyFriendsStatusChangedNoLock(id, status, fromStatus);
            }
        }

        private async Task<List<FriendBase>> GetFriendInfos(MadId owner, params MadId[] ids)
        {
            var friends = await _friendStorage.GetMany(ids);
            var guids = friends.Select(_ => _.PlayerId).ToArray();
            var profiles = await _profileStorage.GetMany(guids);
            var gifts = await _giftStorage.GetAll(owner);
            return ConvertMany(friends, profiles, gifts).ToList();
        }

        private Task<byte> Ping(int sender, byte data)
        {
            return Task.FromResult((byte) 0);
        }

        private async Task<bool> SetNonFriendsWatched(int sender, NonFriendsWatchedRequest request)
        {
            var ids = MustBeAuthed(sender);
            await _nonFriendsStatusWatchStorage.SetWatch(ids.PlayerId, request.Observables);
            return true;
        }

        private async Task<FriendInitResponse> Init(int sender, FriendInitRequest request)
        {
            var result = new FriendInitResponse();
            Friend friend;
            using (await _senderLocker.LockAsync(sender))
            {
                friend = _friendStorage.GetOrCreate(request.PlayerId).Result;
            }

            using (await _friendsLocker.LockAsync(friend.Id))
            {
                if (_madIdToSenderMap.TryGetValue(friend.Id, out var existingConnection))
                {
                    Log.Warn($"Same user connections: {sender} (new), {existingConnection} (old). Old will be disconnected.");
                    _messageProvider.DisconnectSender(existingConnection);
                    _madIdToSenderMap.TryRemove(friend.Id, out var n);
                }

                var profile = await _profileStorage.Get(request.PlayerId);
                var inInvites = await _invitationStorage.GetIncoming(friend.Id);
                var outInvites = await _invitationStorage.GetOutgoing(friend.Id);
                var gifts = _giftStorage.Get(friend.Id).Result;

                var idsFromGifts = gifts.Where(_ => _.From != friend.Id).Select(_ => _.From);
                var allLinkedIds = friend.Friends.Union(inInvites.Select(_ => _.From))
                    .Union(outInvites.Select(_ => _.To)).Union(idsFromGifts).Distinct().ToArray();
                var allInfosRaw = await GetFriendInfos(friend.Id, allLinkedIds);
                var allInfosDict = allInfosRaw.ToDictionary(_ => _.Id);

                friend.Status = FriendStatus.Online;
                friend.LastStatus = DateTime.UtcNow;

                result.Code = FriendInitResult.Success;
                result.Info = Convert(friend, profile, default(DateTime));
                result.Gifts = new List<GiftDescription>();
                result.Config = _config.MakeSharedConfig();
                var freindsHash = new HashSet<MadId>(friend.Friends);
                foreach (var gift in gifts)
                {
                    var giftDesc = new GiftDescription()
                    {
                        Id = gift.Id,
                        From = gift.From,
                        To = gift.To,
                        GameSpecificId = gift.GameSpecificId,
                    };
                    // gift from non-friend
                    if (gift.From != friend.Id)
                    {
                        if (freindsHash.Contains(gift.From))
                            result.Gifts.Add(giftDesc);
                        else if (allInfosDict.TryGetValue(gift.From, out giftDesc.FriendInfo))
                            result.Gifts.Add(giftDesc);
                        // else friend removed from db
                    }
                }

                result.InvitedFriends = outInvites.Select(_ => _.To).ToList();
                result.PendingFriendInvites = inInvites.Where(_ => allInfosDict.ContainsKey(_.From)).Select(_ => new InviteFriendDescription()
                {
                    FromId = _.From,
                    InviteId = _.Id,
                    FriendInfo = allInfosDict[_.From],
                    ExpireTime = _.Expiry
                }).ToList();
                result.Friends = friend.Friends.Distinct().Where(_ => allInfosDict.ContainsKey(_)).Select(_ => allInfosDict[_]).ToList();

                var ids = new Ids(friend.Id, request.PlayerId);
                _authedSenders.AddOrUpdate(sender, ids, (i, tuple) => tuple);
                _madIdToSenderMap.AddOrUpdate(ids.FriendId, sender, (u, s) => s);
                Log.Debug($"Player connect {ids.FriendId}:{ids.PlayerId}. Id={sender}.");
                await NotifyFriendsStatusChangedNoLock(friend.Id, FriendStatus.Online);
                _profileWatcher.WatchProfile(result.Info);
                foreach (var info in allInfosRaw)
                    _statusCheck.Enqueue(info);
                return result;
            }
        }

        private async Task<FriendBaseResponse> FindFriend(int sender, FriendOpRequest request)
        {
            var ids = MustBeAuthed(sender);
            var result = new FriendBaseResponse();
            result.Friends = new List<FriendBase>();
            var friend = await _friendStorage.Get(request.FriendId);
            if (friend == null) return result;
            var profile = await _profileStorage.Get(friend.PlayerId);
            var lastGift = await _giftStorage.GetLastGift(ids.FriendId, request.FriendId);
            var lastGiftTime = lastGift?.CreateTime ?? default(DateTime);
            var friendBase = Convert(friend, profile, lastGiftTime);
            result.Friends.Add(friendBase);
            return result;
        }

        private async Task<FriendBaseResponse> FindFriends(int sender, FindFriendsRequest request)
        {
            var result = new FriendBaseResponse();
            var ids = MustBeAuthed(sender);
            var guids = request.PlayerIds;
            var friends = await _friendStorage.GetMany(guids);
            guids = friends.Select(_ => _.PlayerId).ToArray();
            var profiles = await _profileStorage.GetMany(guids);
            result.Friends = ConvertMany(friends, profiles, new Gift[] { }).ToList();
            return result;
        }

        private Task<bool> RemoveFriend(int sender, FriendOpRequest request)
        {
            var ids = MustBeAuthed(sender);
            return RemoveFriendInt(ids.FriendId, request.FriendId);
        }

        private async Task<bool> RemoveFriendInt(MadId who, MadId whom)
        {
            var result = await _friendStorage.RemoveFriend(who, whom);
            var evt = new FriendEventMessage();
            evt.Event = FriendEvent.FriendRemoved;
            evt.From = whom;
            var removed = 0;
            if (result)
            {
                ++removed;
                if (_madIdToSenderMap.TryGetValue(who, out var target))
                {
                    var data = FriendServerSerializer.Serialize(evt);
                    _messageSender.Notify(target, (int)evt.Event, data);
                }
            }

            result = await _friendStorage.RemoveFriend(whom, who);
            if (result)
            {
                ++removed;
                evt.From = who;
                if (_madIdToSenderMap.TryGetValue(whom, out var target))
                {
                    var data = FriendServerSerializer.Serialize(evt);
                    _messageSender.Notify(target, (int)evt.Event, data);
                }
                else if (_friendsBus.Enabled)
                {
                    _friendsBus.NotifyFriendRemoved(new FriendEventMessageGlobal
                    {
                        Target = whom,
                        Message = evt
                    });
                }
            }

            return removed == 2;
        }

        private async Task<FriendInviteResponse> FriendInvite(int sender, FriendOpRequest request)
        {
            var ids = MustBeAuthed(sender);
            var result = new FriendInviteResponse();
            if (ids.FriendId == request.FriendId)
            {
                result.Result = InviteFriendResult.SelfInvite;
                return result;
            }

            var me = await _friendStorage.Get(ids.FriendId);
            if(me.Friends.Length >= _config.MaxFriends)
            {
                result.Result = InviteFriendResult.MyLimit;
                return result;
            }
            
            var alreadyFriend = me.Friends.Any(_ => _ == request.FriendId);
            if (alreadyFriend)
            {
                result.Result = InviteFriendResult.AlreadyFriend;
                return result;
            }

            var counterInvite = await _invitationStorage.HasInvite(request.FriendId, ids.FriendId);
            if (counterInvite != null)
            {
                var acceptCounterInviteResult = await FriendInviteAccept(sender, new FriendInviteOpRequest() {Id = counterInvite.Id});
                if(acceptCounterInviteResult.Result == InviteFriendResult.Success)
                {
                    result.Result = InviteFriendResult.AlreadyFriend;
                    var evtNewFriend = new FriendEventMessage();
                    evtNewFriend.Event = FriendEvent.NewFriend;
                    evtNewFriend.From = request.FriendId;
                    var data = FriendServerSerializer.Serialize(evtNewFriend);
                    _messageSender.Notify(sender, (int)evtNewFriend.Event, data);
                    return result;
                }
            }

            var alreadyInvited = await _invitationStorage.HasInvite(ids.FriendId, request.FriendId);
            if (alreadyInvited != null)
            {
                result.Result = InviteFriendResult.AlreadySent;
                return result;
            }

            var friend = await _friendStorage.Get(request.FriendId);
            if (friend == null)
            {
                throw new FileNotFoundException($"{request.FriendId} not found unexpetedly. Request from {ids.FriendId}.");
            }

            if (friend.Friends.Length >= _config.MaxFriends)
            {
                result.Result = InviteFriendResult.OtherLimit;
                return result;
            }

            var friendIniteQueueSize = await _invitationStorage.CountIncoming(request.FriendId);
            if (friendIniteQueueSize >= _config.MaxInvites)
            {
                result.Result = InviteFriendResult.InviteLimit;
                return result;
            }

            var invite = await _invitationStorage.Create(ids.FriendId, request.FriendId, _config.InviteTTL);
            result.InviteId = invite.Id;
            result.Result = InviteFriendResult.Success;
            var fromFriend = await _friendStorage.Get(ids.FriendId);
            var profile = await _profileStorage.Get(ids.PlayerId);
            var gift = await _giftStorage.GetLastGift(request.FriendId, ids.FriendId);
            var lastGift = gift?.CreateTime ?? default(DateTime);
            var evt = new FriendsInviteEventMessage();
            evt.Event = FriendEvent.FriendInvite;
            evt.From = ids.FriendId;
            evt.InviteId = invite.Id;
            evt.FriendInfo = Convert(fromFriend, profile, lastGift);
            evt.ExpireTime = invite.Expiry;
            if (_madIdToSenderMap.TryGetValue(request.FriendId, out var target))
            {
                var data = FriendServerSerializer.Serialize(evt);
                _messageSender.Notify(target, (int)evt.Event, data);
            }
            else if(_friendsBus.Enabled)
            {
                _friendsBus.NotifyFriendInvite(new FriendsInviteEventGlobal
                {
                    Target = request.FriendId,
                    Message = evt
                });
            }

            _newBackgroundEvent.Set();
            return result;
        }

        private async Task<bool> FriendInviteCancel(int sender, FriendInviteOpRequest request)
        {
            var ids = MustBeAuthed(sender);
            var invite = await _invitationStorage.Get(request.Id);
            if (invite == null) return false;
            var r = await _invitationStorage.Remove(request.Id);
            if (!r) return false;

            var evt = new FriendsInviteEventMessage();
            evt.InviteId = invite.Id;
            evt.From = ids.FriendId;
            evt.Event = FriendEvent.FriendInviteCancel;
            // notify invite owner about cancelation if (s)he is online
            if (_madIdToSenderMap.TryGetValue(invite.To, out var toSender))
            {
                var data = FriendServerSerializer.Serialize(evt);
                _messageSender.Notify(toSender, (int)evt.Event, data);
            }
            else if (_friendsBus.Enabled)
            {
                _friendsBus.NotifyFriendInviteCanceled(new FriendsInviteEventGlobal
                {
                    Target = invite.To,
                    Message = evt
                });
            }

            return true;
        }

        private Task<FriendInviteResponse> FriendInviteAccept(int sender, FriendInviteOpRequest request)
        {
            return FriendInviteAnswer(sender, true, request);
        }

        private Task<FriendInviteResponse> FriendInviteReject(int sender, FriendInviteOpRequest request)
        {
            return FriendInviteAnswer(sender, false, request);
        }

        private async Task<FriendInviteResponse> FriendInviteAnswer(int sender, bool accept, FriendInviteOpRequest request)
        {
            var ids = MustBeAuthed(sender);
            var result = new FriendInviteResponse();
            result.InviteId = request.Id;
            var invite = await _invitationStorage.Get(request.Id);
            // invite not found
            if (invite == null)
            {
                Log.Warn($"Invite {request.Id} not found.");
                result.Result = InviteFriendResult.Error;
                return result;
            }
            var fromFriend = await _friendStorage.Get(invite.From);
            var toFriend = await _friendStorage.Get(invite.To);
            // on accept check friendlist limits
            if (accept)
            {
                if (fromFriend.Friends.Length >= _config.MaxFriends)
                {
                    result.Result = InviteFriendResult.OtherLimit;
                    return result;
                }

                if (toFriend.Friends.Length >= _config.MaxFriends)
                {
                    result.Result = InviteFriendResult.MyLimit;
                    return result;
                }
            }

            var listUpdated = !accept;
            // on accept we must modify friend lists of each player
            if (accept)
            {
                var s = listUpdated = await _friendStorage.AddFriend(invite.From, invite.To);
                if (s)
                {
                    listUpdated = await _friendStorage.AddFriend(invite.To, invite.From);
                    // rollback
                    if (!listUpdated)
                    {
                        Log.Warn($"AddFriend {invite.From} to {invite.To} failed.");
                        if (!await _friendStorage.RemoveFriend(invite.From, invite.To))
                        {
                            throw new Exception($"Restore consistency failed. invite.From={invite.From}, invite.To={invite.To}.");
                        }
                    }
                }
                else
                {
                    Log.Warn($"AddFriend {invite.To} to {invite.From} failed.");
                }
            }

            var removed = await _invitationStorage.Remove(request.Id);
            if (!removed)
                Log.Warn($"Remove invite {request.Id} failed.");
            var done = removed && listUpdated;
            if (!done)
            {
                // cant remove invite, try rollback friendlist changes
                if (accept && listUpdated)
                {
                    var s = await _friendStorage.RemoveFriend(invite.From, invite.To);
                    s &= await _friendStorage.RemoveFriend(invite.To, invite.From);
                    if (!s)
                    {
                        throw new Exception($"Restore consistency failed. invite.From={invite.From}, invite.To={invite.To}.");
                    }
                }
                result.Result = InviteFriendResult.Error;
                return result;
            }

            var evtType = accept ? FriendEvent.FriendAccept : FriendEvent.FriendReject;
            var type = (int)evtType;
            var evt = new FriendsInviteEventMessage();
            evt.InviteId = invite.Id;
            evt.From = ids.FriendId;
            evt.Event = evtType;
            if (accept)
            {
                var profile = await _profileStorage.Get(toFriend.PlayerId);
                var gift = await _giftStorage.GetLastGift(invite.From, ids.FriendId);
                var lastGift = gift?.CreateTime ?? default(DateTime);
                evt.FriendInfo = Convert(toFriend, profile, lastGift);
            }

            var data = FriendServerSerializer.Serialize(evt);

            // send notify to inviting player if (s)he is online
            if (_madIdToSenderMap.TryGetValue(invite.From, out var fromSender))
            {
                _messageSender.Notify(fromSender, type, data);
            }
            else if (_friendsBus.Enabled)
            {
                _friendsBus.NotifyFriendInviteAnswered(new FriendsInviteEventGlobal()
                {
                    Target = invite.From,
                    Message = evt
                });
            }

            result.Result = InviteFriendResult.Success;
            return result;
        }

        private async Task NotifyProfileChange(params FriendBase[] friendsBases)
        {
            if(friendsBases.Length == 0)
                return;
            var friends = await _friendStorage.GetMany(friendsBases.Select(_ => _.Id).ToArray());
            var baseMap = friendsBases.ToDictionary(_ => _.Id);
            foreach (var friend in friends)
            {
                try
                {
                    if (!baseMap.TryGetValue(friend.Id, out var fb))
                    {
                        Log.Error($"Friend {friend.Id} not found.");
                        continue;
                    }

                    var watchersGuids = await _nonFriendsStatusWatchStorage.GetWatchersOfObservable(friend.PlayerId);
                    var watchersFriends = await _friendStorage.GetMany(watchersGuids);
                    var room = await _roomStorage.Get(friend.Id, false);
                    var notifyList = friend.Friends.Union(watchersFriends.Select(_ => _.Id)).ToArray();
                    if (room != null)
                        notifyList = notifyList.Union(room.Party).Where(_ => _ != friend.Id).Distinct().ToArray();
                    if(notifyList.Length < 1)
                        continue;
                    var evt = new FriendsProfileUpdateMessage();
                    evt.Event = FriendEvent.ProfileUpdate;
                    evt.From = friend.Id;
                    evt.Profile = fb.Profile;
                    var targets = MapFriendsToSenders(false, notifyList);
                    if(targets.Length < 1)
                        continue;
                    var data = FriendServerSerializer.Serialize(evt);
                    _messageSender.Notify(targets, (int) evt.Event, data);
                }
                catch (Exception e)
                {
                    Log.Error($"Update profile {friend?.Id} notify failed.", e);
                }
            }
        }

        private Ids MustBeAuthed(int sender)
        {
            Ids result;
            if(!_authedSenders.TryGetValue(sender, out result))
            {
                throw new UnauthorizedAccessException($"{sender} not authed.");
            }
            return result;
        }

        private IEnumerable<FriendBase> ConvertMany(IEnumerable<Friend> friends, ProfileDTO[] profiles, Gift[] gifts)
        {
            var profileMap = profiles.ToDictionary(_ => _.PlayerId);
            var giftMap = gifts.GroupBy(_ => _.To).ToDictionary(_ => _.Key, _ => _.Max(m => m.CreateTime));
            foreach (var friend in friends)
            {
                ProfileDTO profile;
                profileMap.TryGetValue(friend.PlayerId, out profile);
                giftMap.TryGetValue(friend.Id, out var gift);
                yield return Convert(friend, profile, gift);
            }
        }

        private FriendBase Convert(Friend friend, ProfileDTO profile, DateTime lastGift)
        {
            var nextGift = lastGift == default(DateTime)
                ? default(DateTime)
                : GiftHelper.NextGift(lastGift);
            return new FriendBase()
            {
                Id = friend.Id,
                Status = friend.Status,
                LastStatus = friend.LastStatus,
                NextGift = nextGift,
                Profile = profile
            };
        }

        private async Task<FriendRoomResponse> CreateRoom(int sender, CreateFriendRoomRequest request)
        {
            var ids = MustBeAuthed(sender);
            var result = new FriendRoomResponse();
            var existingRoom = await _roomStorage.Get(ids.FriendId);
            // room already created
            if (existingRoom != null)
            {
                // requesting player is a host
                if (existingRoom.Host == ids.FriendId)
                    await ChangeRoomHost(existingRoom);
                else
                    await LeaveRoom(ids.FriendId, existingRoom);
            }

            _createRoom.WaitOne();
            try
            {
                var room = await _roomStorage.Create(ids.FriendId, request.AutoStartDelay, request.GameSpecificData);
                result.Result = RoomResult.Success;
                result.RoomId = room.Id;
                result.PartyLimit = _config.MaxRoomParty;
                _newBackgroundEvent.Set();
            }
            finally 
            {
                _createRoom.Set();
            }

            await NotifyFriendsStatusChanged(ids.FriendId, FriendStatus.InRoom);
            return result;
        }

        private async Task<bool> LeaveRoom(int sender, FriendRoomRequest request)
        {
            var ids = MustBeAuthed(sender);
            var room = await _roomStorage.Get(request.RoomId);
            if (room == null)
            {
                Log.Error($"Room {request.RoomId} not found.");
                return false;
            }

            return await ChangeHostLeave(ids.FriendId, room);
        }

        private async Task<bool> RoomUpdate(int sender, FriendRoomRequest request)
        {
            var ids = MustBeAuthed(sender);
            var room = await _roomStorage.Get(request.RoomId);
            if (room == null)
            {
                Log.Error($"Room {request.RoomId} not found.");
                return false;
            }

            room = await _roomStorage.Update(request.RoomId, request.GameSpecificData, room.RoomStatus);
            if (room == null)
            {
                Log.Error($"Room {request.RoomId} removed.");
                return false;
            }

            var notifyListRaw = await GetRoomNotifyMadId(room, ids.FriendId);
            var notifyList = MapFriendsToSenders(false, notifyListRaw);
            if (notifyList.Length == 0) return true;
            var evt = new RoomGameDataMessage();
            evt.Event = FriendEvent.RoomGameData;
            evt.RoomId = room.Id;
            evt.GameSpecificId = room.GameSpecificData;
            var data = FriendServerSerializer.Serialize(evt);
            _messageSender.Notify(notifyList, (int)evt.Event, data);
            return true;
        }

        private async Task<bool> ChangeHostLeave(MadId friend, Room room)
        {
            if (room == null) return false;
            if (room.Host == friend)
                await ChangeRoomHost(room);
            else
                await LeaveRoom(friend, room);
            return true;
        }

        private async Task LeaveRoom(MadId friend, Room room)
        {
            if (room.Party.All(_ => _ != friend))
            {
                Log.Debug($"Cant leave. User {friend} not in room {room.Id}.");
                return;
            }

            var notifyList = await GetRoomNotifyMadId(room, friend);
            await _roomStorage.LeaveRoom(room.Id, friend);

            var evt = new RoomLeaveKickEventMessage();
            evt.FriendId = friend;
            evt.RoomId = room.Id;
            evt.Event = FriendEvent.RoomLeave;
            var sendList = MapFriendsToSenders(false, notifyList);
            var data = FriendServerSerializer.Serialize(evt);
            await NotifyFriendsStatusChanged(friend, FriendStatus.Online, FriendStatus.InRoom);
            _messageSender.Notify(sendList, (int)evt.Event, data);
        }

        private async Task<FriendRoomResponse> KickRoom(int sender, FriendRoomRequest request)
        {
            var ids = MustBeAuthed(sender);
            var result = new FriendRoomResponse();
            result.RoomId = request.RoomId;
            var room = await _roomStorage.Get(request.RoomId);
            var notifyListRaw = await GetRoomNotifyMadId(room);
            var notifyList = MapFriendsToSenders(false, notifyListRaw);
            if (room.Host != ids.FriendId)
            {
                result.Result = RoomResult.NotAllowed;
                return result;
            }

            // self kick
            if (request.FriendId == ids.FriendId)
            {
                var r = await LeaveRoom(sender, request);
                result.Result = r ? RoomResult.Success : RoomResult.Error;
                return result;
            }

            var removed = await _roomStorage.LeaveRoom(request.RoomId, request.FriendId);
            result.Result = removed ? RoomResult.Success : RoomResult.Error;
            if (removed)
            {
                var evt = new RoomLeaveKickEventMessage();
                evt.Event = FriendEvent.RoomKick;
                evt.RoomId = request.RoomId;
                evt.FriendId = request.FriendId;
                var data = FriendServerSerializer.Serialize(evt);
                await NotifyFriendsStatusChanged(request.FriendId, FriendStatus.Online, FriendStatus.InRoom);
                _messageSender.Notify(notifyList, (int)evt.Event, data);
            }

            return result;
        }

        private async Task<FriendRoomInviteResponse> InviteRoom(int sender, FriendRoomInviteRequest request)
        {
            var ids = MustBeAuthed(sender);
            var result = new FriendRoomInviteResponse();
            result.RoomId = request.RoomId;
            var room = await _roomStorage.Get(request.RoomId);
            
            if (ids.FriendId != room.Host)
            {
                result.Code = RoomResult.NotAllowed;
                return result;
            }

            var alreadyInRoom = room.Party.Any(_ => _ == request.FriendId);
            if (alreadyInRoom)
            {
                result.Code = RoomResult.AlreadyJoined;
                return result;
            }

            if (room.RoomStatus != RoomStatus.Party)
            {
                result.Code = RoomResult.InvalidStatus;
                return result;
            }

            if (!_config.RoomInviteAnybody && !await _friendStorage.IsFriends(ids.FriendId, request.FriendId))
            {
                result.Code = RoomResult.NotFriend;
                return result;
            }

            if (room.Party.Count >= _config.MaxRoomParty)
            {
                result.Code = RoomResult.RoomIsFull;
                return result;
            }

            if (!_madIdToSenderMap.TryGetValue(request.FriendId, out var invitedTarget))
            {
                Log.Error($"Room {request.RoomId} joined player {request.FriendId} disconnected.");
                result.Code = RoomResult.UserOffline;
                return result;
            }

            var friend = await _friendStorage.Get(request.FriendId);
            if (friend.Status == FriendStatus.InRoom)
            {
                result.Code = RoomResult.UserBusy;
                return result;
            }

            var inviteExpiry = DateTime.UtcNow + _config.RoomInviteTTL;
            var inviteCreated = await _roomInviteStorage.Create(request.RoomId, request.FriendId, ids.FriendId, inviteExpiry);
            if (!inviteCreated)
            {
                // result.Code = RoomResult.AlreadyInvited;
                // return result;
                // we want to repeat invite in case of invites with large cooldown, maybe invited player lost his due to game restart
            }
            _newBackgroundEvent.Set();

            // room updates for invited friend
            if(_config.RoomUpdatesForInvited)
                await SendRoomInfo(room, invitedTarget);

            var evt = new RoomInviteEventMessage();
            evt.Event = FriendEvent.RoomInvite;
            evt.RoomId = request.RoomId;
            evt.Expiry = inviteExpiry;
            evt.From = ids.FriendId;
            var data = FriendServerSerializer.Serialize(evt);
            _messageSender.Notify(invitedTarget, (int)evt.Event, data);

            result.Code = RoomResult.Success;
            return result;
        }

        private async Task<FriendRoomResponse> RejectInviteRoom(int sender, FriendRoomRequest request)
        {
            var ids = MustBeAuthed(sender);
            var result = new FriendRoomResponse();
            result.RoomId = request.RoomId;
            var room = await _roomStorage.Get(request.RoomId);
            if (room == null)
            {
                result.Result = RoomResult.NoRoom;
                return result;
            }
            if (room.RoomStatus != RoomStatus.Party)
            {
                result.Result = RoomResult.InvalidStatus;
                return result;
            }
            var hasInvite = await _roomInviteStorage.Remove(request.RoomId, ids.FriendId, out var expiredInvite);
            if (!hasInvite)
            {
                result.Result = RoomResult.NotInvited;
                if (expiredInvite != null)
                    NotifyRoomInviteExpired(expiredInvite);
                return result;
            }

            byte[] data;
            if (_madIdToSenderMap.TryGetValue(room.Host, out var invitedByTarget))
            {
                var evt = new RoomInviteEventMessage();
                evt.Event = FriendEvent.RoomReject;
                evt.RoomId = request.RoomId;
                evt.From = ids.FriendId;
                data = FriendServerSerializer.Serialize(evt);
                _messageSender.Notify(invitedByTarget, (int)evt.Event, data);
            }

            result.Result = RoomResult.Success;
            return result;
        }

        private async Task<FriendRoomResponse> AcceptInviteRoom(int sender, FriendRoomRequest request)
        {
            var ids = MustBeAuthed(sender);
            var result = new FriendRoomResponse();
            result.RoomId = request.RoomId;
            var room = await _roomStorage.Get(request.RoomId);
            if (room == null)
            {
                result.Result = RoomResult.NoRoom;
                return result;
            }

            if (room.RoomStatus != RoomStatus.Party)
            {
                result.Result = RoomResult.InvalidStatus;
                return result;
            }
            var hasInvite = await _roomInviteStorage.Remove(request.RoomId, ids.FriendId, out var expiredInvite);
            if (!hasInvite)
            {
                result.Result = RoomResult.NotInvited;
                if(expiredInvite != null)
                    NotifyRoomInviteExpired(expiredInvite);
                return result;
            }

            if (room.Party.Count >= _config.MaxRoomParty)
            {
                result.Result = RoomResult.RoomIsFull;
                return result;
            }

            var currentRoom = await _roomStorage.Get(ids.FriendId, false);
            if(currentRoom != null)
                await ChangeHostLeave(ids.FriendId, currentRoom);
            var joined = await _roomStorage.JoinRoom(request.RoomId, ids.FriendId);
            var notifyListRaw = await GetRoomNotifyMadId(room);
            var notifyList = MapFriendsToSenders(false, notifyListRaw);
            if (!joined)
            {
                result.Result = RoomResult.AlreadyJoined;
                return result;
            }

            var friend = await _friendStorage.Get(ids.FriendId);
            if (friend.Status == FriendStatus.InRoom)
            {
                result.Result = RoomResult.UserBusy;
                return result;
            }

            await NotifyFriendsStatusChanged(ids.FriendId, FriendStatus.InRoom);
            byte[] data;
            // notify invite accepted to room host
            if (_madIdToSenderMap.TryGetValue(room.Host, out var invitedByTarget))
            {
                var accEvt = new RoomInviteEventMessage();
                accEvt.Event = FriendEvent.RoomAccept;
                accEvt.From = ids.FriendId;
                accEvt.RoomId = room.Id;
                data = FriendServerSerializer.Serialize(accEvt);
                _messageSender.Notify(invitedByTarget, (int)accEvt.Event, data);
            }

            // notify join to party
            var friendProfile = await _profileStorage.Get(friend.PlayerId);
            var joinEvt = new RoomJoinEventMessage();
            joinEvt.Event = FriendEvent.RoomJoin;
            joinEvt.RoomId = request.RoomId;
            joinEvt.Friend = Convert(friend, friendProfile, default(DateTime));
            joinEvt.Timeout = room.BattleStart;
            data = FriendServerSerializer.Serialize(joinEvt);
            _messageSender.Notify(notifyList, (int)joinEvt.Event, data);

            result.Result = RoomResult.Success;

            // room info notify to joined friend
            await SendRoomInfo(room, sender);

            return result;
        }

        private async Task SendRoomInfo(Room room, int dest)
        {
            var infoEvt = new RoomInfoEventMessage();
            infoEvt.Event = FriendEvent.RoomInfo;
            infoEvt.GameSpecificData = room.GameSpecificData;
            infoEvt.Host = room.Host;
            infoEvt.RoomId = room.Id;
            infoEvt.Timeout = room.BattleStart;
            infoEvt.PartyLimit = _config.MaxRoomParty;
            var friends = await _friendStorage.GetMany(room.Party.ToArray());
            var profiles = await _profileStorage.GetMany(friends.Select(_ => _.PlayerId).ToArray());
            infoEvt.Party = ConvertMany(friends, profiles, new Gift[] { }).ToArray();
            var data = FriendServerSerializer.Serialize(infoEvt);
            _messageSender.Notify(dest, (int)infoEvt.Event, data);
        }

        private async Task<FriendRoomResponse> StartBattle(int sender, FriendRoomRequest request)
        {
            var ids = MustBeAuthed(sender);
            var result = new FriendRoomResponse();
            result.RoomId = request.RoomId;
            var room = await _roomStorage.Get(request.RoomId);
            // only host starts battle
            if (room.Host != ids.FriendId)
            {
                result.Result = RoomResult.NotAllowed;
                return result;
            }
            // room is in invalid state
            if (room.RoomStatus != RoomStatus.Party)
            {
                result.Result = RoomResult.InvalidStatus;
                return result;
            }

            var newStatus = _config.DontCloseRoomOnBattleStart ? RoomStatus.Party : RoomStatus.Battle;
            room = await _roomStorage.Update(request.RoomId, request.GameSpecificData, newStatus);
            var noHostParty = room.Party.Where(_ => _ != room.Host).ToArray();
            var notifyList = MapFriendsToSenders(false, noHostParty);
            var evt = new RoomInfoEventMessage();
            evt.GameSpecificData = room.GameSpecificData;
            evt.Event = FriendEvent.RoomStartBattle;
            evt.Host = room.Host;
            evt.RoomId = room.Id;
            evt.Timeout = DateTime.UtcNow + _config.StartBattleDelay;
            var friends = await _friendStorage.GetMany(room.Party.ToArray());
            var profiles = await _profileStorage.GetMany(friends.Select(_ => _.PlayerId).ToArray());
            evt.Party = ConvertMany(friends, profiles, new Gift[] { }).ToArray();
            var data = FriendServerSerializer.Serialize(evt);
            _messageSender.Notify(notifyList, (int)evt.Event, data);
            result.Result = RoomResult.Success;
            return result;
        }

        private int[] MapFriendsToSenders(bool exactMatch, params MadId[] ids)
        {
            var result = new List<int>();
            foreach (var id in ids)
            {
                if (_madIdToSenderMap.TryGetValue(id, out var sender))
                {
                    result.Add(sender);
                }
                else
                {
                    if(exactMatch) throw new Exception($"Friend {id} not connected.");
                }
            }

            return result.ToArray();
        }

        private async Task<MadId[]> GetRoomNotifyMadId(Room room, params MadId[] exclude)
        {
            MadId[] fromInvites;
            if (_config.RoomUpdatesForInvited)
                fromInvites = (await _roomInviteStorage.GetRoomInvites(room.Id, false)).Select(_ => _.Invited).ToArray();
            else
                fromInvites = new MadId[] { };
            var notifyList = room.Party.Union(fromInvites).Where(_ => !exclude.Contains(_)).Distinct().ToArray();
            return notifyList;
        }

        private async Task ChangeRoomHost(Room room)
        {
            var host = room.Host;
            var notifyList = await GetRoomNotifyMadId(room, room.Host);
            await LeaveRoom(host, room);
            var roomRemoved = false;
            if (room.Party.Count == 0)
            {
                roomRemoved = await _roomStorage.Remove(room.Id);
            }

            if (_madIdToSenderMap.TryGetValue(host, out var target))
            {
                var kickEvt = new RoomLeaveKickEventMessage();
                kickEvt.RoomId = room.Id;
                kickEvt.Event = FriendEvent.RoomKick;
                kickEvt.FriendId = host;
                var data = FriendServerSerializer.Serialize(kickEvt);
                _messageSender.Notify(target, (int) kickEvt.Event, data);
            }

            if(roomRemoved) return;
            // notify party about new host
            {
                var newHost = room.Party.First();
                room = await _roomStorage.ChangeHost(room.Id, newHost);
                var evt = new RoomChangeHostEventMessage();
                evt.Event = FriendEvent.RoomHostChange;
                evt.RoomId = room.Id;
                evt.NewHost = newHost;
                evt.Timeout = room.BattleStart;
                var sendList = MapFriendsToSenders(false, notifyList);
                var data = FriendServerSerializer.Serialize(evt);
                _messageSender.Notify(sendList, (int) evt.Event, data);
            }
        }

        private async Task<bool> FriendStatusChange(int sender, FriendStatusNotify request)
        {
            var ids = MustBeAuthed(sender);
            await NotifyFriendsStatusChanged(ids.FriendId, request.Status);
            return true;
        }

        private async Task<FriendGiftResponse> SendGift(int sender, FriendGiftRequest request)
        {
            var ids = MustBeAuthed(sender);
            var result = new FriendGiftResponse();
            if (!await _friendStorage.IsFriends(ids.FriendId, request.FriendId))
            {
                result.Result = GiftResult.NotFriend;
                return result;
            }

            if (_config.GiftsStackSize > 0)
            {
                var unclaimedGiftsCount = await _giftStorage.CountGifts(ids.FriendId, request.FriendId, false);
                if (unclaimedGiftsCount >= _config.GiftsStackSize)
                {
                    result.Result = GiftResult.StackLimit;
                    return result;
                }
            }

            var lastGift = await _giftStorage.GetLastGift(ids.FriendId, request.FriendId);
            var canSend = lastGift == null || GiftHelper.NextGift(lastGift.CreateTime) <= DateTime.UtcNow;
            if (!canSend)
            {
                result.Result = GiftResult.NotAllowed;
                return result;
            }

            var gift = await _giftStorage.Create(ids.FriendId, request.FriendId, request.GameSpecificId);
            result.Result = GiftResult.Success;
            result.GiftId = gift.Id;
            result.NextGift = GiftHelper.NextGift(gift.CreateTime);

            var evt = new FriendGiftEventMessage();
            evt.From = ids.FriendId;
            evt.Event = FriendEvent.Gift;
            evt.GiftId = gift.Id;
            evt.GameSpecificId = request.GameSpecificId;
            var data = FriendServerSerializer.Serialize(evt);

            if (_madIdToSenderMap.TryGetValue(request.FriendId, out int target))
            {
                _messageSender.Notify(target, (int)evt.Event, data);
            }
            else if (_friendsBus.Enabled)
            {
                _friendsBus.NotifyGiftSent(new FriendGiftEventMessageGlobal
                {
                    Target = request.FriendId,
                    Message = evt
                });
            }

            return result;
        }

        private async Task<GiftResult> ClaimGift(long giftId, MadId giftClaimer)
        {
            var r = await _giftStorage.ClaimGift(giftId, giftClaimer);
            if (r == null)
            {
                return GiftResult.NoGift;
            }
            var evt = new FriendGiftEventMessage();
            evt.From = giftClaimer;
            evt.Event = FriendEvent.GiftClaimed;
            evt.GiftId = r.Id;
            evt.GameSpecificId = r.GameSpecificId;
            if (_madIdToSenderMap.TryGetValue(r.From, out var target))
            {
                var data = FriendServerSerializer.Serialize(evt);
                _messageSender.Notify(target, (int)evt.Event, data);
            }
            else if (_friendsBus.Enabled)
            {
                _friendsBus.NotifyGiftClaim(new FriendGiftEventMessageGlobal
                {
                    Target = r.From,
                    Message = evt
                });
            }

            return GiftResult.Success;
        }

        private async Task<FriendGiftResponse> ClaimGifts(int sender, FriendGiftRequest request)
        {
            var ids = MustBeAuthed(sender);
            var result = new FriendGiftResponse();
            result.BatchResult = new GiftResult[request.ClaimGiftIds.Length];
            for (var i = 0; i < request.ClaimGiftIds.Length; ++i)
            {
                result.BatchResult[i] = await ClaimGift(request.ClaimGiftIds[i], ids.FriendId);
            }

            return result;
        }

        private async Task<FriendGiftResponse> GetGiftClaimStatus(int sender, FriendGiftRequest request)
        {
            var ids = MustBeAuthed(sender);
            var result = new FriendGiftResponse();
            result.BatchResult = new GiftResult[request.ClaimGiftIds.Length];
            var gifts = await _giftStorage.GetById(request.ClaimGiftIds);
            var giftToStateMap = new Dictionary<long, GiftResult>();
            foreach (var gift in gifts)
            {
                giftToStateMap[gift.Id] = gift.IsClaimed ? GiftResult.Success : GiftResult.None;
            }
            for (var i = 0; i < request.ClaimGiftIds.Length; ++i)
            {
                var gid = request.ClaimGiftIds[i];
                if (giftToStateMap.TryGetValue(gid, out var state))
                    result.BatchResult[i] = state;
                else
                    result.BatchResult[i] = GiftResult.NoGift;
            }
            return result;
        }

        private async Task<FriendGiftResponse> ClaimGift(int sender, FriendGiftRequest request)
        {
            var ids = MustBeAuthed(sender);
            var result = new FriendGiftResponse();
            result.Result = await ClaimGift(request.GiftId, ids.FriendId);

            return result;
        }

        class Dispatcher : MtMessageDispatcher
        {
            private readonly Server _server;
            private FriendsUniversalSerializerImpl _serializer;

            public Dispatcher(Server server)
            {
                _server = server;
                _serializer = new FriendsUniversalSerializerImpl();

                // requests
                RegisterHandler(0, new AsyncMessageHandler<byte, byte>(_serializer, _server.Ping));
                RegisterHandler((int)FriendRequest.Init, new AsyncMessageHandler<FriendInitRequest, FriendInitResponse>(_serializer, _server.Init));
                RegisterHandler((int)FriendRequest.FindFriend, new AsyncMessageHandler<FriendOpRequest, FriendBaseResponse>(_serializer, _server.FindFriend));
                RegisterHandler((int)FriendRequest.FindFriendsByPlayerId, new AsyncMessageHandler<FindFriendsRequest, FriendBaseResponse>(_serializer, _server.FindFriends));
                RegisterHandler((int)FriendRequest.FriendInvite, new AsyncMessageHandler<FriendOpRequest, FriendInviteResponse>(_serializer, _server.FriendInvite));
                RegisterHandler((int)FriendRequest.FriendInviteAccept, new AsyncMessageHandler<FriendInviteOpRequest, FriendInviteResponse>(_serializer, _server.FriendInviteAccept));
                RegisterHandler((int)FriendRequest.FriendInviteReject, new AsyncMessageHandler<FriendInviteOpRequest, FriendInviteResponse>(_serializer, _server.FriendInviteReject));
                RegisterHandler((int)FriendRequest.RoomCreate, new AsyncMessageHandler<CreateFriendRoomRequest, FriendRoomResponse>(_serializer, _server.CreateRoom));
                RegisterHandler((int)FriendRequest.RoomKick, new AsyncMessageHandler<FriendRoomRequest,FriendRoomResponse>(_serializer, _server.KickRoom));
                RegisterHandler((int)FriendRequest.RoomInvite, new AsyncMessageHandler<FriendRoomInviteRequest, FriendRoomInviteResponse>(_serializer, _server.InviteRoom));
                RegisterHandler((int)FriendRequest.RoomInviteAccept, new AsyncMessageHandler<FriendRoomRequest, FriendRoomResponse>(_serializer, _server.AcceptInviteRoom));
                RegisterHandler((int)FriendRequest.RoomInviteReject, new AsyncMessageHandler<FriendRoomRequest, FriendRoomResponse>(_serializer, _server.RejectInviteRoom));
                RegisterHandler((int)FriendRequest.RoomStartBattle, new AsyncMessageHandler<FriendRoomRequest, FriendRoomResponse>(_serializer, _server.StartBattle));
                RegisterHandler((int)FriendRequest.GiftSend, new AsyncMessageHandler<FriendGiftRequest, FriendGiftResponse>(_serializer, _server.SendGift));
                RegisterHandler((int)FriendRequest.GiftClaim, new AsyncMessageHandler<FriendGiftRequest, FriendGiftResponse>(_serializer, _server.ClaimGift));
                RegisterHandler((int)FriendRequest.GiftsClaim, new AsyncMessageHandler<FriendGiftRequest, FriendGiftResponse>(_serializer, _server.ClaimGifts));
                RegisterHandler((int)FriendRequest.GiftsClaimStatus, new AsyncMessageHandler<FriendGiftRequest, FriendGiftResponse>(_serializer, _server.GetGiftClaimStatus));
                // notifies
                RegisterHandler((int)FriendRequest.FriendInviteCancel, new AsyncMessageHandler<FriendInviteOpRequest, bool>(_serializer, _server.FriendInviteCancel));
                RegisterHandler((int)FriendRequest.RoomLeave, new AsyncMessageHandler<FriendRoomRequest, bool>(_serializer, _server.LeaveRoom));
                RegisterHandler((int)FriendRequest.StatusChange, new AsyncMessageHandler<FriendStatusNotify, bool>(_serializer, _server.FriendStatusChange));
                RegisterHandler((int)FriendRequest.RemoveFriend, new AsyncMessageHandler<FriendOpRequest, bool>(_serializer, _server.RemoveFriend));
                RegisterHandler((int)FriendRequest.RoomUpdate, new AsyncMessageHandler<FriendRoomRequest, bool>(_serializer, _server.RoomUpdate));
                RegisterHandler((int)FriendRequest.SetNonFriendsStatusWatch, new AsyncMessageHandler<NonFriendsWatchedRequest, bool>(_serializer, server.SetNonFriendsWatched));
            }
        }

        public override void Dispose()
        {
            DeinitBus();
            _messageProvider?.Dispose();
            _messageServer?.Dispose();
            _profileWatcher.Dispose();

            _messageProvider = null;
            _messageServer = null;
            base.Dispose();
        }

        private void NotifyRoomInviteExpired(RoomInvite roomInvite)
        {
            var msgSource = new RoomInviteEventMessage();
            var msgTarget = new RoomInviteEventMessage();
            msgSource.Event = FriendEvent.RoomAutoReject;
            msgTarget.Event = FriendEvent.RoomAutoReject;

            msgSource.From = roomInvite.Invited;
            msgSource.RoomId = roomInvite.RoomId;
            msgSource.Expiry = roomInvite.Expiry;
            var dest = MapFriendsToSenders(false, roomInvite.InvitedBy);
            var data = FriendServerSerializer.Serialize(msgSource);
            _messageSender.Notify(dest, (int)msgSource.Event, data);

            msgTarget.From = roomInvite.InvitedBy;
            msgTarget.RoomId = roomInvite.RoomId;
            msgTarget.Expiry = roomInvite.Expiry;
            dest = MapFriendsToSenders(false, roomInvite.Invited);
            data = FriendServerSerializer.Serialize(msgTarget);
            _messageSender.Notify(dest, (int)msgTarget.Event, data);
        }

        private AutoResetEvent _createRoom = new AutoResetEvent(true);
        private AutoResetEvent _newBackgroundEvent = new AutoResetEvent(false);
        private DateTime _nextInviteExpireTime = DateTime.MinValue;
        protected override void Update(CancellationToken cancellationToken)
        {
            SpinWait.SpinUntil(() => _initialized);
            List<FriendBase> updatedProfiles = _profileWatcher.Update().Result;

            if (updatedProfiles?.Count > 0)
            {
                Log.Debug($"Profiles updated: {updatedProfiles?.Count}.");
                NotifyProfileChange(updatedProfiles.ToArray()).GetAwaiter().GetResult();
            }

            while(_statusCheck.TryDequeue(out var checkStatus))
            {
                if(_cache.TryGetValue<int>(checkStatus.Id, out var status) && status != checkStatus.Status)
                {
                    Log.Debug($"Status change checker. {checkStatus.Id} change status {checkStatus.Status}->{status}.");
                    NotifyFriendsStatusChanged(checkStatus.Id, status).ReportOnFail();
                }
            }

            // sleep if there are not data to process
            if (_roomStorage.Count().Result == 0 && _invitationStorage.Count().Result == 0 && _roomInviteStorage.Count() == 0)
                if (!_newBackgroundEvent.WaitOne(1000))
                    return;

            var expired = _roomStorage.GetExpired().Result;
            if (expired.Length > 0)
            {
                foreach (var room in expired)
                {
                    ChangeRoomHost(room).ReportOnFail();
                }
            }

            // dont flood db with queries
            if (DateTime.UtcNow > _nextInviteExpireTime)
            {
                const int amount = 10;
                var expiredInvites = _invitationStorage.GetExpired(amount).Result;
                MadId[] sendList = new MadId[2];
                while (expiredInvites.Length > 0)
                {
                    for (var i = 0; i < expiredInvites.Length; ++i)
                    {
                        var expiredInvite = expiredInvites[i];
                        sendList[0] = expiredInvite.To;
                        sendList[1] = expiredInvite.From;
                        var targets = MapFriendsToSenders(false, sendList);
                        if (targets.Length > 0)
                        {
                            var evt = new FriendsInviteEventMessage();
                            evt.Event = FriendEvent.FriendInviteExpired;
                            evt.InviteId = expiredInvite.Id;
                            var data = FriendServerSerializer.Serialize(evt);
                            _messageSender.Notify(targets, (int) evt.Event, data);
                        }
                    }

                    var removeIds = expiredInvites.Select(_ => _.Id).ToArray();
                    var removed = _invitationStorage.RemoveMany(removeIds).GetAwaiter().GetResult();
                    if (removed != removeIds.Length)
                    {
                        Log.Warn($"{removed} invites removed. Expected {removeIds.Length}.");
                    }

                    expiredInvites = _invitationStorage.GetExpired(amount).Result;
                }

                if (expiredInvites.Length == 0)
                {
                    var inv = _invitationStorage.GetCloseToExpirInvitation().Result;
                    _nextInviteExpireTime = inv?.Expiry ?? DateTime.MinValue;
                }
            }

            if (_roomInviteStorage.HasExpired())
            {
                var roomInvExp = _roomInviteStorage.GetExpired();
                foreach (var roomInvite in roomInvExp)
                {
                    _roomInviteStorage.Remove(roomInvite.RoomId, roomInvite.Invited, out var inv);
                    if (inv != null)
                    {
                        NotifyRoomInviteExpired(inv);
                    }
                }
            }

            Thread.Sleep(100);
        }
    }
}
