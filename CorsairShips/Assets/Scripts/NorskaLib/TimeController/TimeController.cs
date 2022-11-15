using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

namespace NorskaLib.TimeControl
{
    public sealed partial class TimeController : MonoBehaviour
    {
        public static Action<float> onTimeScaleChanged = (value) => { };

        public static TimeController Instance { get; private set; }

        private float timeScale = 1;
        public float TimeScale
        {
            get => timeScale;

            private set
            {
                if (!Mathf.Approximately(value, timeScale))
                    onTimeScaleChanged.Invoke(value);
                timeScale = value;
            }
        }

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            bool useLerp;
            float targetScale;

            if (requests.Count > 0)
            {
                var removedCount = requests.RemoveAll(r => 
                    r.source == null 
                    || (r.releaseOnDisabled && !r.source.activeInHierarchy));
                if (removedCount > 0)
                    SetupLerp();

                var currentRequest = requests.FirstOrDefault();
                if (currentRequest is null)
                {
                    // Reverting default timescale
                    useLerp = true;
                    targetScale = defaultScale;
                }
                else
                {
                    useLerp = currentRequest.soft;
                    targetScale = currentRequest.scale;
                }
            }
            else
            {
                useLerp = true;
                targetScale = defaultScale;
            }

            if (useLerp)
            {
                if (lerpTimer < softScaleDuration)
                    lerpTimer += Time.deltaTime;

                var t = lerpTimer / softScaleDuration;
                TimeScale = Mathf.Lerp(lerpInitValue, targetScale, t);
            }
            else
            {
                TimeScale = targetScale;
            }
        }

        [SerializeField] float softScaleDuration   = 0.3f;
        private float lerpInitValue                = 1f;
        private float lerpTimer                    = 0f;

        private void SetupLerp()
        {
            lerpInitValue   = timeScale;
            lerpTimer       = 0f;
        }

        #region Default scale management

        private float defaultScale = 1;
        public void SetDefaultScale(float value)
        {
            value = Mathf.Clamp(value, 0, 2);
            defaultScale = value;

            SetupLerp();
        }

        #endregion

        #region Requests management

        private List<TimescaleRequest> requests = new(3);

        public class TimescaleRequest
        {
            public GameObject source;
            public float scale;
            public bool releaseOnDisabled;
            public bool soft;
        }

        /// <summary>
        /// If this source is already present in request queue, it will be replaced.
        /// </summary>
        /// <param name="request"></param>
        public void Request(GameObject source, float scale, bool soft = false, bool releaseOnDisabled = true)
        {
            var clone = requests.FirstOrDefault(r => r.source == source);
            if (clone is null)
            {
                requests.Add(new TimescaleRequest()
                {
                    source = source,
                    scale = scale,
                    soft = soft,
                    releaseOnDisabled = releaseOnDisabled
                });
            }
            else
            {
                clone.scale = scale;
                clone.soft = soft;
                clone.releaseOnDisabled = releaseOnDisabled;
            }

            UpdateRequestsSorting();
        }

        /// <summary>
        /// Discarding request from this source.
        /// </summary>
        /// <param name="source"></param>
        public void Unrequest(GameObject source)
        {
            var request = requests.FirstOrDefault(r => r.source == source);
            if (request is null)
                return;

            requests.Remove(request);
            UpdateRequestsSorting();
        }

        private void UpdateRequestsSorting()
        {
            requests = requests.OrderBy(r => r.scale).ToList();
        }

        #endregion
    }
}
