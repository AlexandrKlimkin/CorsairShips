using System;
using PestelLib.ServerCommon.Db;
using UnityDI;

namespace BackendCommon.Code.Jobs
{
    public class QuartzConfig
    {
        public static QuartzInstance[] GetAllInstances()
        {
            return Instance.GetAllInstances();
        }

        public static void SetQuartzState(string instanceId, bool isOn)
        {
            Instance.SetQuartzState(instanceId, isOn);
        }

        public static bool ShouldExecuteJobs()
        {
            return Instance.ShouldExecuteJobs();
        }

        private static IQuartzConfig Instance => _db.Value;
        private static Lazy<IQuartzConfig> _db = new Lazy<IQuartzConfig>(() => ContainerHolder.Container.Resolve<IQuartzConfig>());
    }
}