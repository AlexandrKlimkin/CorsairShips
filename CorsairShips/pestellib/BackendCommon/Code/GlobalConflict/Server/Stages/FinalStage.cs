using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using S;

namespace BackendCommon.Code.GlobalConflict.Server.Stages
{
    public class FinalStage : Stage
    {
        public override async Task<bool> HasWork()
        {
            var curr = await _api.ConflictsSchedulePrivateApi.GetCurrentConflictAsync().ConfigureAwait(false);
            var result = await _api.ConflictResultsPrivateApi.GetResultAsync(curr.Id).ConfigureAwait(false);
            return result == null;
        }

        public override async Task<bool> Update()
        {
            var conflictState = await _api.ConflictsSchedulePrivateApi.GetCurrentConflictAsync().ConfigureAwait(false);
            var exists = await _api.ConflictResultsPrivateApi.GetResultAsync(conflictState.Id).ConfigureAwait(false);
            if (exists != null)
                return false;
            var sw = Stopwatch.StartNew();
            using (await _lockManager.LockAsync(StageLock, 0, false).ConfigureAwait(false))
            {
                var conflict = new Conflict(conflictState);
                var rp = conflict.GetResultPointsByTeam();
                var result = new ConflictResult
                {
                    ConflictId = conflictState.Id,
                    ResultPoints = rp,
                    WinnerTeam = rp.Count > 0 ? rp.OrderByDescending(_ => _.Points).First().Team : null
                };
                await _api.ConflictResultsPrivateApi.SaveAsync(result).ConfigureAwait(false);
            }
            _api.StatisticsClient?.SendAsync(GlobalConflictPrivateApi._statCategory, "final_stage_time", sw.ElapsedMilliseconds);
            return true;
        }
    }
}