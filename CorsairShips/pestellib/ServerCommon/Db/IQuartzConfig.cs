
namespace PestelLib.ServerCommon.Db
{
    public class QuartzInstance
    {
        public string AppPath;
        public bool IsOn;
        public string Id;
        public string VirtualPath;
    }

    public interface IQuartzConfig
    {
        QuartzInstance[] GetAllInstances();
        bool SetQuartzState(string instanceId, bool isOn);
        bool ShouldExecuteJobs();
        void Save(QuartzInstance instance);
    }
}
