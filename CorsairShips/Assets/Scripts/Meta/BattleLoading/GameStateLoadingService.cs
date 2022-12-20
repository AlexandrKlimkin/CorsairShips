using System.Collections.Generic;
using Game.SeaGameplay.Data;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.Services.SceneManagement;

namespace Game.Meta.BattleLoading {
    public class GameStateLoadingService : ILoadableService, IUnloadableService {
        [Dependency]
        private readonly SceneManagerService _SceneManagerService;

        public GameMode SelectedGameMode { get; set; }
        
        public void Load() {
            SelectedGameMode = GameMode.DeathMatch;
        }

        public void Unload() {
            
        }
        
        public void LoadSelectedGameMode() {
            LoadGameMode(SelectedGameMode);
        }
        
        public void LoadGameMode(GameMode mode, bool enableReload = false) {
            var scenes = _SceneManagerService.Map.GetMatchedScenes(mode);
            var randIndex = Random.Range(0, scenes.Count);
            var scene = scenes[randIndex];
            _SceneManagerService.LoadScene(new GameModeScene { GameMode = mode, SceneType = scene});
        }

        public void LoadMenu() {
            _SceneManagerService.LoadScene(new GameModeScene { GameMode = null, SceneType = SceneType.Menu});
        }
    }
}