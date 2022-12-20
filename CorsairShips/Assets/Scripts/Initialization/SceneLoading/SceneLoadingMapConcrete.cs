using System.Collections.Generic;
using System.Linq;
using Game.Initialization.Parameters;
using Game.SeaGameplay.Data;
using UTPLib.Services.SceneManagement;

namespace Initialization.SceneLoading {
    public class SceneLoadingMapConcrete : SceneLoadingMap {

        public override List<SceneType> GameScenes => new() {
            SceneType.BattleArena,
        };

        public override Dictionary<GameModeScene, SceneLoadingParameters> LoadingMap { get; } = new() {
            {
                new GameModeScene {GameMode = GameMode.DeathMatch, SceneType = SceneType.BattleArena}, new DeathMatchLoadingParameters()
            },
            {
                new GameModeScene {GameMode = GameMode.TeamDeathMatch, SceneType = SceneType.BattleArena}, new DeathMatchLoadingParameters()
            },
            {
                new GameModeScene {GameMode = null, SceneType = SceneType.Menu}, new MenuLoadingParameters()
            },
        };

        public override List<SceneType> GetMatchedScenes(GameMode mode) {
            return LoadingMap.Where(_ => _.Key.GameMode == mode).Select(_ => _.Key.SceneType).ToList();
        }
    }
}