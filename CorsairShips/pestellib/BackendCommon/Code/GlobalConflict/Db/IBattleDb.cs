using System.Threading.Tasks;

namespace BackendCommon.Code.GlobalConflict.Db
{
    public interface IBattleDb
    {
        /// <summary>
        /// Добавляет новую необработанную запись о результате боя
        /// </summary>
        /// <returns></returns>
        Task InsertAsync(string conflictId, string playerId, int nodeId, bool win, decimal winMod, decimal loseMod);
        /// <summary>
        /// Возвращает необработанные записи о результатах боя
        /// </summary>
        /// <param name="conflictId">ид конфликта к которому должны относиться записи</param>
        /// <param name="page">номер запрашиваемой страницы</param>
        /// <param name="batchSize">количество объектов на каждой странице (при постраничном чтении этот параметр должен быть константой, также при использовании совместно с MarkProcessed следует понимать что страница помеченная обработанной соответсвенно уменьшает количество необработанных страниц)</param>
        /// <returns></returns>
        Task<BattleResultInfo[]> GetUnprocessedAsync(string conflictId, int page, int batchSize);
        /// <summary>
        /// Помечает переданные записи о бое как обработанные, т.о. они никогда не будут возвращены методом GetUnprocessed
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        Task MarkProcessedAsync(BattleResultInfo[] results);
        /// <summary>
        /// Очищает БД
        /// </summary>
        /// <returns></returns>
        Task Wipe();
    }
}
