using UnityDI;
using UTPLib.Tasks;

namespace UTPLib.Tasks.Base {
    public class BaseServiceInitializationTask<TBase, TDerived> : AutoCompletedTask where TDerived : class, TBase, new() {
        protected override void AutoCompletedRun() {
            ContainerHolder.Container.RegisterSingleton<TBase, TDerived>();
        }
    }
}