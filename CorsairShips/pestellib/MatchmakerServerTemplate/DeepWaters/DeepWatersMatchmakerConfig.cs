using System;
using System.Collections.Generic;
using PestelLib.MatchmakerServer.Config;

namespace PestelLib.MatchmakerServer.DeepWaters
{
    class DeepWatersMatchmakerTierInfo
    {
        public int MinPower;
        public int MaxPower;
        public int MaxPowerDiff;
    }

    class DeepWatersMatchmakerConfig : MatchmakerConfig
    {
        public int TeamCapacity = 6;
        public float MaxErrorCoeff = 0.25f;
        public int BucketsCount = 3;
        public TimeSpan MaxSearchTime = TimeSpan.FromSeconds(15);
        public TimeSpan MaxWaitPlayersTime = TimeSpan.FromSeconds(10);
        public Dictionary<int, DeepWatersMatchmakerTierInfo> TiersInfo = new Dictionary<int, DeepWatersMatchmakerTierInfo>
            {
                { 1, new DeepWatersMatchmakerTierInfo() { MinPower = 200, MaxPower = 400, MaxPowerDiff = 200 } },
                { 2, new DeepWatersMatchmakerTierInfo() { MinPower = 400, MaxPower = 800, MaxPowerDiff = 400 } },
                { 3, new DeepWatersMatchmakerTierInfo() { MinPower = 800, MaxPower = 1200, MaxPowerDiff = 600 } },
                { 4, new DeepWatersMatchmakerTierInfo() { MinPower = 800, MaxPower = 1600, MaxPowerDiff = 800 } },
                { 5, new DeepWatersMatchmakerTierInfo() { MinPower = 1000, MaxPower = 2000, MaxPowerDiff = 1000 } },
                { 6, new DeepWatersMatchmakerTierInfo() { MinPower = 1200, MaxPower = 2400, MaxPowerDiff = 1200 } },
            };
    }
}
