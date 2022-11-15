using System.Threading.Tasks;
using ServerShared.GlobalConflict;

namespace BackendCommon.Code.GlobalConflict.ApiProfiler
{
    class PlayerProfiler : IPlayersPrivate
    {
        private readonly IPlayersPrivate _original;

        public PlayerProfiler(IPlayersPrivate original)
        {
            _original = original;
        }

        public async Task<PlayerState> RegisterAsync(string conflictId, string userId, string teamId)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IPlayersPrivate_RegisterAsync"))
            {
                return await _original.RegisterAsync(conflictId, userId, teamId).ConfigureAwait(false);
            }
        }

        public async Task<PlayerState> GetPlayerAsync(string userId, string conflictId)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IPlayersPrivate_GetPlayerAsync"))
            {
                return await _original.GetPlayerAsync(userId, conflictId).ConfigureAwait(false);
            }
        }

        public async Task<TeamPlayersStat> GetTeamPlayersStatAsync(string conflictId)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IPlayersPrivate_GetTeamPlayersStatAsync"))
            {
                return await _original.GetTeamPlayersStatAsync(conflictId).ConfigureAwait(false);
            }
        }

        public async Task<long> CountGeneralsAsync(string conflictId)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IPlayersPrivate_CountGeneralsAsync"))
            {
                return await _original.CountGeneralsAsync(conflictId).ConfigureAwait(false);
            }
        }

        public async Task AddDonationAsync(string conflictId, Donation donation)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IPlayersPrivate_AddDonationAsync"))
            {
                await _original.AddDonationAsync(conflictId, donation).ConfigureAwait(false);
            }
        }

        public async Task GiveBonusesToPlayerAsync(string conflictId, string userId, params DonationBonus[] bonus)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IPlayersPrivate_GiveBonusesToPlayerAsync"))
            {
                await _original.GiveBonusesToPlayerAsync(conflictId, userId, bonus).ConfigureAwait(false);
            }
        }

        public async Task SaveAsync(PlayerState playerState)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IPlayersPrivate_SaveAsync"))
            {
                await _original.SaveAsync(playerState).ConfigureAwait(false);
            }
        }

        public async Task SetNameAsync(string userId, string name)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IPlayersPrivate_SetNameAsync"))
            {
                await _original.SetNameAsync(userId, name).ConfigureAwait(false);
            }
        }

        public async Task<string[]> GetNamesAsync(string[] userIds)
        {
            using (new ProfileGuard(GlobalConflictPrivateApi._statCategory, "IPlayersPrivate_GetNamesAsync"))
            {
                return await _original.GetNamesAsync(userIds).ConfigureAwait(false);
            }
        }
    }
}