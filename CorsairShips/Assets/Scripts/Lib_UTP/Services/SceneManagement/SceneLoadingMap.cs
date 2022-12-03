using System.Collections.Generic;
using UTPLib.Services.SceneManagement;

namespace UTPLib.Services.SceneManagement {
    public abstract class SceneLoadingMap {
        public abstract bool IsGameScene { get; }
        public abstract Dictionary<SceneType, SceneLoadingParameters> LoadingMap { get; }
    }
}