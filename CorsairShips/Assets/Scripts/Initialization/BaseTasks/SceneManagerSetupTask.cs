using Initialization.SceneLoading;
using UnityDI;
using UTPLib.Services.SceneManagement;
using UTPLib.Tasks.Base;

namespace Initialization.BaseTasks {
    public class SceneManagerSetupTask : AutoCompletedTask {
        protected override void AutoCompletedRun() {
            var sceneManager = ContainerHolder.Container.Resolve<SceneManagerService>();
            var map = new SceneLoadingMapConcrete();
            //sceneManager.SetupMap(map);
        }
    }
}