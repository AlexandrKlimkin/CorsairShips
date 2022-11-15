using System;
using System.Collections.Generic;
using FriendsClient.Private;
using FriendsClient.Sources;
using S;

namespace FriendsClient.Lobby
{
    public interface IFriendsRoom
    {
        long RoomId { get; }
        /// <summary>
        /// Player is the room host.
        /// </summary>
        bool ImHost { get; }
        /// <summary>
        /// State of a room.
        /// </summary>
        RoomStatus RoomStatus { get; }
        /// <summary>
        /// Max number of player in thre room (length of array <see cref="Party"/>).
        /// </summary>
        int PartyLimit { get; }
        /// <summary>
        /// Friends which accepted invitation (joined room). Joined friend not present in CanInvite list.
        /// </summary>
        IReadOnlyList<InvitedFriend> Party { get; }
        /// <summary>
        /// Friends list available for invitation (not joined current room).
        /// </summary>
        IReadOnlyList<InviteableFriend> CanInvite { get; }
        /// <summary>
        /// Game specific data which can interpret game client to join battle (eg RoomId for Photon-based battle).
        /// </summary>
        string GameSpecificData { get; set; }
        /// <summary>
        /// Time before battle must be started.
        /// </summary>
        TimeSpan BattleCountdown { get; }

        event Action<IFriendsRoom, FriendStatusChangedMessage> OnFriendStatus;
        /// <summary>
        /// Player becomes new host for the room. Caused by old host leave.
        /// Not raised if player is creator of the room (check ImHost instead).
        /// </summary>
        event Action<IFriendsRoom> OnImHost;
        event Action<IFriendsRoom, FriendBase> OnJoined;
        event Action<IFriendsRoom, MadId> OnKick;
        event Action<IFriendsRoom, MadId> OnLeave;
        event Action<IFriendsRoom> OnCanInviteChanged;
        event Action<IFriendsRoom> OnPartyChanged; 
        event Action<IFriendsRoom> OnStartBattle;
        /// <summary>
        /// You've missed to start battle in time. Leave the Room or wait until battle end (in case of ServerConfig.DontCloseRoomOnBattleStart == true, overwise room will be closed anyway).
        /// </summary>
        event Action<IFriendsRoom> OnBattleAlreadyStarted;
        event Action<IFriendsRoom> OnClosed;
        event Action<IFriendsRoom, string> OnGameData;
        /// <summary>
        /// Raised only for room host. This is server warning about time running out for for battle to start.
        /// Client must call <see cref="StartBattle"/> within the timeout passed as argument (TimeSpan).
        /// </summary>
        event Action<IFriendsRoom, TimeSpan> OnRoomCountdown;

        /// <summary>
        /// Leave room. e.g player closes room window.
        /// 
        /// Room members will get <see cref="FriendEvent.RoomLeave"/> notification.
        /// </summary>
        /// <param name="callback"></param>
        void Leave(FriendsDelegate.FriendsClientCallback callback = null);

        /// <summary>
        /// Call if you no longer need this object.
        /// </summary>
        void Close();

        /// <summary>
        /// Tells other members of room (except host) how to join battle. For example, called by room host after he created room on photon passing photon room id as gameData.
        /// 
        /// Room members (except host) will get <see cref= "FriendEvent.RoomStartBattle" /> notification.
        /// 
        /// Possible values of <see cref="RoomResult"/> sent to callback:
        ///     * Success       - notifications to joined friends are sent.
        ///     * NotAllowed    - you are not room host.
        /// </summary>
        void StartBattle(FriendsDelegate.StartBattleCallback callback = null);
    }
}