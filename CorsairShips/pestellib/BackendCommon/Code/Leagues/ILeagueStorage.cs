using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using S;

namespace BackendCommon.Code.Leagues
{
    public interface ILeagueStorage
    {
        LeaguePlayerInfo GetPlayer(Guid playerId);
        void SetPlayerDivision(Guid playerId, Guid division);
        LeaguePlayerInfo CreatePlayer(Guid playerId, string name, string facebookId, Guid division = default(Guid), bool isBot = false, int leagueLvl = 0);
        /// <summary>
        /// Save reward. Reset division, score. Change league.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="info"></param>
        void SeasonEnd(Guid playerId, SeasonEndInfo info);
        /// <summary>
        /// Get reward for specified season, but only once (removes or marks info as claimed).
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="season"></param>
        /// <returns></returns>
        SeasonEndInfo PopSeasonInfo(Guid playerId, int season);

        long IncrementBotScores(int leagueLvl, long scoreMin, long scoreMax, int limit, bool force = false);
        int GetGlobalRank(Guid playerId);
        int GetGlobalRank(int season, Guid playerId);
        int GetLeagueRank(Guid playerId, int leagueLvl);
        int GetLeagueRank(int season, Guid playerId, int leagueLvl);
        IEnumerable<LeaguePlayerInfo> GetGlobalTop(int amount);
        IEnumerable<LeaguePlayerInfo> GetGlobalTop(int season, int amount);
        IEnumerable<LeaguePlayerInfo> GetLeagueTop(int leagueLvl, int amount);
        IEnumerable<LeaguePlayerInfo> GetLeagueTop(int season, int leagueLvl, int amount);
        IEnumerable<LeaguePlayerInfo> GetDivision(Guid divisionId);
        DivisionInfo CreateDivision(int leagueLvl);
        IEnumerable<DivisionInfo> GetDivisions(int leagueLevel);
        IEnumerable<DivisionInfo> GetDivisions(int season, int leagueLevel);
        IEnumerable<DivisionInfo> GetDivisionsWithMaxPopulation(int leagueLevel, int maxPopulation);
        IEnumerable<DivisionInfo> GetDivisionsWithMaxPopulation(int season, int leagueLevel, int maxPopulation);
        Task<IDisposable> Lock();
        //void AddScore(Guid playerId, long scoreDelta);
        void ScheduleAddScore(Guid playerId, long scoreDelta); // call without lock
        void ProcessAddScoreSchedule(); // call without lock
        void OnSeasonEnd();
        LeaguePlayerInfo PlayerLogin(Guid playerId, string name, string facebookId = null);
        IEnumerable<LeaguePlayerInfo> ActivePlayers(int amount);
        LeaguePlayerInfo BanById(Guid playerId);
        LeaguePlayerInfo BanByScore(long score);
        bool HasBan(Guid playerId);
        LeaguePlayerInfo Remove(Guid playerId);
        void WipeSeasonData(int season);
    }
}