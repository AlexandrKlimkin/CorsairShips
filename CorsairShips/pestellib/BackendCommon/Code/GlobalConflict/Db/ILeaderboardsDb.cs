using System;
using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.Db
{
    public interface ILeaderboardsDb
    {
        /// <summary>
        /// Вычисляет положение юзера в топе по донатам, если найдены другие юзеры с идентичным количеством очков, 
        /// то выше тот кто раньше зарегался
        /// </summary>
        /// <param name="conflictId"></param>
        /// <param name="userId"></param>
        /// <param name="team">null - среди всех команд</param>
        /// <param name="userPoints"></param>
        /// <param name="userRegTime"></param>
        /// <returns></returns>
        Task<long> GetDonationTopMyPositionAsync(string conflictId, string userId, string team, int userPoints, DateTime userRegTime);

        /// <summary>
        /// Возвращает указанное количество юзеров из топа доната
        /// </summary>
        /// <param name="conflictId">топ какого конфликта</param>
        /// <param name="team">если null то среди всех команд</param>
        /// <param name="page">смещение</param>
        /// <param name="pageSize">количетсво объектов в выборке</param>
        /// <returns></returns>
        Task<PlayerState[]> GetDonationTopAsync(string conflictId, string team, int page, int pageSize);
        /// <summary>
        /// Вычисляет положение юзера в топе по очкам победы, если найдены другие юзеры с идентичным количеством очков, 
        /// то выше тот кто раньше зарегался
        /// </summary>
        /// <param name="conflictId"></param>
        /// <param name="userId"></param>
        /// <param name="team"></param>
        /// <param name="userPoints"></param>
        /// <param name="userRegTime"></param>
        /// <returns></returns>
        Task<long> GetWinPointsTopMyPositionAsync(string conflictId, string userId, string team, int userPoints, DateTime userRegTime);
        /// <summary>
        /// Возвращает указанное количество юзеров из топа очков победы
        /// </summary>
        /// <param name="conflictId">топ какого конфликта</param>
        /// <param name="team"></param>
        /// <param name="page">смещение</param>
        /// <param name="pageSize">количетсво объектов в выборке</param>
        /// <returns></returns>
        Task<PlayerState[]> GetWinPointsTopAsync(string conflictId, string team, int page, int pageSize);
    }
}
