using PestelLib.ClientConfig;
using PestelLib.TaskQueueLib;
using UnityDI;

namespace PestelLib.UniversalSerializer
{
    public class TaskSetupSerialization : Task
    {
        [Dependency] private Config _config;

        public override void Run()
        {
            ContainerHolder.Container.BuildUp(this);
            
            if (_config.UseJsonSerialization)
            {
                Serializer.SetTextMode();
            }

            OnComplete(this);
        }
    }
}