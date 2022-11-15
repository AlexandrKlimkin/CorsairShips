using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.Db
{
    /// <summary>
    /// База прототипов глобальных конфликтов
    /// Используется в планировщике конфликтов
    /// </summary>
    public interface IConflictPrototypesDb
    {
        /// <summary>
        /// Возвращает количество прототипов в базе
        /// </summary>
        /// <returns></returns>
        Task<long> GetCountAsync();
        /// <summary>
        /// Возвращает указанное количество прототипов по указанному офсэту
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<GlobalConflictState[]> GetProtosAsync(int page, int pageSize);
        /// <summary>
        /// Достает прототип с указанным ид
        /// </summary>
        /// <param name="protoId"></param>
        /// <returns></returns>
        Task<GlobalConflictState> GetProtoAsync(string protoId);
        /// <summary>
        /// Добавляет новый прототип конфликта
        /// </summary>
        /// <param name="proto"></param>
        /// <returns>false - если конфликт с таким ид уже существует в базе</returns>
        Task<bool> InsertAsync(GlobalConflictState proto);
        /// <summary>
        /// Удаляет прототип с указанным ID
        /// </summary>
        /// <param name="protoId"></param>
        /// <returns></returns>
        Task Remove(string protoId);
    }
}
