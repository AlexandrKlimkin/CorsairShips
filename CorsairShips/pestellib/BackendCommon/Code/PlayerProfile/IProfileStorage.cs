using System;
using System.Threading.Tasks;
using ServerShared.PlayerProfile;

namespace ServerLib.PlayerProfile
{
    public interface IProfileStorage
    {
        Task Create(ProfileDTO profile);
        Task<ProfileDTO> Get(Guid playerId, bool noCache);
        Task<ProfileDTO[]> Get(Guid[] playerIds);
        Task<bool> IsUnique(params Guid[] playerIds);
        Task<long> RemoveExpired();

        event Action<ProfileDTO> OnProfileUpdated;
    }
}
