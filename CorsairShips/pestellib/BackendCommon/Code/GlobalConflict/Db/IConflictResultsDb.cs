using System.Threading.Tasks;
using S;

namespace BackendCommon.Code.GlobalConflict.Db
{
    public interface IConflictResultsDb
    {
        /// <summary>
        /// Возвращает результат конфликта по ид
        /// </summary>
        /// <param name="conflictId"></param>
        /// <returns>null если результат не найден</returns>
        Task<ConflictResult> GetResultAsync(string conflictId);
        /// <summary>
        /// Добавляет новый результат конфликта
        /// </summary>
        /// <param name="conflictResult"></param>
        /// <returns>false если результат для указанного конфликта уже присутствует в базе</returns>
        Task<bool> InsertAsync(ConflictResult conflictResult);
        /// <summary>
        /// Очищает БД
        /// </summary>
        /// <returns></returns>
        Task Wipe();
    }
}
