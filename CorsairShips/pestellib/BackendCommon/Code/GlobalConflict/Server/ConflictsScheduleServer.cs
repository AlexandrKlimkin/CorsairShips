using System;
using System.Linq;
using System.Threading.Tasks;
using BackendCommon.Code.GlobalConflict.Db;
using PestelLib.ServerCommon.Extensions;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict.Server
{

    class ConflictsScheduleServer : IConflictsSchedule, IConflictsSchedulePrivate
    {
        private GlobalConflictPrivateApi _api;
#pragma warning disable 0649
        [Dependency]
        private IConflictsScheduleDb _conflictsScheduleDb;
#pragma warning restore 0649

        public ConflictsScheduleServer(GlobalConflictPrivateApi api)
        {
            _api = api;
            ContainerHolder.Container.BuildUp(this);
        }

        public Task<GlobalConflictState> GetCurrentConflictAsync()
        {
            var dt = DateTime.UtcNow;
            return _conflictsScheduleDb.GetByDateAsync(dt);
        }

        public void GetCurrentConflict(Action<GlobalConflictState> callback)
        {
            GetCurrentConflictAsync().ResultToCallback(callback);
        }

        public GlobalConflictState GetCurrentConflict()
        {
            return GetCurrentConflictAsync().Result;
        }

        public Task<long> CountAsync()
        {
            return _conflictsScheduleDb.GetCountAsync();
        }

        public Task<GlobalConflictState[]> ListConflictsAsync(int page, int batchSize)
        {
            return _conflictsScheduleDb.GetOrderedAsync(page, batchSize);
        }

        public Task<GlobalConflictState> GetConflictAsync(string id)
        {
            return _conflictsScheduleDb.GetByIdAsync(id);
        }

        public async Task<ConflictsScheduleRc> ScheduleConflictAsync(string protoId, string scheduledConflictId, DateTime startTime, bool forced = false)
        {
            if (!forced && startTime < DateTime.UtcNow)
                return ConflictsScheduleRc.AlreadyRunning;
            var proto = await _api.ConflictPrototypesPrivateApi.GetConflictPrototype(protoId).ConfigureAwait(false);
            if (proto == null)
                return ConflictsScheduleRc.NotFound;
            var conflict = new Conflict(proto);
            var endTime = startTime + conflict.Period;
            var intersects = await _conflictsScheduleDb.GetOverlappedAsync(startTime.Date, endTime).ConfigureAwait(false) != null;
            if (intersects)
                return ConflictsScheduleRc.AnotherConflict;

            proto.Id = scheduledConflictId;
            proto.StartTime = startTime;
            proto.EndTime = endTime;
            proto.TeamsStates = proto.Teams?.Select(_ => new TeamState() {Id = _}).ToArray();

            if (!await _conflictsScheduleDb.InsertAsync(proto).ConfigureAwait(false))
                return ConflictsScheduleRc.AlreadyExists;

            return ConflictsScheduleRc.Success;
        }

        public async Task<ConflictsScheduleRc> CancelScheduledConflictAsync(string scheduledConflictId)
        {
            var removed = await _conflictsScheduleDb.DeleteAsync(scheduledConflictId, DateTime.UtcNow).ConfigureAwait(false);
            if (removed < 1)
                return ConflictsScheduleRc.NotFound;

            return ConflictsScheduleRc.Success;
        }

        public Task SaveAsync(GlobalConflictState conflict)
        {
            return _conflictsScheduleDb.SaveAsync(conflict);
        }
    }
}