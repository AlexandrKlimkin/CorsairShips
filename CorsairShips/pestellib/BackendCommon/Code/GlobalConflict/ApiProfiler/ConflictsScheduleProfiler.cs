using System;
using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.ApiProfiler
{
    class ConflictsScheduleProfiler: IConflictsSchedulePrivate
    {
        private readonly IConflictsSchedulePrivate _original;

        public ConflictsScheduleProfiler(IConflictsSchedulePrivate original)
        {
            _original = original;
        }

        public async Task<GlobalConflictState> GetCurrentConflictAsync()
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictsSchedulePrivate_GetCurrentConflictAsync"))
            {
                return await _original.GetCurrentConflictAsync().ConfigureAwait(false);
            }
        }

        public async Task<long> CountAsync()
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictsSchedulePrivate_CountAsync"))
            {
                return await _original.CountAsync().ConfigureAwait(false);
            }
        }

        public async Task<GlobalConflictState[]> ListConflictsAsync(int page, int batchSize)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictsSchedulePrivate_ListConflictsAsync"))
            {
                return await _original.ListConflictsAsync(page, batchSize).ConfigureAwait(false);
            }
        }

        public async Task<GlobalConflictState> GetConflictAsync(string id)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictsSchedulePrivate_GetConflictAsync"))
            {
                return await _original.GetConflictAsync(id).ConfigureAwait(false);
            }
        }

        public async Task<ConflictsScheduleRc> ScheduleConflictAsync(string protoId, string scheduledConflictId, DateTime startTime, bool forced = false)
        {
            using(new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictsSchedulePrivate_ScheduleConflictAsync"))
            {
                return await _original.ScheduleConflictAsync(protoId, scheduledConflictId, startTime, forced).ConfigureAwait(false);
            }
        }

        public async Task<ConflictsScheduleRc> CancelScheduledConflictAsync(string scheduledConflictId)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictsSchedulePrivate_CancelScheduledConflictAsync"))
            {
                return await _original.CancelScheduledConflictAsync(scheduledConflictId).ConfigureAwait(false);
            }
        }

        public async Task SaveAsync(GlobalConflictState conflict)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictsSchedulePrivate_SaveAsync"))
            {
                await _original.SaveAsync(conflict).ConfigureAwait(false);
            }
        }
    }
}