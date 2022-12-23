using UnityDI;
using UnityEngine;

namespace UTPLib.Tasks.Base {
    public class RegisterSceneObjectTask<T> : AutoCompletedTask where T : Component {
        protected override void AutoCompletedRun() {
            var obj = Object.FindObjectOfType<T>();
            if(obj == null)
                return;
            ContainerHolder.Container.RegisterInstance(obj);
        }
    }
}