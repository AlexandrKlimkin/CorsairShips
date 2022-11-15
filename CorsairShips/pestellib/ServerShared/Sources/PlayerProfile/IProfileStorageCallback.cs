using System;

namespace ServerShared.PlayerProfile
{
    public interface IProfileStorageCallback
    {
        void Get(Guid playerId, Action<bool, ProfileDTO> callback);
        void Get(Guid[] playerId, Action<bool, ProfileDTO[]> callback);
        void Put(ProfileDTO[] profiles, Action<bool> callback);
    }
}