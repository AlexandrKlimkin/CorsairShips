using System;
using System.Threading.Tasks;

namespace BackendCommon.Code.GlobalConflict
{
    public class BattleResultInfo
    {
        public Guid Id;
        public DateTime Time = DateTime.UtcNow;
        public string ConflictId;
        public string PlayerId;
        public int NodeId;
        public bool Win;
        public decimal WinMod;
        public decimal LoseMod;
    }

    public interface IBattlePrivate
    {
        Task RegisterBattleResultAsync(string playerId, int nodeId, bool win, decimal winMod, decimal loseMod);
        Task<BattleResultInfo[]> UnprocessedBattlesAsync(int page, int batchSize);
        Task BattlesProcessedAsync(params BattleResultInfo[] results);
    }
}