using System.Threading.Tasks;
using S;

namespace BackendCommon.Code.GlobalConflict
{
    public interface IConflictResultsPrivate
    {
        Task<ConflictResult> GetResultAsync(string conflictId);
        Task SaveAsync(ConflictResult conflictResult);
    }
}