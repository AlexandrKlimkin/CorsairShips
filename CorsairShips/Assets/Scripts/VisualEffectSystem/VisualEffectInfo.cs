using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.VisualEffects {
    [Serializable]
    public struct VisualEffectInfo {
        public string EffectName;
        public bool RandomScale;
        [ShowIf("@!RandomScale")]
        public Vector3 Scale;
        [ShowIf("@RandomScale")]
        public float ScaleMin;
        [ShowIf("@RandomScale")]
        public float ScaleMax;
    }
}