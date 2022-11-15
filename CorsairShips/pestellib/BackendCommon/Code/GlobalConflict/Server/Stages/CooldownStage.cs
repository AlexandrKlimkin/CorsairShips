using System.Threading.Tasks;

namespace BackendCommon.Code.GlobalConflict.Server.Stages
{
    public class CooldownStage : Stage
    {
        public override Task<bool> HasWork()
        {
            return Task.FromResult(false);
        }

        public override Task<bool> Update()
        {
            return Task.FromResult(false);
        }
    }
}