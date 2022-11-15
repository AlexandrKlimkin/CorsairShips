using System;

namespace PestelLib.ServerCommon.Utils
{
    public interface IPlayerIpResolver
    {
        string GetPlayerIp(Guid playerId);
    }
}