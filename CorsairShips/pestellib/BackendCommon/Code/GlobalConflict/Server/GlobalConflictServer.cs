using BackendCommon.Code.GlobalConflict.ApiProfiler;
using ServerLib;

namespace BackendCommon.Code.GlobalConflict.Server
{
    public class GlobalConflictServer : GlobalConflictPrivateApi
    {
        private static readonly object Sync = new object();

        public GlobalConflictServer()
        {
            var playersPriv = new PlayersPrivateServer(this);
            var conflictSchedule = new ConflictsScheduleServer(this);
            var battle = new BattleServer(this);
            var lb = new LeaderboardsServer(this);
            var cres = new ConflictResultsServer(this);
            var don = new DonationServer(this);
            var confprot = new ConflictPrototypesServer();
            var poi = new PointOfInterestServer(this);
            var debug = new DebugServer(this);

            ConflictsScheduleApi = conflictSchedule;
            PlayersApi = playersPriv;
            DonationApi = don;
            BattleApi = battle;
            LeaderboardsApi = lb;
            ConflictResultsApi = cres;
            PointOfInterestApi = poi;
            DebugApi = debug;

            DebugPrivateApi = debug;

            if (AppSettings.Default.GlobalConflictSettings.EnableProfiler)
            {
                ConflictsSchedulePrivateApi = new ConflictsScheduleProfiler(conflictSchedule);
                PlayersPrivateApi = new PlayerProfiler(playersPriv);
                DonationStagePrivateApi = new DonationStageProfiler(don);
                ConflictPrototypesPrivateApi = new ConflictPrototypesProfiler(confprot);
                BattlePrivateApi = new BattleProfiler(battle);
                LeaderboardsPrivateApi = new LeaderbordsProfiler(lb);
                ConflictResultsPrivateApi = new ConflictResultsProfiler(cres);
                PointOfInterestPrivateApi = new PointOfInterestProfiler(poi);
            }
            else
            {
                ConflictsSchedulePrivateApi = conflictSchedule;
                PlayersPrivateApi = playersPriv;
                DonationStagePrivateApi = don;
                ConflictPrototypesPrivateApi = confprot;
                BattlePrivateApi = battle;
                LeaderboardsPrivateApi = lb;
                ConflictResultsPrivateApi = cres;
                PointOfInterestPrivateApi = poi;
            }
        }
    }
}