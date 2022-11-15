using System;
using System.Threading.Tasks;
using BackendCommon.Code.GlobalConflict.ApiProfiler;
using ServerLib.PlayerProfile;
using ServerShared.PlayerProfile;

namespace Backend.Code.PlayerProfile
{
    public class ProfileStorageBenchmark : IProfileStorage
    {
        private readonly IProfileStorage _original;
        private const string _statCat = "prof_st";

        public ProfileStorageBenchmark(IProfileStorage original)
        {
            _original = original;
        }

        public event Action<ProfileDTO> OnProfileUpdated
        {
            add { _original.OnProfileUpdated += value; }
            remove { _original.OnProfileUpdated -= value; }
        }

        public async Task Create(ProfileDTO profile)
        {
            using (new ProfileGuard(_statCat, "Create"))
            {
                await _original.Create(profile);
            }
        }

        public Task<ProfileDTO> Get(Guid playerId, bool noCache)
        {
            if (noCache)
                return GetNoCache(playerId);
            return GetFromCache(playerId);
        }

        public async Task<ProfileDTO[]> Get(Guid[] playerIds)
        {
            using (new ProfileGuard(_statCat, $"Get_{playerIds.Length}"))
            {
                return await _original.Get(playerIds);
            }
        }

        public async Task<bool> IsUnique(params Guid[] playerIds)
        {
            using (new ProfileGuard(_statCat, $"IsUnique_{playerIds.Length}"))
            {
                return await _original.IsUnique(playerIds);
            }
        }

        public async Task<long> RemoveExpired()
        {
            using (new ProfileGuard(_statCat, $"RemoveExpired"))
            {
                return await _original.RemoveExpired();
            }
        }

        private async Task<ProfileDTO> GetFromCache(Guid playerId)
        {
            using (new ProfileGuard(_statCat, "GetFromCache"))
            {
                return await _original.Get(playerId, false);
            }
        }
        private async Task<ProfileDTO> GetNoCache(Guid playerId)
        {
            using (new ProfileGuard(_statCat, "GetFromCache"))
            {
                return await _original.Get(playerId, true);
            }
        }
    }
}