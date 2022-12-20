using System.Collections.Generic;
using Core.Initialization.Base;
using Game.Dmg;
using Game.Initialization.Base;
using Game.Initialization.GameTasks;
using Game.Meta.BattleLoading;
using Game.SeaGameplay;
using Game.SeaGameplay.Spawn;
using PestelLib.TaskQueueLib;
using Game.Quality;
using Game.SeaGameplay.AI;
using Game.SeaGameplay.Bounds;
using Game.SeaGameplay.GameModes;
using Game.SeaGameplay.Statistics;
using Initialization.BaseTasks;
using Stats;
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
            new RegisterAndLoadServiceTask<GameStateLoadingService>(),
            //new SceneManagerSetupTask(),
            new RegisterAndLoadServiceTask<MarkerService>(),
            new GUIInitilizationTask(),
            new RegisterAndLoadServiceTask<QualityService>(),
            new GuiSystemsLoadTask(),
        };
        
        public static List<Task> Loading_BaseGame_Tasks => new() {
            new RegisterAndLoadServiceTask<BattleStatisticsService>(),
            new RegisterAndLoadServiceTask<AIService>(),
            new RegisterAndLoadServiceTask<StatsService>(),
            new RegisterAndLoadServiceTask<ShipCreationService>(),
            new RegisterAndLoadServiceTask<DamageService>(),
            
            new WaitForAwakesTask(),
            
            new GameCameraSpawnTask(),
            new RegisterAndLoadServiceTask<LevelBoundsService>(),
            new RegisterAndLoadServiceTask<ShipsSpawnService>(),
        };

        public static List<Task> Unloading_BaseGame_Tasks => new() {
            new CloseGUITask(),
            new UnregisterAndUnloadServiceTask<ShipsSpawnService>(),
            new UnregisterAndUnloadServiceTask<LevelBoundsService>(),
            new UnregisterAndUnloadServiceTask<DamageService>(),
            new UnregisterAndUnloadServiceTask<ShipCreationService>(),
            new UnregisterAndUnloadServiceTask<StatsService>(),
            new UnregisterAndUnloadServiceTask<AIService>(),
            new UnregisterAndUnloadServiceTask<BattleStatisticsService>(),
        };

        public static List<Task> Loading_DeathMatch_Tasks => new() {
            new RegisterAndLoadServiceTask<DeathMatchService>(),
        };

        public static List<Task> Unloading_DeathMatch_Tasks => new() {
            new UnregisterAndUnloadServiceTask<DeathMatchService>(),
        };

        public static List<Task> Loading_Menu_Tasks => new() {
            new MenuGuiLoadTask(),
        };
        
        public static List<Task> Unloading_Menu_Tasks => new() {
            new CloseGUITask(),
        };
    }
}
