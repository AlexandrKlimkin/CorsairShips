using System.Threading.Tasks;
using PestelLib.ServerCommon.Redis;
using UnityDI;

namespace BackendCommon.Code.GlobalConflict.Server.Stages
{
    public abstract class Stage
    {
        public const string StageLock = "GlobalConflictStageLock";
        [Dependency]
        protected readonly GlobalConflictPrivateApi _api;
        [Dependency]
        protected readonly ILockManager _lockManager;

        protected Stage()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        public abstract Task<bool> HasWork();
        public abstract Task<bool> Update();
    }
}