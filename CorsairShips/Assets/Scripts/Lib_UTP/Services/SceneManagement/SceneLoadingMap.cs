using System.Collections.Generic;
using Game.SeaGameplay.Data;
using UTPLib.Services.SceneManagement;

namespace UTPLib.Services.SceneManagement {
    public abstract class SceneLoadingMap {
        public abstract List<SceneType> GameScenes { get; }
        public abstract Dictionary<GameModeScene, SceneLoadingParameters> LoadingMap { get; }
        public abstract List<SceneType> GetMatchedScenes(GameMode mode);
    }
}