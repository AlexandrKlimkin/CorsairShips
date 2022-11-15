using UnityDI;
using UTPLib.Services;

namespace UTPLib.Tasks.Base {
    public class UnregisterAndUnloadServiceTask<T> : AutoCompletedTask {
        protected override void AutoCompletedRun() {
            var container = ContainerHolder.Container;
            var service = container.Resolve<T>();
            container.Unregister<T>();
            (service as IUnloadableService)?.Unload();
        }
    }
}