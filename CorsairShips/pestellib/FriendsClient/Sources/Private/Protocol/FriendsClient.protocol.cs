using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FriendsClient.FriendList;
using FriendsClient.Sources;
using FriendsClient.Sources.Serialization;
using MessagePack;
using S;
using ServerShared.PlayerProfile;
using ServerShared.Sources.Numeric;

namespace FriendsClient.Private
{
    public enum FriendInitResult
    {
        None,
        Success,
        InvalidAuth
    }

    public enum InviteFriendResult
    {
        None,
        Success,        // invite sent.
        MyLimit,        // my friends list reached cap MaxFriends.
        OtherLimit,     // target player's friends list reached cap MaxFriends.
        InviteLimit,    // target player's pending invites list reached limit MaxInvites.
        AlreadyFriend,  // player already accepted your invite erlier.
        /// <summary>
        /// invite already sent (get invitation status through <see cref="IFriendContext.InviteContext"/>)
        /// </summary>
        AlreadySent,
        SelfInvite,
        Error           // connection error.
    }

    public enum RoomResult
    {
        None,
        Success,
        /// <summary>
        /// Battle already started or all players had left the room.
        /// </summary>
        NoRoom,
        /// <summary>
        /// Invitations are not allowed. It's about to start battle.
        /// </summary>
        RoomTimeout,
        /// <summary>
        /// Can't invite player if it's not friend-listed.
        /// </summary>
        NotFriend,
        /// <summary>
        /// Can't perform host operation while not beeing a host (invite management, battle start, etc.).
        /// </summary>
        NotAllowed,
        /// <summary>
        /// Can't answer invites when not invited.
        /// </summary>
        NotInvited,
        /// <summary>
        /// Request cant be performed because of target's invalid status.
        /// </summary>
        InvalidStatus,
        AlreadyJoined,
        AlreadyInvited,
        UserOffline,
        RoomIsFull,
        UserBusy,
        Error
    }

    public enum GiftResult
    {
        None,
        /// <summary>
        /// Operation completed.
        /// </summary>
        Success,
        /// <summary>
        /// Cant send gift in cooldown. (check <see cref="FriendBase.NextGift"/>).
        /// </summary>
        NotAllowed,
        /// <summary>
        /// Cant send gift to non friend.
        /// </summary>
        NotFriend,
        /// <summary>
        /// Gift already claimed.
        /// </summary>
        NoGift,
        /// <summary>
        /// Propably connection error.
        /// </summary>
        Error,
        /// <summary>
        /// Limit of unclaimed gifts reached (<see cref="FriendInitResponse.GiftsStackSize"/>).
        /// </summary>
        StackLimit,
        /// <summary>
        /// Gift claiming is in the progress. (duplicated request)
        /// </summary>
        ClaimInProgress,
        /// <summary>
        /// Gift already claimed by another request.
        /// </summary>
        AlreadyClaimed
    }

    // invite, accept, reject
    [MessagePackObject()]
    public class FriendsInviteEventMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public MadId From;
        [Key(2)]
        public long InviteId;
        [Key(3)]
        public FriendBase FriendInfo;
        [Key(4)]
        public DateTime ExpireTime;
    }

    [MessagePackObject()]
    public class FriendsProfileUpdateMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public MadId From;
        [Key(2)]
        public ProfileDTO Profile;
    }

    // eg. remove from friends
    [MessagePackObject()]
    public class FriendEventMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public MadId From;
    }

    /// <summary>
    /// Invite owner cancels invitation. Sent from owner to invited player.
    /// </summary>
    [MessagePackObject()]
    public class FriendsInviteCancelEventMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public MadId From;
        [Key(2)]
        public long InviteId;
    }

    [MessagePackObject()]
    public class FriendStatusChangedMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public MadId From;
        [Key(2)]
        public int StatusCode;
        [Key(3)]
        public DateTime Time;
    }

    [MessagePackObject()]
    public class FriendGiftEventMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public MadId From;
        [Key(2)]
        public int GameSpecificId;
        [Key(3)]
        public long GiftId;
    }

    [MessagePackObject()]
    public class RoomInviteEventMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public MadId From;
        [Key(2)]
        public long RoomId;
        [Key(3)]
        public DateTime Expiry;
    }

    [MessagePackObject()]
    public class RoomGameDataMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public long RoomId;
        [Key(2)]
        public string GameSpecificId;
    }

    [MessagePackObject()]
    public class RoomHostChangeEventMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public long InviteId;
        [Key(2)]
        public MadId OldHost;
        [Key(3)]
        public MadId NewHost;
        /// <summary>
        /// Same delay was passed to CreateRoom for the room.
        /// </summary>
        [Key(4)]
        public TimeSpan AutostartDelay;
    }

    /// <summary>
    /// All room members receive joined friend info.
    /// </summary>
    [MessagePackObject()]
    public class RoomJoinEventMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public long RoomId;
        /// <summary>
        /// Joined friend.
        /// </summary>
        [Key(2)]
        public FriendBase Friend;
        [Key(3)]
        public DateTime Timeout;
    }

    /// <summary>
    /// Player leaves the room or kicked by the host. Which one reason had happend see in RoomLeaveKickEventMessage.Event.
    /// </summary>
    [MessagePackObject()]
    public class RoomLeaveKickEventMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(2)]
        public long RoomId;
        [Key(3)]
        public MadId FriendId;
    }

    [MessagePackObject()]
    public class RoomStartBattleEventMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public long RoomId;
        /// <summary>
        /// Game specific data for joining to the battle.
        /// </summary>
        [Key(2)]
        public string GameData;
    }

    /// <summary>
    /// Room info sent once for every joined player.
    /// </summary>
    [MessagePackObject()]
    public class RoomInfoEventMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public long RoomId;
        [Key(2)]
        public string GameSpecificData;
        [Key(3)]
        public DateTime Timeout;
        [Key(4)]
        public FriendBase[] Party;
        /// <summary>
        /// ID of hosting player.
        /// </summary>
        [Key(5)]
        public MadId Host;
        [Key(6)]
        public int PartyLimit;
    }

    /// <summary>
    /// Change host requested from the server.
    /// </summary>
    [MessagePackObject()]
    public class RoomChangeHostEventMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public long RoomId;
        [Key(2)]
        public MadId NewHost;
        [Key(3)]
        public DateTime Timeout;
    }

    /// <summary>
    /// Server updates start match timer.
    /// </summary>
    [MessagePackObject()]
    public class RoomCountdownEventMessage
    {
        [Key(0)]
        public FriendEvent Event;
        [Key(1)]
        public long RoomId;
        [Key(2)]
        public DateTime Timeout;
    }

    // Server requests
    [MessagePackObject()]
    public struct FriendRoomInviteRequest
    {
        [Key(0)]
        public MadId SenderId;
        [Key(1)]
        public MadId FriendId;
        [Key(2)]
        public long RoomId;
    }

    [MessagePackObject()]
    public class FriendInitRequest
    {
        [Key(0)]
        public Guid PlayerId;
        [Key(1)]
        public string AuthData;
    }

    [MessagePackObject()]
    public class NonFriendsWatchedRequest
    {
        [Key(0)] 
        public Guid[] Observables;
    }

    [MessagePackObject()]
    public class FriendOpRequest
    {
        [Key(0)] public MadId FriendId;
    }

    [MessagePackObject()]
    public class FindFriendsRequest
    {
        [Key(0)] public Guid[] PlayerIds;
    }

    [MessagePackObject()]
    public class FriendInviteOpRequest
    {
        [Key(0)] public long Id;
    }

    [MessagePackObject()]
    public class CreateFriendRoomRequest
    {
        [Key(0)]
        public TimeSpan AutoStartDelay;
        [Key(1)]
        public string GameSpecificData;
    }

    [MessagePackObject()]
    public class FriendRoomRequest
    {
        [Key(0)]
        public long RoomId;
        [Key(1)]
        public MadId FriendId;
        [Key(2)]
        public string GameSpecificData;
    }

    [MessagePackObject()]
    public class FriendRoomResponse
    {
        [Key(0)]
        public long RoomId;
        [Key(1)]
        public RoomResult Result;
        [Key(2)]
        public int PartyLimit;
    }

    [MessagePackObject()]
    public class FriendBaseResponse
    {
        [Key(0)]
        public List<FriendBase> Friends;
    }

    [MessagePackObject()]
    public class FriendInviteResponse
    {
        [Key(0)]
        public long InviteId;
        [Key(1)]
        public InviteFriendResult Result;
    }

    [MessagePackObject()]
    public class InviteFriendDescription
    {
        [Key(0)] public long InviteId;
        [Key(1)] public MadId FromId;
        [Key(2)] public FriendBase FriendInfo;
        [Key(3)] public DateTime ExpireTime;
    }

    [MessagePackObject()]
    public class GiftDescription
    {
        [Key(0)] public MadId From;
        [Key(1)] public MadId To;
        [Key(2)] public int GameSpecificId;
        [Key(3)] public long Id;
        [Key(4)] public FriendBase FriendInfo;
    }

    [MessagePackObject()]
    public class FriendInitResponse
    {
        [Key(0)]
        public FriendInitResult Code;
        [Key(1)]
        public FriendBase Info;
        [Key(2)]
        public List<MadId> InvitedFriends;
        [Key(3)]
        public List<FriendBase> Friends;
        [Key(4)]
        public List<InviteFriendDescription> PendingFriendInvites;
        [Key(5)]
        public List<GiftDescription> Gifts;
        //[Key(6)]
        //[Key(7)]
        //[Key(8)]
        [Key(9)]
        public SharedConfig Config;
    }

    /// <summary>
    /// Sender notifies about his status change.
    /// </summary>
    [MessagePackObject()]
    public class FriendStatusNotify
    {
        [Key(0)]
        public int Status;
    }

    [MessagePackObject()]
    public class FriendGiftRequest
    {
        [Key(0)]
        public MadId FriendId;
        [Key(1)]
        public int GameSpecificId;
        [Key(2)]
        public long GiftId;
        [Key(3)]
        public long[] ClaimGiftIds;
    }

    [MessagePackObject()]
    public class FriendGiftResponse
    {
        [Key(0)]
        public GiftResult Result;
        [Key(1)]
        public long GiftId;
        [Key(2)]
        public DateTime NextGift;
        [Key(3)]
        public GiftResult[] BatchResult;
    }

    /// <summary>
    /// Server response to invite room.
    /// </summary>
    [MessagePackObject()]
    public class FriendRoomInviteResponse
    {
        [Key(0)]
        public long RoomId;
        [Key(1)]
        public RoomResult Code;
    }

    public partial class FriendsClient
    {
        private int Server = 1;
        private FriendsServerAnswerToCallback<FriendRoomInviteResponse> _lobbyInviteResponse = new FriendsServerAnswerToCallback<FriendRoomInviteResponse>();
        private FriendsServerAnswerToCallback<FriendInitResponse> _initResponse = new FriendsServerAnswerToCallback<FriendInitResponse>();
        private FriendsServerAnswerToCallback<FriendBaseResponse> _friendBaseResponse = new FriendsServerAnswerToCallback<FriendBaseResponse>();
        private FriendsServerAnswerToCallback<FriendInviteResponse> _friendInviteResponse = new FriendsServerAnswerToCallback<FriendInviteResponse>();
        private FriendsServerAnswerToCallback<FriendRoomResponse> _roomResponse = new FriendsServerAnswerToCallback<FriendRoomResponse>();
        private FriendsServerAnswerToCallback<FriendGiftResponse> _giftResponse = new FriendsServerAnswerToCallback<FriendGiftResponse>();

        private void Deinit()
        {
            Initialized = false;
            if(_initAwaiter.Task.IsCompleted)
                _initAwaiter = new TaskCompletionSource<bool>();
        }

        private void Init()
        {
            var msg = new FriendInitRequest();
            msg.PlayerId = _playerId;
            msg.AuthData = _authData;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)FriendRequest.Init, data, _initResponse);
            _initResponse.RegisterCallback(tag, _initDone);
        }

        public void RemoveFriend(MadId friendId, FriendsDelegate.FriendsClientCallback callback)
        {
            var msg = new FriendOpRequest();
            msg.FriendId = friendId;
            var data = FriendServerSerializer.Serialize(msg);
            _messageSender.Notify(Server, (int)FriendRequest.RemoveFriend, data);
            if (callback != null)
                callback(_messageSender.IsValid);
        }

        public void FindFriend(MadId id, FriendsDelegate.FindFriendCallback callback)
        {
            var existingFriend = FriendList.Friends.FirstOrDefault(_ => _.FriendInfo.Id == id);
            if (existingFriend != null)
            {
                callback(existingFriend.FriendInfo);
                return;
            }

            var msg = new FriendOpRequest();
            msg.FriendId = id;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)FriendRequest.FindFriend, data, _friendBaseResponse);
            _friendBaseResponse.RegisterCallback(tag, r =>
            {
                if (r.Friends == null || r.Friends.Count < 1)
                    callback(null);
                else
                    callback(r.Friends[0]);
            });
        }

        public void FindFriends(Guid[] playerIds, FriendsDelegate.FindFriendsCallback callback)
        {
            var msg = new FindFriendsRequest();
            msg.PlayerIds = playerIds;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)FriendRequest.FindFriendsByPlayerId, data, _friendBaseResponse);
            _friendBaseResponse.RegisterCallback(tag, r =>
            {
                if (r.Friends == null)
                    callback(null);
                else
                    callback(r.Friends.ToArray());
            });
        }

        public void InviteFriend(MadId id, FriendsDelegate.InviteFriendCallback callback)
        {
            var msg = new FriendOpRequest();
            msg.FriendId = id;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)FriendRequest.FriendInvite, data, _friendInviteResponse);
            _friendInviteResponse.RegisterCallback(tag, r =>
            {
                if (callback != null)
                    callback(r.InviteId, r.Result);
            });
        }

        public void InviteFriendAnswer(long inviteId, bool accept, FriendsDelegate.InviteFriendCallback callback)
        {
            var type = accept ? FriendRequest.FriendInviteAccept : FriendRequest.FriendInviteReject;
            var msg = new FriendInviteOpRequest();
            msg.Id = inviteId;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)type, data, _friendInviteResponse);
            _friendInviteResponse.RegisterCallback(tag, r =>
            {
                if (callback != null)
                    callback(r.InviteId, r.Result);
            });
        }

        public bool InviteFriendCancel(long inviteId)
        {
            var msg = new FriendInviteOpRequest();
            msg.Id = inviteId;
            var data = FriendServerSerializer.Serialize(msg);
            _messageSender.Notify(Server, (int)FriendRequest.FriendInviteCancel, data);
            return _messageSender.IsValid;
        }

        public void CreateRoom(TimeSpan autostartDelay, string gameSpecificData, FriendsDelegate.CreateRoomAnswerCallback callback)
        {
            var msg = new CreateFriendRoomRequest();
            msg.AutoStartDelay = autostartDelay + BattleStartDecisionDelay;
            msg.GameSpecificData = gameSpecificData;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)FriendRequest.RoomCreate, data, _roomResponse);
            _roomResponse.RegisterCallback(tag, r =>
            {
                callback(r.RoomId, r.PartyLimit, r.Result);
            });
        }

        public void InviteRoom(long roomId, MadId friendId, FriendsDelegate.RoomAnswerCallback callback)
        {
            var msg = new FriendRoomInviteRequest();
            msg.FriendId = friendId;
            msg.SenderId = Id;
            msg.RoomId = roomId;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)FriendRequest.RoomInvite, data, _lobbyInviteResponse);
            _lobbyInviteResponse.RegisterCallback(tag, (resp) =>
            {
                if (callback != null)
                    callback(resp.RoomId, resp.Code);
            });
        }

        public void InviteRoomAnswer(long roomId, bool accept, FriendsDelegate.RoomAnswerCallback callback)
        {
            var msg = new FriendRoomRequest();
            msg.RoomId = roomId;
            var type = accept ? FriendRequest.RoomInviteAccept : FriendRequest.RoomInviteReject;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)type, data, _roomResponse);
            _roomResponse.RegisterCallback(tag, r =>
            {
                if (callback != null)
                    callback(r.RoomId, r.Result);
            });
        }

        public void LeaveRoom(long roomId, FriendsDelegate.FriendsClientCallback callback)
        {
            var msg = new FriendRoomRequest();
            msg.RoomId = roomId;
            var data = FriendServerSerializer.Serialize(msg);
            _messageSender.Notify(Server, (int)FriendRequest.RoomLeave, data);
            if (callback != null)
                callback(_messageSender.IsValid);
        }

        public void KickRoom(long roomId, MadId friendId, FriendsDelegate.RoomAnswerCallback callback)
        {
            var msg = new FriendRoomRequest();
            msg.RoomId = roomId;
            msg.FriendId = friendId;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)FriendRequest.RoomKick, data, _roomResponse);
            _roomResponse.RegisterCallback(tag, r =>
            {
                if (callback != null)
                    callback(r.RoomId, r.Result);
            });
        }

        public void StartBattle(long roomId, string gameData, FriendsDelegate.StartBattleCallback callback)
        {
            var msg = new FriendRoomRequest();
            msg.RoomId = roomId;
            msg.GameSpecificData = gameData;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)FriendRequest.RoomStartBattle, data, _roomResponse);
            _roomResponse.RegisterCallback(tag, r =>
            {
                if (callback != null)
                    callback(r.Result);
            });
        }

        public void RoomUpdate(long roomdId, string gameSpecificData)
        {
            var msg = new FriendRoomRequest();
            msg.RoomId = roomdId;
            msg.GameSpecificData = gameSpecificData;
            var data = FriendServerSerializer.Serialize(msg);
            _messageSender.Notify(Server, (int)FriendRequest.RoomUpdate, data);
        }

        public void SendGift(MadId friendId, int gameSpecificId, FriendsDelegate.FriendGiftCallback callback)
        {
            var msg = new FriendGiftRequest();
            msg.FriendId = friendId;
            msg.GameSpecificId = gameSpecificId;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)FriendRequest.GiftSend, data, _giftResponse);
            _giftResponse.RegisterCallback(tag, r =>
            {
                if (callback != null)
                    callback(r.GiftId, r.Result, r.NextGift);
            });
        }

        private Dictionary<long, Tuple<GiftResult, DateTime, FriendsDelegate.FriendGiftCallback>> _giftClaims = new Dictionary<long, Tuple<GiftResult, DateTime, FriendsDelegate.FriendGiftCallback>>();

        public GiftResult GetGiftClaimResultFromCache(long giftId)
        {
            if (_giftClaims.TryGetValue(giftId, out var ctx))
                return ctx.Item1;
            return GiftResult.None;
        }

        public void ClaimGift(long giftId, FriendsDelegate.FriendGiftCallback callback)
        {
            if (_giftClaims.TryGetValue(giftId, out var ctx))
            {
                if(ctx.Item1 == GiftResult.None)
                    callback?.Invoke(giftId, GiftResult.ClaimInProgress, DateTime.MinValue);
                if(ctx.Item1 == GiftResult.Success)
                    callback?.Invoke(giftId, GiftResult.AlreadyClaimed, DateTime.MinValue);
                return;
            }

            _giftClaims[giftId] = new Tuple<GiftResult, DateTime, FriendsDelegate.FriendGiftCallback>(GiftResult.None, DateTime.MinValue, callback);
            var msg = new FriendGiftRequest();
            msg.GiftId = giftId;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)FriendRequest.GiftClaim, data, _giftResponse);
            _giftResponse.RegisterCallback(tag, r =>
            {
                _giftClaims[giftId] = new Tuple<GiftResult, DateTime, FriendsDelegate.FriendGiftCallback>(r.Result, r.NextGift, callback);
                callback?.Invoke(r.GiftId, r.Result, r.NextGift);
            });
        }

        static readonly Dictionary<GiftResult, GiftStatus> giftResult2Status = new Dictionary<GiftResult, GiftStatus>()
        {
            {GiftResult.None,  GiftStatus.NotClaimed },
            {GiftResult.NoGift,  GiftStatus.NoGift },
            {GiftResult.Success,  GiftStatus.Claimed }
        };

        public void GetGiftClaimState(long[] gifts, Action<GiftStatus[]> callback)
        {
            var msg = new FriendGiftRequest();
            msg.ClaimGiftIds = gifts;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)FriendRequest.GiftsClaimStatus, data, _giftResponse);
            _giftResponse.RegisterCallback(tag, r =>
            {
                var result = r.BatchResult.Select(_ => giftResult2Status[_]).ToArray();
                callback?.Invoke(result);
            });
        }

        public void ClaimGifts(long[] targetGiftIds, FriendsDelegate.FriendGiftBatchCallback callback)
        {
            var giftIds = targetGiftIds.Where(_ => !_giftClaims.ContainsKey(_)).ToArray();
            if (giftIds.Length < 1)
                return;
            foreach (var id in giftIds)
            {
                _giftClaims[id] = new Tuple<GiftResult, DateTime, FriendsDelegate.FriendGiftCallback>(GiftResult.None, DateTime.MinValue, null);
            }
            var msg = new FriendGiftRequest();
            msg.ClaimGiftIds = giftIds;
            var data = FriendServerSerializer.Serialize(msg);
            var tag = _messageSender.Request(Server, (int)FriendRequest.GiftsClaim, data, _giftResponse);
            _giftResponse.RegisterCallback(tag, r =>
            {
                for (var i = 0; i < giftIds.Length; ++i)
                {
                    _giftClaims[giftIds[i]] = new Tuple<GiftResult, DateTime, FriendsDelegate.FriendGiftCallback>(r.BatchResult[i], r.NextGift, null);
                }
                if (callback != null)
                    callback(giftIds, r.BatchResult, r.NextGift);
            }, () => 
            {
                foreach (var giftId in giftIds)
                    _giftClaims.Remove(giftId);
            });
        }

        public void StatusChanged(int statusId)
        {
            var msg = new FriendStatusNotify();
            msg.Status = statusId;
            FriendList.Me.Status = statusId;
            FriendList.Me.LastStatus = _time.Now;
            var data = FriendServerSerializer.Serialize(msg);
            _messageSender.Notify(Server, (int)FriendRequest.StatusChange, data);
        }

        public void SetNonFriendsStatusWatch(Guid[] playerIds)
        {
            var msg = new NonFriendsWatchedRequest();
            msg.Observables = playerIds;
            var data = FriendServerSerializer.Serialize(msg);
            _messageSender.Notify(Server, (int)FriendRequest.SetNonFriendsStatusWatch, data);
        }

        public int Status {
            get { return FriendList.Me.Status; }
            set { StatusChanged(value); }
        }

        public int GiftsStackSize => Config.GiftsStackSize;
        public bool RoomInviteAnybody => Config.RoomInviteAnybody;
    }
}
