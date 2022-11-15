using Backend.Code.Statistics;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict
{
    public abstract partial class GlobalConflictPrivateApi : GlobalConflictApi
    {
        public IConflictsSchedulePrivate ConflictsSchedulePrivateApi { get; protected set; }
        public IPlayersPrivate PlayersPrivateApi { get; protected set; }
        public IDonationStagePrivate DonationStagePrivateApi { get; protected set; }
        public IConflictPrototypesPrivate ConflictPrototypesPrivateApi { get; protected set; }
        public IBattlePrivate BattlePrivateApi { get; protected set; }
        public ILeaderboardsPrivate LeaderboardsPrivateApi { get; protected set; }
        public IConflictResultsPrivate ConflictResultsPrivateApi { get; protected set; }
        public IPointOfInterestPrivate PointOfInterestPrivateApi { get; protected set; }
        public IDebugPrivate DebugPrivateApi { get; protected set; }
        public DefaultStatisticsClient StatisticsClient => _statsClient;
    }
}