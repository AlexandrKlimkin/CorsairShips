using System;
using System.Threading.Tasks;
using BackendCommon.Code.Modules.ClassicLeaderboards;
using Quartz;
using UnityDI;

namespace BackendCommon.Code.Jobs
{
    [DisallowConcurrentExecution]
    public class RedisUpdateCurrentSeasonIndex : IJob
    {
#pragma warning disable 0649
        [Dependency] private LeaderboardConfig _leaderboardConfig;
#pragma warning restore 0649

        public RedisUpdateCurrentSeasonIndex()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        public Task Execute(IJobExecutionContext context)
        {
            var seasonStartTimestamp = LeaderboardUtils.CurrentSeasonStartTimestamp;
            var seasonEndTimestamp = seasonStartTimestamp + _leaderboardConfig.SeasonDuration;
            if (DateTime.UtcNow > seasonEndTimestamp)
            {
                LeaderboardUtils.CurrentSeasonIndex++;
                LeaderboardUtils.CurrentSeasonStartTimestamp = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }
    }
}