using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendCommon.Code.GlobalConflict.Db;
using log4net;
using PestelLib.ServerCommon.Extensions;
using ServerLib;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict.Server
{
    public class DebugServer : IDebug, IDebugPrivate
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DebugServer));
        protected GlobalConflictPrivateApi _api;
        [Dependency] protected IConflictsScheduleDb _scheduleDb;
        [Dependency] protected IBattleDb _battleDb;
        [Dependency] protected IConflictPrototypesDb _conflictPrototypesDb;
        [Dependency] protected IConflictResultsDb _conflictResultsDb;
        [Dependency] protected IDonationsDb _donationsDb;
        [Dependency] protected ILeaderboardsDb _leaderboardsDb;
        [Dependency] protected IPlayersDb _playersDb;
        [Dependency] protected IPointsOfInterestsDb _pointsOfInterestsDb;

        public DebugServer(GlobalConflictPrivateApi api)
        {
            _api = api;
            ContainerHolder.Container.BuildUp(this);
        }

        public void AddTime(int secondsToAdd, Action callback)
        {
            AddTimeAsync(secondsToAdd).ResultToCallback(callback);
        }

        public void StartConflict(string id, Action<string> callback)
        {
            StartConflictAsync(id).ResultToCallback(callback);
        }

        private void DebugCheck()
        {
            if (!AppSettings.Default.GlobalConflictSettings.DebugEnabled)
            {
                throw new Exception("GlobalConflict not in debug mode");
            }
        }

        public async Task<string> StartConflictAsync(string id)
        {
            DebugCheck();
            var r = await _api.ConflictPrototypesPrivateApi.GetConflictPrototype(id).ConfigureAwait(false);
            if (r == null)
            {
                throw new KeyNotFoundException();
            }

            await WipeDb().ConfigureAwait(false);
            var dt = DateTime.UtcNow;
            var fix = 0;
            var scheduledId = "";
            ConflictsScheduleRc rc;
            do
            {
                scheduledId = id + dt.ToString("yyyyMMdd") + (fix++ > 0 ? $"_{fix}" : "");
                rc = await _api.ConflictsSchedulePrivateApi.ScheduleConflictAsync(id, scheduledId, dt, true).ConfigureAwait(false);
            } while (rc == ConflictsScheduleRc.AlreadyExists);

            if(rc != ConflictsScheduleRc.Success)
                Log.Debug($"ConflictsSchedulePrivateApi returned {rc}");

            return scheduledId;
        }

        public void StartConflict(GlobalConflictState prototype, Action<string> callback)
        {
            StartConflictAsync(prototype).ResultToCallback(callback);
        }

        public async Task<string> StartConflictAsync(GlobalConflictState prototype)
        {
            DebugCheck();
            await _api.ConflictPrototypesPrivateApi.AddOrReplacePrototype(prototype).ConfigureAwait(false);
            return await StartConflictAsync((string) prototype.Id).ConfigureAwait(false);
        }

        public void ListConflictPrototypes(Action<string[]> callback)
        {
            ListConflictPrototypesAsync().ResultToCallback(callback);
        }

        public async Task<string[]> ListConflictPrototypesAsync()
        {
            DebugCheck();
            var prots = await _api.ConflictPrototypesPrivateApi.ListConflictPrototypes(0, 1000).ConfigureAwait(false);
            var result = Enumerable.ToArray<string>(prots.Select(_ => _.Id));
            return result;
        }

        private async Task WipeDb()
        {
            await Task.WhenAll(
                _scheduleDb.Wipe(),
                _playersDb.Wipe(),
                _battleDb.Wipe(),
                _conflictResultsDb.Wipe(),
                _donationsDb.Wipe(),
                _pointsOfInterestsDb.Wipe()
            ).ConfigureAwait(false);
        }

        public async Task AddTimeAsync(int secondsToAdd)
        {
            DebugCheck();
            var conflictState = await _scheduleDb.GetByDateAsync(DateTime.UtcNow).ConfigureAwait(false);
            var offset = TimeSpan.FromSeconds(secondsToAdd);
            conflictState.StartTime -= offset;
            conflictState.EndTime -= offset;
            await _scheduleDb.SaveAsync(conflictState).ConfigureAwait(false);
            await _api.Update().ConfigureAwait(false);
        }
    }
}