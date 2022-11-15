using System;

namespace PestelLib.MatchmakerShared
{
    // стата обнуляется при перезапуске
    public struct ServerStats
    {
        public int PendingRequests; // кол-во игроков в очереди
        public int PendingMatches;  // кол-во неполных матчей
        public int MatchesServed; // кол-во матчей собрано 
        public TimeSpan AverageWaitTime; // среднее время ожидания матча (за последние 10 матчей)

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 23 + PendingRequests.GetHashCode();
            hash = hash * 23 + PendingMatches.GetHashCode();
            hash = hash * 23 + MatchesServed.GetHashCode();
            hash = hash * 23 + AverageWaitTime.GetHashCode();
            return hash;
        }
    }
}
