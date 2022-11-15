using System.Threading.Tasks;
using S;

namespace BackendCommon.Code.GlobalConflict.ApiProfiler
{
    class ConflictResultsProfiler : IConflictResultsPrivate
    {
        private readonly IConflictResultsPrivate _original;

        public ConflictResultsProfiler(IConflictResultsPrivate original)
        {
            _original = original;
        }

        public async Task<ConflictResult> GetResultAsync(string conflictId)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictResultsPrivate_GetResultAsync"))
            {
                return await _original.GetResultAsync(conflictId).ConfigureAwait(false);
            }
        }

        public async Task SaveAsync(ConflictResult conflictResult)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictResultsPrivate_SaveAsync"))
            {
                await _original.SaveAsync(conflictResult).ConfigureAwait(false);
            }
        }
    }
}