using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.Db
{
    public interface IPlayersDb
    {
        /// <summary>
        /// Создает стейт игрока в указанном конфкликте
        /// </summary>
        /// <param name="playerState"></param>
        /// <returns>false если стейт уже существует</returns>
        Task<bool> InsertAsync(PlayerState playerState);
        /// <summary>
        /// Возарвщает стейт игрока в конфликте если есть
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conflictId"></param>
        /// <returns>null если для игрока не создан стейт (см. InsertAsync)</returns>
        Task<PlayerState> GetPlayerAsync(string userId, string conflictId);
        /// <summary>
        /// Подсчитывает количество игроков конфликта в конкретной команде
        /// </summary>
        /// <param name="conflictId"></param>
        /// <param name="teamId"></param>
        /// <returns></returns>
        Task<long> GetCountTeamPlayersAsync(string conflictId, string teamId);
        /// <summary>
        /// Подсчитывает количество генералов в конфликте
        /// </summary>
        /// <param name="conflictId"></param>
        /// <returns></returns>
        Task<long> GetCountGeneralsAsync(string conflictId);
        /// <summary>
        /// Подсчитывает количество генералов в конфликте за определенную команду
        /// </summary>
        /// <param name="conflictId"></param>
        /// <param name="teamId"></param>
        /// <returns></returns>
        Task<long> GetCountGeneralsAsync(string conflictId, string teamId);
        /// <summary>
        /// Увеличивает счетчик доната в стейте игрока
        /// </summary>
        /// <param name="conflictId"></param>
        /// <param name="userId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task IncrementPlayerDonationAsync(string conflictId, string userId, int amount);
        /// <summary>
        /// Прописывает бонусы от доната в стейт игрока
        /// </summary>
        /// <param name="conflictId"></param>
        /// <param name="userId"></param>
        /// <param name="bonuses"></param>
        /// <returns></returns>
        Task GiveBonusesToPlayerAsync(string conflictId, string userId, params DonationBonus[] bonuses);
        /// <summary>
        /// Сохраняет существующий стейт игрока
        /// </summary>
        /// <param name="playerState"></param>
        /// <returns></returns>
        Task SaveAsync(PlayerState playerState);
        /// <summary>
        /// Очищает БД
        /// </summary>
        /// <returns></returns>
        Task Wipe();
        /// <summary>
        /// Устанавливает имя игрока (используется в выдаче лидбордов)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Task SetPlayerName(string userId, string name);
        /// <summary>
        /// Возвращает имена юзеров по из Id (используется в выдаче лидбордов)
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        Task<string[]> GetPlayersNames(string[] userIds);
    }
}
