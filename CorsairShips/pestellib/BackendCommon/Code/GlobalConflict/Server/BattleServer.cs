using System;
using System.Threading.Tasks;
using BackendCommon.Code.GlobalConflict.Db;
using PestelLib.ServerCommon.Extensions;
using ServerShared.GlobalConflict;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict.Server
{
    class BattleServer : IBattle, IBattlePrivate
    {
        protected GlobalConflictPrivateApi _api;
#pragma warning disable 0649
        [Dependency]
        private IBattleDb _battleDb;
#pragma warning restore 0649

        public BattleServer(GlobalConflictPrivateApi api)
        {
            _api = api;
            ContainerHolder.Container.BuildUp(this);
        }

        public void RegisterBattleResult(string playerId, int nodeId, bool win, decimal winMod, decimal loseMod, Action callback)
        {
            _api.BattlePrivateApi.RegisterBattleResultAsync(playerId, nodeId, win, winMod, loseMod).ResultToCallback(callback);
        }

        public async Task RegisterBattleResultAsync(string playerId, int nodeId, bool win, decimal winMod, decimal loseMod)
        {
            var conflict = await _api.ConflictsSchedulePrivateApi.GetCurrentConflictAsync().ConfigureAwait(false);
            await _battleDb.InsertAsync(conflict.Id, playerId, nodeId, win, winMod, loseMod).ConfigureAwait(false);
        }

        public async Task<BattleResultInfo[]> UnprocessedBattlesAsync(int page, int batchSize)
        {
            var conflict = await _api.ConflictsSchedulePrivateApi.GetCurrentConflictAsync().ConfigureAwait(false);
            return await _battleDb.GetUnprocessedAsync(conflict.Id, page, batchSize).ConfigureAwait(false);
        }

        public Task BattlesProcessedAsync(params BattleResultInfo[] results)
        {
            return _battleDb.MarkProcessedAsync(results);
        }
    }
}