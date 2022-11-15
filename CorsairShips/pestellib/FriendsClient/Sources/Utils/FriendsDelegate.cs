using System;
using System.Collections.Generic;
using System.Text;
using FriendsClient.Lobby;
using FriendsClient.Private;
using FriendsClient.Sources;

namespace FriendsClient
{
    public static class FriendsDelegate
    {
        public delegate void CreateRoomAnswerCallback(long roomId, int partyLimit, RoomResult result);
        public delegate void RoomAnswerCallback(long roomId, RoomResult result);
        public delegate void InviteFriendCallback(long inviteId, InviteFriendResult result);
        public delegate void FindFriendCallback(FriendBase friend);
        public delegate void FindFriendsCallback(FriendBase[] friends);
        public delegate void FriendsClientCallback(bool success);
        public delegate void FriendGiftCallback(long giftId, GiftResult result, DateTime nextGift);
        public delegate void FriendGiftBatchCallback(long[] giftId, GiftResult[] results, DateTime nextGift);
        public delegate void InitCallback(FriendInitResponse response);
        /// <summary>
        /// Possible values of <see cref="RoomResult"/>:
        ///     * Success       - notifications to joined friends are sent.
        ///     * NotAllowed    - you are not room host.
        /// </summary>
        /// <param name="result"></param>
        public delegate void StartBattleCallback(RoomResult result);
    }
}
