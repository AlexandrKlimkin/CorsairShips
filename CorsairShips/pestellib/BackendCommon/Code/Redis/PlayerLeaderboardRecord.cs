using System;
using System.Collections.Generic;
using StackExchange.Redis;

namespace ServerLib
{
    public struct PlayerLeaderboardRecord : IEquatable<PlayerLeaderboardRecord>, IComparable, IComparable<PlayerLeaderboardRecord>
    {
        public readonly Guid userId;
        public readonly int score;

        public Guid UserId
        {
            get
            {
                return userId;
            }
        }

        public double Score
        {
            get
            {
                return score;
            }
        }

        public PlayerLeaderboardRecord(Guid userId, int score)
        {
            this.userId = userId;
            this.score = score;
        }

        public PlayerLeaderboardRecord(SortedSetEntry sortedSetEntry)
        {
            this.userId = Guid.Parse(sortedSetEntry.Element);
            this.score = (int)sortedSetEntry.Score;
        }


        public bool Equals(PlayerLeaderboardRecord other)
        {
            if (score == other.score)
                return userId == other.userId;
            return false;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is PlayerLeaderboardRecord))
                return -1;
            return CompareTo((PlayerLeaderboardRecord)obj);
        }

        public int CompareTo(PlayerLeaderboardRecord other)
        {
            var comarsionResult = other.score - score;
            if (comarsionResult == 0)
            {
                return userId.CompareTo(other.userId);
            }

            return comarsionResult;
        }

        public override int GetHashCode()
        {
            return userId.GetHashCode() ^ score.GetHashCode();
        }
    }

    /// <summary>
    /// Comparer for comparing two keys, handling equality as beeing greater
    /// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class PlayerLeaderboardRecordComparer : IComparer<PlayerLeaderboardRecord>
    {
        #region IComparer<TKey> Members

        public int Compare(PlayerLeaderboardRecord x, PlayerLeaderboardRecord y)
        {
            return x.CompareTo(y);
        }

        #endregion
    }
}
