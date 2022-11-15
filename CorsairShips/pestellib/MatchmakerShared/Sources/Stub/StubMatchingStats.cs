using System;

namespace PestelLib.MatchmakerShared
{
    public class StubMatchingStats : MatchingStats
    {
        public TimeSpan WaitTime;
        public int PlayersInRoom;

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 23 + WaitTime.GetHashCode();
            hash = hash * 23 + PlayersInRoom.GetHashCode();
            return hash;
        }
    }
}
