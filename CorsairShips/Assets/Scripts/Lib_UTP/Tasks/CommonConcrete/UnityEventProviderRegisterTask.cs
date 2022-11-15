using PestelLib.Utils;
using UnityDI;
using UnityEngine;
using UTPLib.Tasks.Base;

namespace UTPLib.Tasks.ConcreteCommon {
    public class UnityEventProviderRegisterTask : AutoCompletedTask {
        protected override void AutoCompletedRun() {
            var obj = new GameObject("EventProvider");
            var eventProvider = obj.AddComponent<UnityEventsProvider>();
            ContainerHolder.Container.RegisterInstance(eventProvider);
            Object.DontDestroyOnLoad(eventProvider.gameObject);
        }
    }
}
