using System;
using System.Net;
using System.Threading.Tasks;
using MasterServerProtocol;

namespace BoltLoadBalancing.MasterServer
{
    public interface IMasterServer
    {
        void UpdateState(MasterServerReport newState);
        Task<Message> CreateGame(CreateGameRequest request);
        Task<ReserveSlotResponse> ReserveSlot(ReserveSlotRequest request);
        IPAddress RemoteIP { get; }
        Guid InstanceId { get; }
        int MasterListenerPort { get; }
        float CPUUsage { get; }
        GameInfo GameInfo { get; }
        GameServerStateReport[] GameServers { get; }
    }
}