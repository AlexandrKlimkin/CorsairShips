using System;
using System.Collections.Generic;
using System.Text;

namespace FriendsClient
{
    public enum FriendRequest
    {
        // Ping = 0
        Init = 1,
        RemoveFriend,
        FindFriend,
        FriendInvite,
        FriendInviteAccept,
        FriendInviteReject,
        FriendInviteCancel,
        RoomCreate,
        RoomInvite,
        RoomInviteAccept,
        RoomInviteReject,
        RoomLeave,
        RoomKick,
        RoomStartBattle,
        RoomUpdate,
        StatusChange,
        GiftSend,
        GiftClaim,
        FindFriendsByPlayerId,
        GiftsClaim,
        GiftsClaimStatus,
        SetNonFriendsStatusWatch
    }
}
