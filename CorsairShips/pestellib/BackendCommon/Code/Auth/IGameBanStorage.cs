using System;
using System.Collections.Generic;

namespace BackendCommon.Code.Auth
{
    public class GameBanStorageItem
    {
        public Guid PlayerId;
        public string Reason;
    }


    public interface IGameBanStorage
    {
        bool IsBanned(Guid userId, string deviceId, out string reason);
        bool Ban(Guid userId, string reason);
        bool Unban(Guid userId);
        IEnumerable<GameBanStorageItem> ListBans();
    }
}
