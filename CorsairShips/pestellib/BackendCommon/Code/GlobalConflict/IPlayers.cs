using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict
{
    public enum ConflictPrototypesRc
    {
        Success,
        AlreadyExists,
    }

    public interface IConflictPrototypesPrivate
    {
        Task<long> ConflictPrototypesCount();
        Task<GlobalConflictState[]> ListConflictPrototypes(int page, int batchSize);
        Task<GlobalConflictState> GetConflictPrototype(string id);
        Task<ConflictPrototypesRc> AddPrototype(GlobalConflictState globalConflict);
        Task AddOrReplacePrototype(GlobalConflictState globalConflict);
    }

    public interface IPlayersPrivate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conflictId"></param>
        /// <param name="userId"></param>
        /// <param name="teamId"></param>
        /// <returns>null если в момент вызова нет активных конфликтов или конфликт в стадии завершения. также вернет null, если стейт уже существует</returns>
        Task<PlayerState> RegisterAsync(string conflictId, string userId, string teamId);
        Task<PlayerState> GetPlayerAsync(string userId, string conflictId);
        /// <summary>
        /// Подсчитывает количество игроков конфликта для каждой из сторон (может использоваться для простой балансировки при автоматическом распределении в команды)
        /// </summary>
        /// <param name="conflictId"></param>
        /// <returns>null если в момент вызова нет активных конфликтов</returns>
        Task<TeamPlayersStat> GetTeamPlayersStatAsync(string conflictId);
        Task<long> CountGeneralsAsync(string conflictId);
        Task AddDonationAsync(string conflictId, Donation donation);
        Task GiveBonusesToPlayerAsync(string conflictId, string userId, params DonationBonus[] bonus);
        Task SaveAsync(PlayerState playerState);
        Task SetNameAsync(string userId, string name);
        Task<string[]> GetNamesAsync(string[] userIds);
    }
}
