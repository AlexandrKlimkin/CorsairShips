using System;
using UnityDI;
using UnityEngine;

namespace PestelLib.QualitySwitcher
{
    public class QualityDependent : MonoBehaviour {
        public GameObject[] dependentGameObjects;
        public Behaviour[] dependentComponent;
        public bool invert;

        [HideInInspector] public int minQualityToEnable;
        [HideInInspector] public QualityPlatform ExcludedPlatforms;

        protected IQualitySettingsManager qualitySettingsManager;

        protected void Start()
        {
            qualitySettingsManager = ContainerHolder.Container.Resolve<IQualitySettingsManager>();
            if (qualitySettingsManager == null)
            {
                qualitySettingsManager = ContainerHolder.Container.Resolve<QualitySettingsManager>();
            }

            qualitySettingsManager.OnChangeQuality += OnChangeQuality;
            UpdateState();
        }

        void OnDestroy() {
            if (qualitySettingsManager != null) {
                qualitySettingsManager.OnChangeQuality -= OnChangeQuality;
            }
        }

        void OnChangeQuality(int qualityLevel) {
            UpdateState();
        }

        protected virtual void UpdateState() {
            bool nowEnabled = (qualitySettingsManager.CurrentQuality >= minQualityToEnable);
            
            //Debug.Log("Name: " + name + ". Current quality " + qualitySettingsManager.CurrentQuality);

            if (invert)
                nowEnabled = !nowEnabled;

            if (IsExcludedPlatform())
                nowEnabled = false;

            for (int i = 0; i < dependentComponent.Length; i++) {
                if (dependentComponent[i] == null)
                {
                    Debug.LogError("QualityDependent " + gameObject.name + " has an empty element in components");
                    continue;
                }
                dependentComponent[i].enabled = nowEnabled;
            }

            for (int i = 0; i < dependentGameObjects.Length; i++) {
                if (dependentGameObjects[i] == null)
                {
                    Debug.LogError("QualityDependent " + gameObject.name + " has an empty element in gameobjects");
                    continue;
                }
                dependentGameObjects[i].SetActive(nowEnabled);
            }
        }

        private bool IsExcludedPlatform()
        {
            QualityPlatform platformToCheck = 0;
            RuntimePlatform runtimePlatform = Application.platform;

            if (runtimePlatform == RuntimePlatform.Android)
                platformToCheck = QualityPlatform.Android;

            if (runtimePlatform == RuntimePlatform.IPhonePlayer)
                platformToCheck = QualityPlatform.iOS;

            if (runtimePlatform == RuntimePlatform.WindowsEditor || runtimePlatform == RuntimePlatform.OSXEditor)
                platformToCheck = QualityPlatform.Editor;
            
            if (runtimePlatform == RuntimePlatform.WindowsPlayer ||
                runtimePlatform == RuntimePlatform.OSXPlayer ||
                runtimePlatform == RuntimePlatform.LinuxPlayer)
                platformToCheck = QualityPlatform.Standalone;
            
            return (ExcludedPlatforms & platformToCheck) != 0;
        }
    }
}