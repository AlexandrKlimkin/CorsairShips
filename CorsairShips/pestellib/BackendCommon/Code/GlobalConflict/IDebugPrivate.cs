using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict
{
    public interface IDebugPrivate
    {
        Task AddTimeAsync(int secondsToAdd);
        Task<string> StartConflictAsync(string id);
        Task<string> StartConflictAsync(GlobalConflictState prototype);
        Task<string[]> ListConflictPrototypesAsync();
    }
}
