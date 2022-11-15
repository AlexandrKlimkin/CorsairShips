using System.Threading.Tasks;
using BackendCommon.Code.GlobalConflict.Db;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict.Server
{
    class ConflictPrototypesServer : IConflictPrototypesPrivate
    {
#pragma warning disable 0649
        [Dependency]
        private IConflictPrototypesDb _conflictPrototypesDb;
#pragma warning restore 0649

        public ConflictPrototypesServer()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        public Task<long> ConflictPrototypesCount()
        {
            return _conflictPrototypesDb.GetCountAsync();
        }

        public Task<GlobalConflictState[]> ListConflictPrototypes(int page, int batchSize)
        {
            return _conflictPrototypesDb.GetProtosAsync(page, batchSize);
        }

        public Task<GlobalConflictState> GetConflictPrototype(string id)
        {
            return _conflictPrototypesDb.GetProtoAsync(id);
        }

        public async Task<ConflictPrototypesRc> AddPrototype(GlobalConflictState globalConflict)
        {
            if(!await _conflictPrototypesDb.InsertAsync(globalConflict).ConfigureAwait(false))
                return ConflictPrototypesRc.AlreadyExists;

            return ConflictPrototypesRc.Success;
        }

        public async Task AddOrReplacePrototype(GlobalConflictState globalConflict)
        {
            var rc = await AddPrototype(globalConflict).ConfigureAwait(false);
            if(rc == ConflictPrototypesRc.Success)
                return;
            await _conflictPrototypesDb.Remove(globalConflict.Id).ConfigureAwait(false);
            if (!await _conflictPrototypesDb.InsertAsync(globalConflict).ConfigureAwait(false))
                throw null;
        }
    }
}