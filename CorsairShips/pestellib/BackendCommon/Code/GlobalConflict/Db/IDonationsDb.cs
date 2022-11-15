using System.Threading.Tasks;

namespace BackendCommon.Code.GlobalConflict.Db
{
    /// <summary>
    /// База данных для учета доната
    /// </summary>
    public interface IDonationsDb
    {
        /// <summary>
        /// Запланировать добавление нового доната (добавиться в БД в необработанном стейте)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task InsertAsync(string userId, int amount);
        /// <summary>
        /// Получить необработанные донаты
        /// </summary>
        /// <param name="batchSize">максимально количество возвращаемых объектов</param>
        /// <returns></returns>
        Task<Donation[]> GetUnprocessedAsync(int batchSize);
        /// <summary>
        /// Пометить донаты как обработанные
        /// </summary>
        /// <param name="donations"></param>
        /// <returns></returns>
        Task MarkProcessedAsync(params Donation[] donations);
        /// <summary>
        /// Очищает БД
        /// </summary>
        /// <returns></returns>
        Task Wipe();
    }
}
