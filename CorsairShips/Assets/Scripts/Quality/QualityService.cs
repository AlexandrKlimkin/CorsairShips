using Tayx.Graphy;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.Services.ResourceLoader;

namespace Game.Quality {
    public class QualityService : ILoadableService, IUnloadableService {
        [Dependency]
        private readonly IResourceLoaderService _ResourceLoader;

        private bool _SpawnFPSMonitor = false;
        
        public void Load() {
            Application.targetFrameRate = 1000;
#if UNITY_ANDROID
            if (Debug.isDebugBuild && _SpawnFPSMonitor) {
                _ResourceLoader.LoadResourceOnScene<GraphyManager>("Prefabs/Debug/Graphy_Variant");
            }
#endif
        }

        public void Unload() {
            
        }
    }
}