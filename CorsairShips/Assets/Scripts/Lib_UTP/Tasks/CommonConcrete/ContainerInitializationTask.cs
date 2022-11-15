using UnityDI;
using UTPLib.Tasks;
using UTPLib.Tasks.Base;

namespace UTPLib.Tasks.ConcreteCommon {
    public class ContainerInitializationTask : AutoCompletedTask {
        protected override void AutoCompletedRun() {
            ContainerHolder.Container.RegisterInstance(ContainerHolder.Container);
        }
    }
}
