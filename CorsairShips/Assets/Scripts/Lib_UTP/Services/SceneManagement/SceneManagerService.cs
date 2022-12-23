using System;
using System.Collections.Generic;
using Game.SeaGameplay.Data;
using Initialization.SceneLoading;
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
        
        public GameMode? ActiveGameMode { get; private set; }

        public GameModeScene ActiveGameModeScene => new() {
            GameMode = ActiveGameMode,
            SceneType = ActiveScene,
        };

        public bool IsGameScene => Map.GameScenes.Contains(ActiveScene);

        public void Load() {
            SetupMap(new SceneLoadingMapConcrete());
            if (IsGameScene) {
                ActiveGameMode = GameMode.DeathMatch;
            }
        }
        public SceneLoadingMap Map { get; private set; }
        
        private void SetupMap(SceneLoadingMap map) {
            Map = map;
        }
        
        public void LoadScene(GameModeScene gameModeScene, bool enableReboot = false) {
            var activeScene = ActiveScene;
            var scene = gameModeScene.SceneType;
            if (activeScene == scene && !enableReboot)
                return;
            var oldSceneParameters = Map.LoadingMap[new GameModeScene{GameMode = ActiveGameMode, SceneType = ActiveScene}];
            var newParameters = Map.LoadingMap[gameModeScene];
            oldSceneParameters.BeforeUnload();
            oldSceneParameters.UnloadingTasks.RunTasksListAsQueue(
                () => {
                    OnSceneUnloadSuccess(gameModeScene);
                    oldSceneParameters.AfterUnload();
                    newParameters.BeforeLoad();
                    SceneManager.LoadScene(scene.ToString());
                    newParameters.LoadingTasks.RunTasksListAsQueue(
                        () => {
                            OnSceneLoadSuccess(gameModeScene);
                            newParameters.AfterLoad();
                        },
                        (task, e) => {
                            OnSceneLoadFail(gameModeScene);
                            Debug.LogError($"{task} task failed with {e}");
                        },
                        null);
                },
                (task, e) => {
                    OnSceneUnloadFail(gameModeScene);
                    Debug.LogError($"{task} task failed with {e}");
                },
                null);
        }

        private void OnSceneUnloadSuccess(GameModeScene scene) {
            Debug.Log($"{scene} successfully unloaded");
        }

        private void OnSceneUnloadFail(GameModeScene scene) {
            Debug.LogError($"{scene} unload fail");
        }

        private void OnSceneLoadSuccess(GameModeScene scene) {
            ActiveGameMode = scene.GameMode;
            Debug.Log($"{scene} successfully loaded");
        }

        private void OnSceneLoadFail(GameModeScene scene) {
            Debug.LogError($"{scene} load fail");
        }
    }
}
