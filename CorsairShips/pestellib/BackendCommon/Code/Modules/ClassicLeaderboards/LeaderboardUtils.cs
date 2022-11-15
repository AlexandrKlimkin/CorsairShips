using System;
using System.Globalization;
using log4net;
using ServerLib;

namespace BackendCommon.Code.Modules.ClassicLeaderboards
{
    public static class LeaderboardUtils
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(LeaderboardUtils));

        static public void CheckLeaderboardName(string leaderboardName)
        {
            /*
            if (Array.IndexOf(LeaderBoards, leaderboardName) == -1)
            {
                throw new Exception("Wrong leaderboard type: " + leaderboardName);
            }
            */
        }

        public static string CurrentSeasonId
        {
            get { return "HonorPoints:" + CurrentSeasonIndex; }
        }

        private const string LeaderboardCurrentSeasonIdKey = "Leaderboard:CurrentSeasonId";
        public static int CurrentSeasonIndex
        {
            get {
                if (!RedisUtils.Cache.KeyExists(LeaderboardCurrentSeasonIdKey))
                {
                    CurrentSeasonIndex = 0;
                }
                return int.Parse((string) RedisUtils.Cache.StringGet(LeaderboardCurrentSeasonIdKey));
            }

            set
            {
                RedisUtils.Cache.StringSet(LeaderboardCurrentSeasonIdKey, value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
            }
        }

        private const string LeaderboardCurrentStartTimestamp = "Leaderboard:CurrentSeasonStartTimestamp";
        public static DateTime CurrentSeasonStartTimestamp
        {
            get
            {
                if (!RedisUtils.Cache.KeyExists(LeaderboardCurrentStartTimestamp))
                {
                    CurrentSeasonStartTimestamp = DateTime.UtcNow;
                }

                return DateTime.Parse((string) RedisUtils.Cache.StringGet(LeaderboardCurrentStartTimestamp), CultureInfo.InvariantCulture);
            }

            set
            {
                RedisUtils.Cache.StringSet(LeaderboardCurrentStartTimestamp, value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
            }
        }
    }
}