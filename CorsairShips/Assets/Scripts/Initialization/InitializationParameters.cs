using System.Collections.Generic;
using Core.Initialization.Base;
using Game.Dmg;
using Game.Initialization.Base;
using Game.Initialization.GameTasks;
using Game.SeaGameplay;
using Game.SeaGameplay.Spawn;
using PestelLib.TaskQueueLib;
using Game.Quality;
using Game.SeaGameplay.AI;
using Game.SeaGameplay.GameModes;
using Game.SeaGameplay.Statistics;
using UI.Markers;
using UTPLib.Services.ResourceLoader;
using UTPLib.Services.SceneManagement;
using UTPLib.SignalBus;
using UTPLib.Tasks.Base;
using UTPLib.Tasks.ConcreteCommon;

namespace Game.Initialization {
    public static class InitializationParameters {
        public static List<Task> BaseTasks => new() {
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
        
        public static List<Task> LoadingGameTasks => new() {
            new GameCameraSpawnTask(),
            new RegisterAndLoadServiceTask<BattleStatisticsService>(),
            new RegisterAndLoadServiceTask<AIService>(),
            new RegisterAndLoadServiceTask<ShipCreationService>(),
            new RegisterAndLoadServiceTask<DamageService>(),
            
            new WaitForAwakesTask(),
            
            new RegisterAndLoadServiceTask<ShipsSpawnService>(),
            new RegisterAndLoadServiceTask<DeathMatchService>(),
            new GUISetupTask(),
        };

        public static List<Task> UnloadingGameTasks => new() {
            new UnregisterAndUnloadServiceTask<DeathMatchService>(),
            new UnregisterAndUnloadServiceTask<ShipsSpawnService>(),
            new UnregisterAndUnloadServiceTask<DamageService>(),
            new UnregisterAndUnloadServiceTask<ShipCreationService>(),
            new UnregisterAndUnloadServiceTask<AIService>(),
            new UnregisterAndUnloadServiceTask<BattleStatisticsService>(),
        };
    }
}
