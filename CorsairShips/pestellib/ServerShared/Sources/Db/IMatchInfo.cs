using System;
using System.Collections.Generic;

namespace PestelLib.ServerCommon.Db
{
    public interface IMatchInfo
    {
        /// <summary>
        /// Set match result from trusted source
        /// </summary>
        void SetMatchEnd(string match, string[] winners, string[] losers, string[] draw, Dictionary<string,string> extra);
        /// <summary>
        /// Match untrusted results with trusted data
        /// </summary>
        bool Validate(string match, S.MatchResult result, string userId);
        /// <summary>
        /// Removes old match results
        /// </summary>
        /// <returns></returns>
        long CleanOldMatchInfos(DateTime olderThan);
    }
}
