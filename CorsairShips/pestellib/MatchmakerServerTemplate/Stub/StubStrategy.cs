using System;
using System.Collections.Generic;
using System.Linq;
using PestelLib.MatchmakerShared;
using PestelLib.MatchmakerServer.Config;

namespace PestelLib.MatchmakerServer.Stub
{
    class StubStrategy : MatchmakerStrategyBase<MatchmakerRequest, StubMatch>
    {
        List<MatchmakerRequest> _requests = new List<MatchmakerRequest>();

        public StubStrategy(MatchmakerConfig matchmakerConfig)
            :base(matchmakerConfig)
        { }

        protected override StubMatch GenerateMatch(StubMatch match, HashSet<MatchmakerRequest> pool)
        {
            const int matchSize = 3;

            // existing match, try to stuff with request from the pool
            if (match != null)
            {
                if (match.IsFull)
                    return match;

                var count = Math.Min(pool.Count, (matchSize - match.SimpleParty.Count));
                while (pool.Count > 0 && !match.IsFull)
                {
                    var r = pool.First();
                    match.SimpleParty.Add(r);
                    pool.Remove(r);
                }

                match.Stats = new StubMatchingStats
                {
                    PlayersInRoom = match.Party.Count,
                    WaitTime = match.GetWaitTime()
                };

                return match;
            }
            // try to assemble new match
            if (!pool.Any())
                return null;

            var result = new StubMatch();
            return GenerateMatch(result, pool);
        }
    }
}
