using UTPLib.Services;
using UnityEngine;
using UnityEngine.Advertisements;

    public class AdsInitializationService : ILoadableService, IUnloadableService, IUnityAdsInitializationListener {
        private string _AndroidGameId = "5108027";
        private string _IOSGameId = "5108026";
        private bool _TestMode = true;
        private string _GameId;
        
        public void Load() {
            InitializeAds();
        }

        public void Unload() {
            
        }

        public void InitializeAds()
        {
            _GameId = (Application.platform == RuntimePlatform.IPhonePlayer)
                ? _IOSGameId
                : _AndroidGameId;
            Advertisement.Initialize(_GameId, _TestMode, this);
        }
 
        public void OnInitializationComplete()
        {
            Debug.Log("Unity Ads initialization complete.");
        }
 
        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
        }
    }
    