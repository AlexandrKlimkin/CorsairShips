using System.Collections;
using UnityEngine;

namespace PestelLib.QualitySwitcher
{
    public class QualityDependentFog : QualityDependent
    {
        protected override void UpdateState()
        {
            RenderSettings.fog = (qualitySettingsManager.CurrentQuality >= minQualityToEnable);
            StartCoroutine(ForceApplyFogSettings(RenderSettings.fog));
        }

        //unity applies fog settings AFTER QualityDependentFog.Start
        //so we have to wait one frame...
        private IEnumerator ForceApplyFogSettings(bool fogEnabled)
        {
            yield return null;
            RenderSettings.fog = fogEnabled;
        }
    }
}