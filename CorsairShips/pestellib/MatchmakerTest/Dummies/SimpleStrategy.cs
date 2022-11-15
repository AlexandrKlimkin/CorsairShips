using System;
using System.Collections.Generic;
using System.Linq;
using PestelLib.MatchmakerShared;
using PestelLib.MatchmakerServer;
using PestelLib.MatchmakerServer.Config;

namespace MatchmakerTest.Dummies
{
    class SimpleStrategy : MatchmakerStrategyBase<MatchmakerRequest, SimpleMatch>
    {
        List<MatchmakerRequest> _requests = new List<MatchmakerRequest>();

        public SimpleStrategy(MatchmakerConfig matchmakerConfig)
            :base(matchmakerConfig)
        { }

        protected override SimpleMatch GenerateMatch(SimpleMatch match, HashSet<MatchmakerRequest> pool)
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

                return match;
            }
            // try to assemble new match
            if (!pool.Any())
                return null;

            var result = new SimpleMatch();
            return GenerateMatch(result, pool);
        }
    }
}
