using System.Threading.Tasks;
using MasterServerProtocol;
using System;

namespace BoltMasterServer
{
    internal interface IGameServerLauncher
    {
        void StopAllProcesses();
        Task<JoinInfo> StartNewServerInstance(string matchmakingData);
        bool KillProcess(Guid gameServerId);
    }
}