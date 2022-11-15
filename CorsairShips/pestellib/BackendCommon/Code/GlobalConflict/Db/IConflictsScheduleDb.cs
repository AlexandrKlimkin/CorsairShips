using System;
using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.Db
{
    /// <summary>
    /// Интерфейс базы данных планировщика глобальных конфликтов
    /// </summary>
    public interface IConflictsScheduleDb
    {
        /// <summary>
        /// Поиск конфликта по дате
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        Task<GlobalConflictState> GetByDateAsync(DateTime dateTime);
        /// <summary>
        /// Поиск конфликта по временному отрезку
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        Task<GlobalConflictState> GetOverlappedAsync(DateTime start, DateTime end);
        /// <summary>
        /// Добавить новый конфликт в планировщик
        /// </summary>
        /// <param name="conflictState"></param>
        /// <returns>false - если такой конфликт уже существует в базе</returns>
        Task<bool> InsertAsync(GlobalConflictState conflictState);
        /// <summary>
        /// Сохраняет существующий конфликт
        /// </summary>
        /// <param name="conflictState"></param>
        /// <returns></returns>
        Task SaveAsync(GlobalConflictState conflictState);
        /// <summary>
        /// Удалить конфликт с указанным Id и временем начала конфликта после after
        /// </summary>
        /// <param name="scheduledConflictId">ид конфликта</param>
        /// <param name="after">время после которого должен начинаться конфликт (иначе вернет null, даже если есть конфликт с указанным Id)</param>
        /// <returns>количествоудаленных записей</returns>
        Task<long> DeleteAsync(string scheduledConflictId, DateTime after);
        /// <summary>
        /// Количество конфликтов в планировщике
        /// </summary>
        /// <returns></returns>
        Task<long> GetCountAsync();
        /// <summary>
        /// Достает все конфликты (отсортированны по дате по возрастанию)
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<GlobalConflictState[]> GetOrderedAsync(int page, int pageSize);
        /// <summary>
        /// Возвращает конфликтпо ид
        /// </summary>
        /// <param name="conflictId"></param>
        /// <returns>конфликт либо null, если такого конфликта нет</returns>
        Task<GlobalConflictState> GetByIdAsync(string conflictId);
        /// <summary>
        /// Очищает БД
        /// </summary>
        /// <returns></returns>
        Task Wipe();
    }
}
