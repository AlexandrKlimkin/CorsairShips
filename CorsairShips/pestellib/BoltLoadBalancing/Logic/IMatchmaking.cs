using System.Collections.Generic;
using BoltLoadBalancing.MasterServer;
using BoltLoadBalancing.MatchMaking;

namespace BoltLoadBalancing.Logic
{
    public interface IMatchmaking
    {
        List<MatchMakingGame> GetPossibleMatches(string matchmakingString, IEnumerable<IMasterServer> masters);
    }
}