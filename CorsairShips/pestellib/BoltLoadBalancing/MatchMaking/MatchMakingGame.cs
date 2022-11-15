using MasterServerProtocol;
using System;
using BoltLoadBalancing.MasterServer;

namespace BoltLoadBalancing.MatchMaking
{
    public struct MatchMakingGame
    {
        public IMasterServer MasterServer;
        public GameServerStateReport GameServerStateReport;
    }
}
