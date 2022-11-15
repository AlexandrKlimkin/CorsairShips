using UnityEditor;
using UnityEngine;

namespace PestelLib.QualitySwitcher
{
    [CustomEditor(typeof(QualityDependent), true)]
    public class QualityDependentInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var owner = ((QualityDependent) target);

            var newQualityToEnable = EditorGUILayout.Popup("Min Quality To Enable", owner.minQualityToEnable, QualitySettings.names);
            if (newQualityToEnable != owner.minQualityToEnable)
            {
                owner.minQualityToEnable = newQualityToEnable;
                EditorUtility.SetDirty(owner);
            }

            var newExcludedPlatforms = (QualityPlatform)EditorGUILayout.EnumMaskField("Excluded Platforms", owner.ExcludedPlatforms);
            if (newExcludedPlatforms != owner.ExcludedPlatforms)
            {
                owner.ExcludedPlatforms = newExcludedPlatforms;
                EditorUtility.SetDirty(owner);
            }
            DrawDefaultInspector();
        }
    }
}