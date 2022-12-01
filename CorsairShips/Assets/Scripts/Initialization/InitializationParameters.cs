using System.Collections.Generic;
using Core.Initialization.Base;
using Game.Initialization.Base;
using Game.Initialization.GameTasks;
using Game.SeaGameplay;
using Game.SeaGameplay.Spawn;
using PestelLib.TaskQueueLib;
using Game.Quality;
using Game.SeaGameplay.AI;
using UI.Markers;
using UTPLib.Services.ResourceLoader;
using UTPLib.Services.SceneManagement;
using UTPLib.SignalBus;
using UTPLib.Tasks.Base;
using UTPLib.Tasks.ConcreteCommon;

namespace Game.Initialization {
    public static class InitializationParameters {
        public static List<Task> BaseTasks => new List<Task>() {
            new TaskInitMessagePack(),
            new ContainerInitializationTask(),
            new BaseServiceInitializationTask<SignalBus, SignalBus>(),
            new BaseServiceInitializationTask<IResourceLoaderService, ResourceLoaderService>(),
            
            new DataInitializationTask(),
            
            new UnityEventProviderRegisterTask(),
            new RegisterAndLoadServiceTask<SceneManagerService>(),
            
            new RegisterAndLoadServiceTask<MarkerService>(),
            new GUIInitilizationTask(),
            new RegisterAndLoadServiceTask<QualityService>(),
        };
        
        public static List<Task> LoadingGameTasks => new List<Task>() {
            new GameCameraSpawnTask(),
            new RegisterAndLoadServiceTask<AIService>(),
            new RegisterAndLoadServiceTask<ShipCreationService>(),
            new GUISetupTask(),
            
            new WaitForAwakesTask(),

            new RegisterAndLoadServiceTask<ShipsSpawnService>(),
        };

        public static List<Task> UnloadingGameTasks => new List<Task>() {
            new UnregisterAndUnloadServiceTask<AIService>(),
            new UnregisterAndUnloadServiceTask<ShipsSpawnService>(),
            new UnregisterAndUnloadServiceTask<ShipCreationService>(),
        };
    }
}
