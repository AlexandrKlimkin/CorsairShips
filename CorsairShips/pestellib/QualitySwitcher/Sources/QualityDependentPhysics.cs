using UnityDI;
using UnityEngine;

namespace PestelLib.QualitySwitcher
{
    public class QualityDependentPhysics : MonoBehaviour
    {
        [Dependency] private QualitySettingsManager _qualitySettingsManager;

        protected void Start()
        {
            ContainerHolder.Container.BuildUp(this);
            _qualitySettingsManager.OnChangeQuality += OnChangeQuality;
            OnChangeQuality(_qualitySettingsManager.CurrentQuality);
        }

        void OnDestroy()
        {
            if (_qualitySettingsManager != null)
            {
                _qualitySettingsManager.OnChangeQuality -= OnChangeQuality;
            }
        }

        private void OnChangeQuality(int quality)
        {
            if (_qualitySettingsManager.CurrentQuality == 0)
            {
                Physics.defaultSolverIterations = 3;
                Time.fixedDeltaTime = 0.033333333333f;
            }
            else
            {
                Physics.defaultSolverIterations = 6;
                Time.fixedDeltaTime = 0.016666666666f;
            }
        }
    }
}
