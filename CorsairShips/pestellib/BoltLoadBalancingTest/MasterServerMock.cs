using System;
using System.Net;
using System.Threading.Tasks;
using BoltLoadBalancing.MasterServer;
using MasterServerProtocol;

namespace BoltLoadBalancingTest
{
    public class MasterServerMock : IMasterServer
    {
        public bool CreateGameWasCalled = false;
        public bool ReserveSlotWasCalled = false;
        public bool ForceReserveToAlwaysFail = false;
        
        public void UpdateState(MasterServerReport newState)
        {
            throw new NotImplementedException();
        }

        public async Task<Message> CreateGame(CreateGameRequest request)
        {
            await Task.Delay(10);
            CreateGameWasCalled = true;
            return new CreateGameResponse()
            {
                JoinInfo = new JoinInfo()
                {
                    Port = 123,
                    IpAddress = "127.0.0.1",
                    ReservedSlot = ReservedSlot
                },
                MessageId = request.MessageId
            };
        }

        public Guid ReservedSlot => Guid.Parse("6ceb4571-ff5a-4839-8a35-3e85fa6dfbfc");

        public async Task<ReserveSlotResponse> ReserveSlot(ReserveSlotRequest request)
        {
            await Task.Delay(10);
            ReserveSlotWasCalled = true;
            if (ForceReserveToAlwaysFail)
            {
                return new ReserveSlotResponse() {Succeed = false};
            }
            return new ReserveSlotResponse()
            {
                Succeed = true,
                JoinInfo = new JoinInfo()
                {
                    Port = 123,
                    IpAddress = "127.0.0.1",
                    ReservedSlot = ReservedSlot
                },
                MessageId = request.MessageId
            };
        }

        public IPAddress RemoteIP => IPAddress.Parse("127.0.0.1");
        public Guid InstanceId { get; }
        public int MasterListenerPort { get; }
        public float CPUUsage { get; }
        public GameInfo GameInfo =>
            new GameInfo()
            {
                GameName = "submarines",
                GameVersion = "1.05"
            };

        public GameServerStateReport[] GameServers => new[]
        {
            new GameServerStateReport()
            {
                Port = 123,
                ProcessID = 500,
                MatchmakingData = string.Empty
            }
        };
    }
}