using System;
using UTPLib.Tasks;
using PestelLib.Utils;
using UnityDI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UTPLib.Services.SceneManagement {
    public class SceneManagerService : ILoadableService {

        [Dependency] private readonly UnityEventsProvider _EventProvider;

        public SceneType ActiveScene {
            get {
                Enum.TryParse(SceneManager.GetActiveScene().name, false, out SceneType result);
                return result;
            }
        }

        public bool IsGameScene => _Map.IsGameScene;

        public void Load() { }

        private SceneLoadingMap _Map;
        public void SetupMap(SceneLoadingMap map) {
            _Map = map;
        }
        
        public void LoadScene(SceneType scene, bool enableReboot = false) {
            var activeScene = ActiveScene;
            if (activeScene == scene && !enableReboot)
                return;
            var oldSceneParameters = _Map.LoadingMap[activeScene];
            var newParameters = _Map.LoadingMap[scene];
            oldSceneParameters.BeforeUnload();
            oldSceneParameters.UnloadingTasks.RunTasksListAsQueue(
                () => {
                    OnSceneUnloadSuccess(activeScene);
                    oldSceneParameters.AfterUnload();
                    newParameters.BeforeLoad();
                    SceneManager.LoadScene(scene.ToString());
                    newParameters.LoadingTasks.RunTasksListAsQueue(
                        () => {
                            OnSceneLoadSuccess(scene);
                            newParameters.AfterLoad();
                        },
                        (task, e) => {
                            OnSceneLoadFail(scene);
                            Debug.LogError($"{task} task failed with {e}");
                        },
                        null);
                },
                (task, e) => {
                    OnSceneUnloadFail(scene);
                    Debug.LogError($"{task} task failed with {e}");
                },
                null);
        }

        private void OnSceneUnloadSuccess(SceneType scene) {
            Debug.Log($"{scene} successfully unloaded");
        }

        private void OnSceneUnloadFail(SceneType scene) {
            Debug.LogError($"{scene} unload fail");
        }

        private void OnSceneLoadSuccess(SceneType scene) {
            Debug.Log($"{scene} successfully loaded");
        }

        private void OnSceneLoadFail(SceneType scene) {
            Debug.LogError($"{scene} load fail");
        }
    }
}
