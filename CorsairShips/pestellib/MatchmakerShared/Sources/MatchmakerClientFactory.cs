namespace PestelLib.MatchmakerShared
{
    public static class MatchmakerClientFactory
    {
        public static MatchmakerClientProtocol<Request, MatchT, StateT> CreateTcp<Request, MatchT, StateT>(string host, int port) where Request : MatchmakerRequest where MatchT : Match<Request> where StateT : MatchingStats
        {
            var tcp = new AsyncTcpMatchmakerClient(host, port);
            return new MatchmakerClientProtocol<Request, MatchT, StateT>(tcp);
        }
    }
}
