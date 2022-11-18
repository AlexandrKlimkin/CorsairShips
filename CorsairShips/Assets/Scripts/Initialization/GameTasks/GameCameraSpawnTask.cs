using UnityDI;
using UTPLib.Services.ResourceLoader;
using UTPLib.Tasks.Base;

namespace Game.Initialization.GameTasks {
    public class GameCameraSpawnTask : AutoCompletedTask {
        protected override void AutoCompletedRun() {
            var resourceLoader = ContainerHolder.Container.Resolve<IResourceLoaderService>();
            resourceLoader.LoadResourceOnScene<SimpleFollowCamera>(ResourcePath.Camera.GameCameraPath);
        }
    }
}