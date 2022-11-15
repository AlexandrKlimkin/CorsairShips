using System;
using FriendsClient.FriendList;
using FriendsClient.Lobby;
using FriendsClient.Sources;
using S;

namespace FriendsClient.Private
{
    public interface IFriendsClientPrivate
    {
        /// <summary>
        /// Friend id of current user.
        /// </summary>
        MadId Id { get; }

        /// <summary>
        /// Player is busy (in shop, opening boxes, etc.). Asks server to resend battle invitations later if any are present.
        /// </summary>
        bool MuteBattleInvitations { get; set; }
        /// <summary>
        /// Specifies amout of time you have to wait before you can invite friend to any room if he rejects you previous invitation. 
        /// </summary>
        TimeSpan RoomReinviteCooldown { get; set; }
        /// <summary>
        /// Delay for the room host after AutostartDelay (<see cref="ILobby.CreateRoom"/> first arg) expired to start battle or he will be kicked from the room.
        /// </summary>
        TimeSpan BattleStartDecisionDelay { get; set; }
        int Status { get; set; }
        int GiftsStackSize { get; }
        bool RoomInviteAnybody { get; }

        event Action<FriendsInviteEventMessage> OnInvite;
        event Action<FriendsInviteEventMessage> OnInviteAccepted;
        event Action<FriendsInviteEventMessage> OnInviteRejected;
        event Action<FriendsInviteEventMessage> OnInviteCanceled;
        event Action<FriendsInviteEventMessage> OnInviteExpired;
        event Action<FriendStatusChangedMessage> OnFriendStatus;
        event Action<FriendGiftEventMessage> OnFriendGift;
        event Action<FriendGiftEventMessage> OnFriendGiftClaimed;
        event Action<RoomInviteEventMessage> OnRoomInvite;
        event Action<RoomInviteEventMessage> OnRoomAccept;
        event Action<RoomInviteEventMessage> OnRoomReject;
        event Action<RoomInviteEventMessage> OnRoomAutoReject;
        event Action<RoomJoinEventMessage> OnRoomJoin;
        event Action<RoomLeaveKickEventMessage> OnRoomLeave;
        event Action<RoomLeaveKickEventMessage> OnRoomKick;
        event Action<RoomInfoEventMessage> OnRoomInfo;
        event Action<RoomInfoEventMessage> OnRoomStartBattle;
        event Action<RoomChangeHostEventMessage> OnRoomNewHost;
        event Action<RoomGameDataMessage> OnRoomGameData;
        event Action<FriendEventMessage> OnNewFriend;
        event Action<FriendsProfileUpdateMessage> OnProfileUpdate;
        /// <summary>
        /// Requests host to start match or he will be kicked from room after <see cref="RoomStartBattleTimeoutMessage.AnswerTimeout"/>.
        /// To start battle call <see cref="FriendsClient.StartBattle"/>.
        /// </summary>
        event Action<RoomCountdownEventMessage> OnRoomCountdown;
        /// <summary>
        /// Your friend removed you from his friend-list.
        /// </summary>
        event Action<FriendEventMessage> OnFriendRemoved;

        IFriendList FriendList { get; }
        ILobby Lobby { get; }

        /// <summary>
        /// Returns all firends
        /// </summary>
        /// <returns></returns>
        FriendBase [] GetFriends();

        bool IsMyFriend(MadId id);

        IFriendInviteContext GetInvitationByFriend(MadId friendId);
        IFriendInviteContext CreateInvitaion(MadId friendId, FriendsDelegate.InviteFriendCallback callback);
        int RemoveInvitationByFriend(MadId friendId);

        DateTime GetRoomInviteCooldown(MadId friendId);

        /// <summary>
        /// Connect to friends server. Wait for <see cref="OnInitialized"/> before using any api.
        /// </summary>
        void Start();
        /// <summary>
        /// Disconnects from friends server.
        /// </summary>
        void Stop();

        /// <summary>
        /// Only for a single session stores all gift claims and returnda them through this method.
        /// </summary>
        /// <param name="giftId"></param>
        /// <returns></returns>
        GiftResult GetGiftClaimResultFromCache(long giftId);

        /// <summary>
        /// Searches friend by id. Results are passed to the callback.
        /// If null passed to a callback, then specified friend not found.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        void FindFriend(MadId id, FriendsDelegate.FindFriendCallback callback);

        /// <summary>
        /// Searches friends by player ids. Results are passed to the callback.
        /// </summary>
        /// <param name="playerIds"></param>
        /// <param name="callback"></param>
        void FindFriends(Guid[] playerIds, FriendsDelegate.FindFriendsCallback callback);

        /// <summary>
        /// Invites new friend.
        /// 
        /// Target user will get <see cref="FriendEvent.FriendInvite"/> notification.
        /// 
        /// posible InviteFriendResult values passed to callback:
        ///     * Success       - invite sent
        ///     * MyLimit       - my friends list reached cap MaxFriends (invite not sent).
        ///     * OtherLimit    - target player's friends list reached cap MaxFriends (invite not sent).
        ///     * InviteLimit   - target player's pending invites list reached limit MaxInvites (invite not sent).
        ///     * AlreadyFriend
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        void InviteFriend(MadId id, FriendsDelegate.InviteFriendCallback callback);

        /// <summary>
        /// Answer invite from another user.
        /// 
        /// Target user will get <see cref="FriendEvent.FriendAccept"/> or <see cref="FriendEvent.FriendReject"/> notification.
        /// 
        /// posible InviteFriendResult values passed to callback:
        ///     * Success       - [Reject or Accept]    answer processed with no errors (invite deleted).
        ///     * MyLimit       - [Only Accept]         my friends list reached cap MaxFriends (invite stay in not answered state).
        ///     * OtherLimit    - [Only Accept]         another player's friends list reached cap MaxFriends (invite stay in not answered state).
        ///     * Error         - [Reject or Accept]    invite not found.
        /// 
        /// </summary>
        /// <param name="inviteId"></param>
        /// <param name="accept">true to acceps invite (add to friends)</param>
        void InviteFriendAnswer(long inviteId, bool accept, FriendsDelegate.InviteFriendCallback callback);

        /// <summary>
        /// Cancel my invite.
        /// 
        /// Target user will get <see cref="FriendEvent.FriendInviteCancel"/> notification.
        /// </summary>
        /// <param name="inviteId"></param>
        bool InviteFriendCancel(long inviteId);

        void RemoveFriend(MadId friendId, FriendsDelegate.FriendsClientCallback callback);

        /// <summary>
        /// Create new room. Gives ability to play with friends.
        /// </summary>
        /// <param name="autostartDelay"></param>
        /// <param name="gameSpecificData">any data related to the battle. e.g. GameMode, GameMap</param>
        /// <param name="callback"><see cref="IFriendsRoom"/> will be passed here on success.</param>
        void CreateRoom(TimeSpan autostartDelay, string gameSpecificData, FriendsDelegate.CreateRoomAnswerCallback callback);

        /// <summary>
        /// Invite friend to room.
        /// 
        /// Target user will get <see cref="FriendEvent.RoomInvite"/> notification.
        /// 
        /// posible RoomResult values passed to callback:
        ///     * Success       - invite sent.
        ///     * NotFriend     - can't invite player if it's not friend-listed. Invite not sent.
        ///     * NotAllowed    - only host can invite friends.
        ///     * InvalidStatus - target friend is offline or in another room (check <see cref="FriendBase.Status"> for details).
        /// 
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="friendId"></param>
        /// <param name="callback"></param>
        void InviteRoom(long roomId, MadId friendId, FriendsDelegate.RoomAnswerCallback callback);

        /// <summary>
        /// Answer room invite from another user.
        /// 
        /// Target user will get <see cref="FriendEvent.RoomAccept"/> or <see cref="FriendEvent.RoomReject"/> notification.
        /// If invitation accepted then each member of a room gets <see cref="FriendEvent.RoomJoin"/> notification.
        /// 
        /// posible RoomResult values passed to callback:
        ///     * Success       - invite answered and removed. wait for the first <see cref="FriendEvent.RoomInfo"/>.
        ///     * NotInvited    - not invited. can't answer notexisting invitation.
        /// 
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="accept"></param>
        void InviteRoomAnswer(long roomId, bool accept, FriendsDelegate.RoomAnswerCallback callback);

        /// <summary>
        /// Player closes room window.
        /// 
        /// Room members will get <see cref="FriendEvent.RoomLeave"/> notification.
        /// 
        /// </summary>
        /// <param name="roomId"></param>
        void LeaveRoom(long roomId, FriendsDelegate.FriendsClientCallback callback);

        /// <summary>
        /// Kick friend from room. Only host can perform this operation.
        /// 
        /// Room members will get <see cref="FriendEvent.RoomKick"/> notification.
        /// 
        /// posible RoomResult values passed to callback:
        ///     * Success       - room member kicked.
        ///     * NotAllowed    - only host can kick members.
        ///     * NotInvited    - player not in room.
        /// 
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="friendId"></param>
        void KickRoom(long roomId, MadId friendId, FriendsDelegate.RoomAnswerCallback callback);

        /// <summary>
        /// Tells other members of room (except host) how to join battle. For example, called by room host after he created room on photon.
        /// 
        /// Room members (except host) will get <see cref= "FriendEvent.RoomStartBattle" /> notification.
        /// 
        /// Possible values of <see cref="RoomResult"/> sent to callback:
        ///     * Success       - notifications to joined friends are sent.
        ///     * NotAllowed    - you are not room host.
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="gameData">game specific data which can interpret game client to join battle (eg RoomId for Photon-based battle).</param>
        void StartBattle(long roomId, string gameData, FriendsDelegate.StartBattleCallback callback);

        void RoomUpdate(long roomdId, string gameSpecificId);

        /// <summary>
        /// Sends a gift to a friend.
        /// Gifts can be send only once to a friend per day.
        /// 
        /// Terget Player will get <see cref="FriendEvent.Gift"/> notification.
        /// 
        /// </summary>
        /// <param name="friendId"></param>
        /// <param name="gameSpecificId">game specific gift id</param>
        /// <param name="callback"></param>
        void SendGift(MadId friendId, int gameSpecificId, FriendsDelegate.FriendGiftCallback callback);

        /// <summary>
        /// Claims gift.
        /// 
        /// Giftsender will receive <see cref="FriendEvent.GiftClaimed"/> notification.
        /// 
        /// </summary>
        /// <param name="friendId"></param>
        /// <param name="giftId"></param>
        /// <param name="callback"></param>
        void ClaimGift(long giftId, FriendsDelegate.FriendGiftCallback callback);

        /// <summary>
        /// Claims one or more gifts in single request.
        /// </summary>
        /// <param name="giftIds"></param>
        /// <param name="callback"></param>
        void ClaimGifts(long[] giftIds, FriendsDelegate.FriendGiftBatchCallback callback);

        /// <summary>
        /// Check gift claim state on server. For the case when claim request sent but not properly processed by client (e.g. due crash).
        /// </summary>
        /// <param name="gifts"></param>
        /// <param name="callback"></param>
        void GetGiftClaimState(long[] gifts, Action<GiftStatus[]> callback);

        /// <summary>
        /// Report status change. You must send any game specific player status. Dont send offline (0), online (1) and in room(2) statuses here they are processed automaticaly by the server.
        /// 
        /// Each friend and room members (even if they are not friends) will get <see cref="FriendEvent.StatusChanged"/> notification.
        /// 
        /// </summary>
        /// <param name="statusId">any value >=3 has game specific meaning. Predefined values 0 - offline, 1 - online, 2 - in room.</param>
        void StatusChanged(int statusId);

        //incoming events
        void IncomingInvite(FriendsInviteEventMessage evt);

        void SetNonFriendsStatusWatch(Guid[] playerIds);
    }
}
