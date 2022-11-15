using System;

namespace BackendCommon.Code.Modules.ClassicLeaderboards
{
    public class LeaderboardConfig
    {
        public TimeSpan SeasonDuration = TimeSpan.FromDays(30);

        public int[] Leagues = new int[]
        {
            300, //0
            600, //1
            1000, //2
            2000, //3
            4000, //4
            7000, //5
            12000, //6
            20000, //7
            35000, //8
            int.MaxValue //9
        };
    }
}