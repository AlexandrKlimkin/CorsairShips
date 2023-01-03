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
    
    public class RegisterAndLoadServiceTask<TBase, TDerived> : AutoCompletedTask where TDerived : class, TBase, new() {
        protected override void AutoCompletedRun() {
            var container = ContainerHolder.Container;
            var serviceInstance = new TDerived();
            ContainerHolder.Container.RegisterInstance<TBase, TDerived>(serviceInstance);
            container.BuildUp(serviceInstance);
            (serviceInstance as ILoadableService)?.Load();
        }
    }
}