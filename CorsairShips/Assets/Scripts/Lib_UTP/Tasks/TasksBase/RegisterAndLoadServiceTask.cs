using UnityDI;
using UTPLib.Services;
using UTPLib.Tasks;

namespace UTPLib.Tasks.Base {
    public class RegisterAndLoadServiceTask<T> : AutoCompletedTask where T : class, new() {
        protected override void AutoCompletedRun() {
            var container = ContainerHolder.Container;
            var serviceInstance = new T();
            container.RegisterInstance(serviceInstance);
            container.BuildUp(serviceInstance.GetType(), serviceInstance);
            (serviceInstance as ILoadableService)?.Load();
        }
    }
}