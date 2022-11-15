using System;
using System.Collections;
using System.Collections.Generic;
using UnityDI;
using UnityEngine;

namespace PestelLib.QualitySwitcher
{
    public class MWQualitySettingsManager : MonoBehaviour, IQualitySettingsManager
    {
        public event Action<int> OnChangeQuality = level => { };

        public int CurrentQuality
        {
            get
            {
                if (IsQualityOverrided)
                    return SavedQualityLevel;
                else
                    return DefaultQuality;
            }
        }

        public void SetQualityByUserChoice(int qualityLevel)
        {
            IsQualityOverrided = true;
            SavedQualityLevel = qualityLevel;
            
            UpdateQuality();
        }

        [SerializeField]
        private List<QualityLevel> _qualityLevels;

        [Space]
        [SerializeField]
        private bool _canChangeFps = false;
        [SerializeField]
        private bool _canChangeShaderLod = true;
        [SerializeField]
        private bool _canChangeScreenResolution = true;

        private const string SavedQualityLevelKey = "SavedQualityLevel";
        private const string IsQualityOverridedKey = "IsQualityOverrided";

        static private float _defaultScreenWidth;
        static private float _defaultScreenHeight;
        static private float _defaultDpi;
        static private bool _isDefaultWrote;

        private void Awake()
        {
            if (_isDefaultWrote == false)//should guarantee it will be call once
            {
                _defaultScreenHeight = Screen.height;
                _defaultScreenWidth = Screen.width;
                _defaultDpi = Screen.dpi;
                _isDefaultWrote = true;
            }
        }

        protected void Start()
        {
            if (!IsQualityOverrided)
            {
                SavedQualityLevel = CurrentQuality;
                IsQualityOverrided = true;
            }

            UpdateQuality();
        }

        private void UpdateQuality()
        {
            try
            {
                QualityLevelSettings qualitySettigns = _qualityLevels[Mathf.Clamp(CurrentQuality, 0, _qualityLevels.Count)].settings;

                if (_canChangeShaderLod && Shader.globalMaximumLOD != qualitySettigns.shaderLod)
                    Shader.globalMaximumLOD = qualitySettigns.shaderLod;

                if (_canChangeFps && Application.targetFrameRate != qualitySettigns.targetFPS)
                    Application.targetFrameRate = qualitySettigns.targetFPS;

                if (_canChangeScreenResolution)
                    ApplyScreenResolution(qualitySettigns.screenSizeRatio);
                
                QualitySettings.SetQualityLevel(CurrentQuality, true);

                OnChangeQuality(CurrentQuality);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n " + e.StackTrace);
            }
        }

        private bool CheckUseMinQualityLevel() {
            if (Application.platform == RuntimePlatform.Android) {
                var osStr = SystemInfo.operatingSystem;
                var apiLevelStr = osStr.Substring(osStr.IndexOf("-", StringComparison.Ordinal) + 1, 3);
                var apiLevel = -1;
                var apiLevelRetrieved = int.TryParse(apiLevelStr, out apiLevel);
                return apiLevelRetrieved && apiLevel < 26;
            }
            return false;
        }
        
        private int DefaultQuality
        {
            get {
                if (CheckUseMinQualityLevel()) return 0;
                
                int processorFrequncySum = SystemInfo.processorCount * SystemInfo.processorFrequency;
                int systemMemory = SystemInfo.systemMemorySize;
                int graphicsMemorySize = SystemInfo.graphicsMemorySize;
                int videoTier = (int)Graphics.activeTier;

                //need check with >0, because parammeteres can return -1

                for (int i = _qualityLevels.Count - 1; i > 0; i--)
                {
                    if (processorFrequncySum > 0 && _qualityLevels[i].requirements.processorFrequncySum > processorFrequncySum)
                        continue;

                    if (systemMemory > 0 && _qualityLevels[i].requirements.systemMemory > systemMemory)
                        continue;

                    if (graphicsMemorySize > 0 && _qualityLevels[i].requirements.graphicsMemorySize > graphicsMemorySize)
                        continue;

                    if (_qualityLevels[i].requirements.videoTier > videoTier)
                        continue;

                    return i;
                }

                return 0;
            }
        }

        private static bool IsQualityOverrided
        {
            get { return PlayerPrefs.GetInt(IsQualityOverridedKey, 0) == 1; }

            set
            {
                PlayerPrefs.SetInt(IsQualityOverridedKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private static int SavedQualityLevel
        {
            get { return PlayerPrefs.GetInt(SavedQualityLevelKey); }
            set { PlayerPrefs.SetInt(SavedQualityLevelKey, value); }
        }

        const int minimalDpi = 240;
        private void ApplyScreenResolution(float dpiRatio)
        {
            float dpi = Mathf.Clamp(_defaultDpi * dpiRatio, minimalDpi, _defaultDpi);
            float coef = Mathf.Clamp01(dpi / _defaultDpi);

            int newWidth = (int)(_defaultScreenWidth * coef);
            int newHeight = (int)(_defaultScreenHeight * coef);

            if (Screen.width != newWidth || Screen.height != newHeight)
                Screen.SetResolution(newWidth, newHeight, true, 60);
        }

        [Serializable]
        private class QualityLevel
        {
            public QualityLevelSettings settings;
            public QualityLevelRequirement requirements;
        }

        [Serializable]
        private class QualityLevelSettings
        {
            public int targetFPS = 30;
            public int shaderLod = 1000;
            public float screenSizeRatio = 1;
        }

        [Serializable]
        private class QualityLevelRequirement
        {
            public int graphicsMemorySize;
            public int videoTier;
            public int processorFrequncySum;
            public int systemMemory;
        }
    }
}