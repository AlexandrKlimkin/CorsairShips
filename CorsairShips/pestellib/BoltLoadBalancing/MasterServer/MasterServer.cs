using System;
using System.Net;
using System.Threading.Tasks;
using MasterServerProtocol;

namespace BoltLoadBalancing.MasterServer
{
    public class MasterServer : IMasterServer
    {
        private readonly MasterServerConnection _connection;
        private MasterServerReport _state;

        public MasterServer(MasterServerConnection connection, MasterServerReport state)
        {
            _connection = connection;
            _state = state;
        }

        public void UpdateState(MasterServerReport newState)
        {
            _state = newState;
        }

        public Task<Message> CreateGame(CreateGameRequest request)
        {
            return _connection.SendMessageAsync<Message>(request);
        }

        public Task<ReserveSlotResponse> ReserveSlot(ReserveSlotRequest request)
        {
            return _connection.SendMessageAsync<ReserveSlotResponse>(request);
        }

        public IPAddress RemoteIP => _connection.RemoteIP;
        public Guid InstanceId => _state.InstanceId;
        public int MasterListenerPort => _state.MasterListenerPort;
        public float CPUUsage => _state.CPUUsage;
        public GameInfo GameInfo => _state.GameInfo;
        public GameServerStateReport[] GameServers => _state.GameServers;
    }
}
