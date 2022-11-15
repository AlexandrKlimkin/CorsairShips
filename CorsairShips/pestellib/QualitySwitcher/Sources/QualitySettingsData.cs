using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PestelLib.QualitySwitcher
{
    [CreateAssetMenu(fileName = "QualitySettingsData", menuName = "Settings/QualitySettingsData")]
    public class QualitySettingsData : ScriptableObject
    {
        public List<QualityLevel> qualityLevels { get { return _qualityLevels; } }

        public bool canChangeFps { get { return _canChangeFps; } }
        public bool canChangeShaderLod { get { return _canChangeShaderLod; } }
        public bool canChangeScreenResolution { get { return _canChangeScreenResolution; } }

        [SerializeField]
        private List<QualityLevel> _qualityLevels;

        [Space]
        [SerializeField]
        private bool _canChangeFps = false;
        [SerializeField]
        private bool _canChangeShaderLod = true;
        [SerializeField]
        private bool _canChangeScreenResolution = true;
        
        [Serializable]
        public class QualityLevel
        {
            public QualityLevelSettings settings;
            public QualityLevelRequirement requirements;
        }

        [Serializable]
        public class QualityLevelSettings
        {
            public int targetFPS = 30;
            public int shaderLod = 1000;
            public float screenSizeRatio = 1;
        }

        [Serializable]
        public class QualityLevelRequirement
        {
            public int graphicsMemorySize;
            public int videoTier;
            public int processorFrequncySum;
            public int systemMemory;
        }
    }
}