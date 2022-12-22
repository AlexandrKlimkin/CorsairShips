using UnityDI;

namespace UTPLib.Tasks.Base {
    public class UnregisterObjectTask<T> : AutoCompletedTask {
        protected override void AutoCompletedRun() {
            ContainerHolder.Container.Unregister<T>();
        }
    }
}