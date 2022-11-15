using System;
using System.Threading.Tasks;
using BackendCommon.Code.GlobalConflict.Db;
using PestelLib.ServerCommon.Extensions;
using S;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict.Server
{
    class ConflictResultsServer : IConflictResults, IConflictResultsPrivate
    {
        private GlobalConflictPrivateApi _api;
#pragma warning disable 0649
        [Dependency]
        private IConflictResultsDb _conflictResultsDb;
#pragma warning restore 0649

        public ConflictResultsServer(GlobalConflictPrivateApi api)
        {
            _api = api;
            ContainerHolder.Container.BuildUp(this);
        }

        public void GetResult(string conflictId, Action<ConflictResult> callback)
        {
            _api.ConflictResultsPrivateApi.GetResultAsync(conflictId).ResultToCallback(callback);
        }

        public Task<ConflictResult> GetResultAsync(string conflictId)
        {
            return _conflictResultsDb.GetResultAsync(conflictId);
        }

        public Task SaveAsync(ConflictResult conflictResult)
        {
            return _conflictResultsDb.InsertAsync(conflictResult);
        }
    }
}