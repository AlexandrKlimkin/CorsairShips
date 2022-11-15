using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.ApiProfiler
{
    class ConflictPrototypesProfiler: IConflictPrototypesPrivate
    {
        private readonly IConflictPrototypesPrivate _original;

        public ConflictPrototypesProfiler(IConflictPrototypesPrivate original)
        {
            _original = original;
        }

        public async Task<long> ConflictPrototypesCount()
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictPrototypesPrivate_ConflictPrototypesCount"))
            {
                return await _original.ConflictPrototypesCount().ConfigureAwait(false);
            }
        }

        public async Task<GlobalConflictState[]> ListConflictPrototypes(int page, int batchSize)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictPrototypesPrivate_ListConflictPrototypes"))
            {
                return await _original.ListConflictPrototypes(page, batchSize).ConfigureAwait(false);
            }
        }

        public async Task<GlobalConflictState> GetConflictPrototype(string id)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictPrototypesPrivate_GetConflictPrototype"))
            {
                return await _original.GetConflictPrototype(id).ConfigureAwait(false);
            }
        }

        public async Task<ConflictPrototypesRc> AddPrototype(GlobalConflictState globalConflict)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictPrototypesPrivate_AddPrototype"))
            {
                return await _original.AddPrototype(globalConflict).ConfigureAwait(false);
            }
        }

        public async Task AddOrReplacePrototype(GlobalConflictState globalConflict)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IConflictPrototypesPrivate_AddOrReplacePrototype"))
            {
                await _original.AddOrReplacePrototype(globalConflict).ConfigureAwait(false);
            }
        }
    }
}