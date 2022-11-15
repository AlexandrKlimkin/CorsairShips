using System;
using System.Threading.Tasks;
using MasterServerProtocol;

namespace BoltMasterServer
{
    internal interface IGameServer
    {
        int Players { get; }
        Guid GameServerId { get; }
        int Port { get; set; }
        string IPAddress { get; set; }
        int Reserved { get; }
        TimeSpan TimeSinceUpdate { get; }
        GameServerStateReport State { get; set; }
        bool IsItTimeToShutdownServer();
        bool IsServerGoingToClose();
        Task<ReserveSlotResponse> ReserveSlot(ReserveSlotRequest request);
        void Shutdown();
    }
}