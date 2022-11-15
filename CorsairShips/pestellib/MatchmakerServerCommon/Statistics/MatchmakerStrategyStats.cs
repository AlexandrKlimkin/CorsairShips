using System;
using System.Collections.Generic;
using System.Text;

namespace PestelLib.MatchmakerServer.Statistics
{
    public class MatchmakerStrategyStats
    {
        public int PendingRequestsCount;
        public long UsersMatched;
        public long BotsMatched;
        public long Uptime;
        public float AverageFitment;
        public long MatchesCount;
        public long AverageAwaitTime;
        public int PendingMatches;
        public long UsersLeft;
        public float BotFraction => (float)BotsMatched / UsersMatched;
    }
}
