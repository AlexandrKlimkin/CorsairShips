using System.Collections.Generic;
using Game.Initialization.Parameters;
using UTPLib.Services.SceneManagement;

namespace Initialization.SceneLoading {
    public class SceneLoadingMapConcrete : SceneLoadingMap {
        public override bool IsGameScene { get; }
        public override Dictionary<SceneType, SceneLoadingParameters> LoadingMap { get; } = new () {
            {
                SceneType.BattleArena, new DeathMatchLoadingParameters()
            },
        };
    }
}