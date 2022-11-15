using System.Threading.Tasks;

namespace BackendCommon.Code.GlobalConflict.ApiProfiler
{
    class BattleProfiler : IBattlePrivate
    {
        private readonly IBattlePrivate _original;

        public BattleProfiler(IBattlePrivate original)
        {
            _original = original;
        }

        public async Task RegisterBattleResultAsync(string playerId, int nodeId, bool win, decimal winMod, decimal loseMod)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IBattlePrivate_RegisterBattleResultAsync"))
            {
                await _original.RegisterBattleResultAsync(playerId, nodeId, win, winMod, loseMod).ConfigureAwait(false);
            }
        }

        public async Task<BattleResultInfo[]> UnprocessedBattlesAsync(int page, int batchSize)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IBattlePrivate_UnprocessedBattlesAsync"))
            {
                return await _original.UnprocessedBattlesAsync(page, batchSize).ConfigureAwait(false);
            }
        }

        public async Task BattlesProcessedAsync(params BattleResultInfo[] results)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IBattlePrivate_BattlesProcessedAsync"))
            {
                await _original.BattlesProcessedAsync(results).ConfigureAwait(false);
            }
        }
    }
}