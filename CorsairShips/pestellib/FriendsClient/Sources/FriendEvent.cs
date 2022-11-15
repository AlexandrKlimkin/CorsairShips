namespace FriendsClient.Sources
{
    public enum FriendEvent
    {
        /// <summary>
        /// Another player invites me to become a friend.
        /// From: Player.
        /// To: Player.
        /// </summary>
        FriendInvite = 0x80,
        /// <summary>
        /// Another player accepted my invitation.
        /// From: Accepting player.
        /// To: Owner of invitation.
        /// </summary>
        FriendAccept,
        /// <summary>
        /// Another player rejected my invitation.
        /// From: Rejecting player.
        /// To: Owner of invitation.
        /// </summary>
        FriendReject,
        /// <summary>
        /// Invite sender cancels invitation.
        /// From: Owner of invite.
        /// To: Invited player.
        /// </summary>
        FriendInviteCancel,
        /// <summary>
        /// Invite has expiried.
        /// From: Server.
        /// To: Invite owner.
        /// </summary>
        FriendInviteExpired,
        /// <summary>
        /// Your friend removed you from his friend-list.
        /// From: Your ex friend.
        /// To: You.
        /// </summary>
        FriendRemoved,
        /// <summary>
        /// For situations when two conter-invites accepted automaticaly.
        /// From: Server.
        /// To: Friend who have created counter invite. (has incoming invite and invites same user by himself).
        /// </summary>
        NewFriend,
        /// <summary>
        /// Game specific statue of my friend has changed (<see cref="FriendBase.Status"/>).
        /// From: Player which status has changed or Server (on/offline and 'in room' statuses defined by the server <see cref="FriendStatus"/>).
        /// To: All friends of than player.
        /// </summary>
        StatusChanged,
        /// <summary>
        /// Another player sent me a gift.
        /// From: Friend.
        /// To: Friend.
        /// </summary>
        Gift,
        /// <summary>
        /// Another player claims my gift.
        /// From: Friend.
        /// To: Friend.
        /// </summary>
        GiftClaimed,
        /// <summary>
        /// My friend invites me to join battle.
        /// From: Room host.
        /// To: Room hots' friend.
        /// </summary>
        RoomInvite,
        /// <summary>
        /// My friend accepts my invitation to join battle.
        /// From: Accepting player.
        /// To: Owner of invitation.
        /// </summary>
        RoomAccept,
        /// <summary>
        /// My friend rejects my invitation to join battle.
        /// From: Rejecting player.
        /// To: Owner of invitation.
        /// </summary>
        RoomReject,
        /// <summary>
        /// Room host has left. New host chosen by the server.
        /// From: Server.
        /// To: Members of a room (new host too).
        /// </summary>
        RoomHostChange,
        /// <summary>
        /// Another player just joined room.
        /// From: Server.
        /// To: Members of a room.
        /// </summary>
        RoomJoin,
        /// <summary>
        /// Another player has left the room.
        /// From: Server or Leaving Player.
        /// To: Members of a room.
        /// </summary>
        RoomLeave,
        /// <summary>
        /// Another player has been kicked from the room.
        /// From: Room host.
        /// To: Members of room (kicked player too).
        /// </summary>
        RoomKick,
        /// <summary>
        /// Room host starts match manually.
        /// From: Room host.
        /// To: Members of a room (except host).
        /// </summary>
        RoomStartBattle,
        /// <summary>
        /// Room info sent to each joined player.
        /// From: Server.
        /// To: Members of a room.
        /// </summary>
        RoomInfo,
        /// <summary>
        /// Game specific data needed to start the game.
        /// From: Host.
        /// To: Members of a room.
        /// </summary>
        RoomGameData,
        /// <summary>
        /// Update battle start timer. In the end requests host to start match or he will be kicked from room after timeout.
        /// From: Server.
        /// To: Members of a room.
        /// </summary>
        RoomCountdown,
        /// <summary>
        /// My friend's room invite has expiried.
        /// From: Server.
        /// To: Owner of invitation and invitation target.
        /// </summary>
        RoomAutoReject,
        /// <summary>
        /// Someone who i know updated his profile.
        /// From: Server.
        /// To: Anyone who knows updated player.
        /// </summary>
        ProfileUpdate
    }
}