using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PestelLib.MatchmakerShared;

namespace PestelLib.MatchmakerServer
{
    public interface IMatchmakerStrategy
    {
        /// <summary>
        /// Matchmaker CCU
        /// </summary>
        int PendingRequestsCount { get; }
        /// <summary>
        /// Number of users matched
        /// </summary>
        long UsersMatched { get; }
        /// <summary>
        /// Number of bots matched
        /// </summary>
        long BotsMatched { get; }
        /// <summary>
        /// Amount of mseconds elapsed since strategy was created
        /// </summary>
        long Uptime { get; }
        /// <summary>
        /// Total uptime assembled matches count
        /// </summary>
        long MatchesCount { get; }
        /// <summary>
        /// Average fitment to the matchmaking criterias (depends on the implementation)
        /// </summary>
        float AverageFitment { get; }
        /// <summary>
        /// Average mseconds to wait for match 
        /// </summary>
        long AverageAwaitTime { get; }
        /// <summary>
        /// Matches awaiting to be send to the users (>0 means networking problems)
        /// </summary>
        int PendingMatches { get; }
        /// <summary>
        /// Number of users which has left the matchmaker before being matched
        /// </summary>
        long UsersLeft { get; }

        ServerStats Stats { get; }

        /// <summary>
        /// Called by a layer which receives matchmaker requests from the users
        /// </summary>
        /// <param name="request"></param>
        void NewRequest(MatchmakerRequest request);
        /// <summary>
        /// Called by a layer which is responsible for answering to users' matchmaker requests
        /// </summary>
        /// <returns></returns>
        Task<IMatch> GetMatchAsync();
        /// <summary>
        /// Called by a layer which is responsible for sending matching progress notification
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<IMatch>> GetIncompleteMatches();
        /// <summary>
        /// User leaves the matchmaker
        /// </summary>
        /// <param name="guid"></param>
        void Leave(Guid guid);
    }
}
