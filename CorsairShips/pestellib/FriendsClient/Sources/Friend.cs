using System;
using System.Collections.Generic;
using FriendsClient.Sources;
using MessagePack;
using PestelLib.ServerClientUtils;
using S;
using ServerShared.PlayerProfile;

namespace FriendsClient
{
    [MessagePackObject()]
    public class FriendBase
    {
        /// <summary>
        /// Profile attached to a friend (game specific info about friend: nick, level, etc.)
        /// </summary>
        [Key(0)]
        public ProfileDTO Profile;
        /// <summary>
        /// Human-friendly id of a friend
        /// </summary>
        [Key(1)]
        public MadId Id;
        /// <summary>
        /// 0 - offline, 2 - online, 1 - 'in room' (set automatically <see cref="FriendStatus"/>). >=3 game specific friend status (in battle, etc.).
        /// Change status through FriendsClient.StatusChanged.
        /// </summary>
        [Key(3)]
        public int Status;
        /// <summary>
        /// Server time of last status change. Use <see cref="SharedTime"/> in calculations.
        /// </summary>
        [Key(4)]
        public DateTime LastStatus;
        /// <summary>
        /// Time then send gift will be available. Use <see cref="SharedTime"/> in calculations.
        /// </summary>
        [Key(5)]
        public DateTime NextGift;
    }

    public class FriendBaseComparer : IEqualityComparer<FriendBase>
    {
        public static readonly FriendBaseComparer Instance = new FriendBaseComparer();

        public bool Equals(FriendBase x, FriendBase y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(FriendBase obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
