using System;
using System.Threading.Tasks;
using PestelLib.MatchmakerShared;

namespace PestelLib.MatchmakerServer
{
    public interface IMatchmakerRequestProvider
    {
        /// <summary>
        /// Called by main matchmaker controller which role is 
        /// to bind MatchmakerRequest source with MatchmakerStrategy
        /// It's promise to return one or more MatchmakerRequest
        /// </summary>
        /// <returns></returns>
        Task<MatchmakerRequest[]> GetRequestsAsync();
        /// <summary>
        /// Called by main matchmaker controller
        /// Relies on the implementation to notify all match members about the match
        /// </summary>
        /// <param name="matches"></param>
        Task AnnounceMatch(IMatch match);
        /// <summary>
        /// Event fires each time client disconnects from matchmaker
        /// </summary>
        event Action<Guid> UserLeave;
        /// <summary>
        /// For incomplete matches send some stats (defined by the strategy, see IMath.State) to picked players
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        Task SendMatchingInfo(IMatch match);
        /// <summary>
        /// Send general server info (see ServerStats)
        /// </summary>
        /// <returns></returns>
        Task SendServerInfo(ServerStats stats);
    }
}
