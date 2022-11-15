using System;
using PestelLib.ServerCommon.Config;

namespace BackendCommon.Code.Leagues
{
    public class LeagueLeaderboardConfig
    {
        public bool SeasonStartAlign = false;
        public int ActivePlayersAmount = 1000;
        public int SeasonHistory = 1;
        public bool UseBots = true;
        public TimeSpan BotsUpdateDelay = TimeSpan.FromHours(2);
        public int StorageLockTimeThreshold = 20;
        public int SeasonControllerTimeThreshold = 10000;
        public float DivisionUpCoeff = 0.1f;
        public float DivisionDownCoeff = 0.1f;
    }

    public class LeagueLeaderboardConfigCache
    {
        public const string LeagueLeaderboardConfigKey = "League:Config";
        public static LeagueLeaderboardConfig Get()
        {
            return SimpleJsonConfigLoader.LoadConfigFromRedis<LeagueLeaderboardConfig>(LeagueLeaderboardConfigKey, false);
        }
    }
}