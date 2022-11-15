using System;
using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.Db
{
    public interface IPointsOfInterestsDb
    {
        /// <summary>
        /// Возвращает указанную точку интереса по ид если есть
        /// </summary>
        /// <param name="conflictId"></param>
        /// <param name="poiId"></param>
        /// <param name="team"></param>
        /// <returns>null такой точки интереса нет в базе (возможноона не выставлена)</returns>
        Task<PointOfInterest> GetByIdAsync(string conflictId, string poiId, string team);
        /// <summary>
        /// Возвращает все точки интереса выставленные конкретной командой
        /// </summary>
        /// <param name="conflictId"></param>
        /// <param name="teamId"></param>
        /// <returns></returns>
        Task<PointOfInterest[]> GetByTeam(string conflictId, string teamId);
        /// <summary>
        /// Ищет задеплоеную точку в указанной ноде у определенной команды
        /// </summary>
        /// <param name="conflictId"></param>
        /// <param name="teamId"></param>
        /// <param name="nodeId"></param>
        /// <returns>null если точка в ноде не установлена</returns>
        Task<PointOfInterest> GetByNode(string conflictId, string teamId, int nodeId);
            /// <summary>
        /// Создает новую точку интереса
        /// </summary>
        /// <param name="conflictId"></param>
        /// <param name="playerId"></param>
        /// <param name="team"></param>
        /// <param name="nodeId"></param>
        /// <param name="data"></param>
        /// <param name="updateTime"></param>
        /// <returns></returns>
        Task InsertAsync(string conflictId, string playerId, string team, int nodeId, PointOfInterest data, DateTime updateTime);

        /// <summary>
        /// Обновляет существующую точку интереса
        /// </summary>
        /// <param name="conflictId"></param>
        /// <param name="deployedPoiId"></param>
        /// <param name="team"></param>
        /// <param name="nodeId"></param>
        /// <param name="updateTime"></param>
        /// <param name="deployCooldown"></param>
        /// <param name="bonusTime"></param>
        /// <returns></returns>
        Task UpdateAsync(string conflictId, string deployedPoiId, string team, int nodeId, DateTime updateTime, TimeSpan deployCooldown, TimeSpan bonusTime);
        /// <summary>
        /// Очищает БД
        /// </summary>
        /// <returns></returns>
        Task Wipe();
    }
}
