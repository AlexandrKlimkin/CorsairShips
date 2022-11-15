using System;
using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict
{
    public enum ConflictsScheduleRc
    {
        Success,
        NotFound,
        AnotherConflict,
        AlreadyRunning,
        AlreadyExists
    }

    public interface IConflictsSchedulePrivate
    {
        Task<GlobalConflictState> GetCurrentConflictAsync();
        Task<long> CountAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="batchSize"></param>
        /// <returns>list of conflicts sorted by date</returns>
        Task<GlobalConflictState[]> ListConflictsAsync(int page, int batchSize);
        Task<GlobalConflictState> GetConflictAsync(string id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protoId"></param>
        /// <param name="scheduledConflictId"></param>
        /// <param name="startTime"></param>
        /// <param name="forced"></param>
        /// <returns>
        /// Success
        /// NotFound - proto not found
        /// AnotherConflict - start/end overlaps another scheduled conflict
        /// AlreadyExists - is scheduledConflictId not uniq
        /// AlreadyRunning - startTime is in the past
        /// </returns>
        Task<ConflictsScheduleRc> ScheduleConflictAsync(string protoId, string scheduledConflictId, DateTime startTime, bool forced = false);
        /// <summary>
        /// </summary>
        /// <param name="scheduledConflictId"></param>
        /// <returns>
        /// Success
        /// NotFound - conflict in progress or not found
        /// </returns>
        Task<ConflictsScheduleRc> CancelScheduledConflictAsync(string scheduledConflictId);

        Task SaveAsync(GlobalConflictState conflict);
    }
}
