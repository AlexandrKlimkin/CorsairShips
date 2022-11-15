using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using S;

namespace BackendCommon.Code.Leagues
{
    public abstract class LeagueStorageBase : ILeagueStorage
    {
        protected readonly LeagueLeaderboardConfig _config;
        protected readonly LeagueStateCache _state;

        protected LeagueStorageBase(LeagueLeaderboardConfig config, LeagueStateCache state)
        {
            _config = config;
            _state = state;
        }

        public int GetGlobalRank(Guid playerId) { return GetGlobalRank(_state.Season, playerId); }
        public int GetLeagueRank(Guid playerId, int leagueLvl) { return GetLeagueRank(_state.Season, playerId, leagueLvl); }
        public IEnumerable<LeaguePlayerInfo> GetGlobalTop(int amount) { return GetGlobalTop(_state.Season, amount); }
        public IEnumerable<LeaguePlayerInfo> GetLeagueTop(int leagueLvl, int amount) { return GetLeagueTop(_state.Season, leagueLvl, amount); }
        public IEnumerable<DivisionInfo> GetDivisions(int leagueLevel) { return GetDivisions(_state.Season, leagueLevel); }
        public IEnumerable<DivisionInfo> GetDivisionsWithMaxPopulation(int leagueLevel, int maxPopulation) { return GetDivisionsWithMaxPopulation(_state.Season, leagueLevel, maxPopulation); }

        public abstract void OnSeasonEnd();
        public abstract long IncrementBotScores(int leagueLvl, long scoreMin, long scoreMax, int limit, bool force = false);
        public abstract SeasonEndInfo PopSeasonInfo(Guid playerId, int season);
        public abstract void SeasonEnd(Guid playerId, SeasonEndInfo info);
        public abstract LeaguePlayerInfo GetPlayer(Guid playerId);
        public abstract void SetPlayerDivision(Guid playerId, Guid division);
        public abstract LeaguePlayerInfo CreatePlayer(Guid playerId, string name, string facebookId, Guid division, bool isBot, int leagueLvl);
        public abstract int GetGlobalRank(int season, Guid playerId);
        public abstract int GetLeagueRank(int season, Guid playerId, int leagueLvl);
        public abstract IEnumerable<LeaguePlayerInfo> GetGlobalTop(int season, int amount);
        public abstract IEnumerable<LeaguePlayerInfo> GetLeagueTop(int season, int leagueLvl, int amount);
        public abstract IEnumerable<LeaguePlayerInfo> GetDivision(Guid divisionId);
        public abstract DivisionInfo CreateDivision(int leagueLvl);
        public abstract IEnumerable<DivisionInfo> GetDivisions(int season, int leagueLevel);
        public abstract IEnumerable<DivisionInfo> GetDivisionsWithMaxPopulation(int season, int leagueLevel, int maxPopulation);
        public abstract Task<IDisposable> Lock();
        public abstract void ScheduleAddScore(Guid playerId, long scoreDelta);
        public abstract void ProcessAddScoreSchedule();
        public abstract LeaguePlayerInfo PlayerLogin(Guid playerId, string name, string facebookId = null);
        public abstract IEnumerable<LeaguePlayerInfo> ActivePlayers(int amount);
        public abstract LeaguePlayerInfo BanById(Guid playerId);
        public abstract LeaguePlayerInfo BanByScore(long score);
        public abstract bool HasBan(Guid playerId);
        public abstract LeaguePlayerInfo Remove(Guid playerId);
        public abstract void WipeSeasonData(int season);
    }
}
