using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NorskaLib.TimeControl
{
#if UNITY_EDITOR
    public sealed partial class TimeController
    {
        [Header("Debugging")]

        [Range(0, 2)]
        [DisableIf("@true"), ShowInInspector]
        private float currentScaleView;
        [Range(0, 2)]
        [DisableIf("@true"), ShowInInspector] 
        private float defaultScaleView;

        [DisableIf("@true"), ShowInInspector] 
        private float lerpTimerView;

        [DisableIf("@true"), ShowInInspector] 
        private TimescaleRequestView[] requestsView = new TimescaleRequestView[0];

        [System.Serializable]
        public struct TimescaleRequestView
        {
            public GameObject source;
            public float value;
            public bool releaseOnDisabled;
            public bool soft;

            public TimescaleRequestView(TimescaleRequest request)
            {
                this.source = request.source;
                this.value = request.scale;
                this.releaseOnDisabled = request.releaseOnDisabled;
                this.soft = request.soft;
            }
        }

        void LateUpdate()
        {
            currentScaleView = timeScale;
            defaultScaleView = defaultScale;
            lerpTimerView = lerpTimer;

            requestsView = requests.Select(r => new TimescaleRequestView(r)).ToArray();
        }
    }
#endif
}