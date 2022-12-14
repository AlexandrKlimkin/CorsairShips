using System;
using System.Collections;
using PestelLib.Localization;
using RCUtils.Sources.Extensions;
using TMPro;
using UnityDI;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Timers {
    public class TimerController : MonoBehaviour {
        [FormerlySerializedAs("Text")]
        [SerializeField]
        private TextMeshProUGUI _Text;
        [SerializeField]
        private string _StringWrapperLocalizationKey;
        private Coroutine _TimerCoroutine;
        private readonly string _Zero = 0.ToString();
        protected int ElapsedFlooredTime;
        protected virtual string CurrentFormattedTime => ElapsedFlooredTime.ToString();
        public string FormatOverride { get; set; }

        protected ILocalization Localization => _Localization ?? (_Localization = ContainerHolder.Container.Resolve<ILocalization>());
        protected ILocalization _Localization;

        public void DisplayStaticValue(float seconds) {
            this.StopCoroutineSafe(ref _TimerCoroutine);
            ElapsedFlooredTime = Mathf.CeilToInt(seconds);
            var timeText = string.IsNullOrEmpty(FormatOverride) 
                ? CurrentFormattedTime 
                : TimeSpan.FromSeconds(ElapsedFlooredTime).ToString(FormatOverride);

            _Text.text = string.IsNullOrEmpty(_StringWrapperLocalizationKey) ? timeText : WrapString(timeText);
        }

        public void SetWrapper(string wrapper) {
            _StringWrapperLocalizationKey = wrapper;
        }

        public void StartDescendingTimer(float seconds, Action callback) {
            this.StartCoroutineSafe(TimeDescendingEnumerator(seconds, callback), ref _TimerCoroutine);
        }

        public void StartAscendingTimer() {
            this.StartCoroutineSafe(TimerAscendingEnumerator(), ref _TimerCoroutine);
        }
        
        public void StopTimer() {
            this.StopCoroutineSafe(ref _TimerCoroutine);
        }

        private IEnumerator TimeDescendingEnumerator(float seconds, Action callback) {
            var startTime = System.Environment.TickCount;
            do {
                ElapsedFlooredTime = Mathf.CeilToInt(seconds - TimestampExtensions.TimeStampsAbsIncrement(startTime, System.Environment.TickCount) / 1000f);
                var timeText = string.IsNullOrEmpty(FormatOverride) 
                    ? CurrentFormattedTime 
                    : TimeSpan.FromSeconds(ElapsedFlooredTime).ToString(FormatOverride);
                _Text.text = string.IsNullOrEmpty(_StringWrapperLocalizationKey) ? timeText : WrapString(timeText);
                yield return null;
            }
            while (TimestampExtensions.TimeStampsAbsIncrement(startTime, System.Environment.TickCount) < seconds * 1000);
            callback?.Invoke();
        }

        private IEnumerator TimerAscendingEnumerator() {
            var pastTime = 0f;
            _Text.text = string.IsNullOrEmpty(_StringWrapperLocalizationKey) ? _Zero : WrapString(_Zero);
            while (true) {
                yield return null;
                pastTime += Time.deltaTime;
                var timeText = ((int)Mathf.Floor(pastTime)).ToString();
                _Text.text = string.IsNullOrEmpty(_StringWrapperLocalizationKey) ? timeText : WrapString(timeText);
            }
        }

        private string WrapString(string wrapped) {
            return string.Format(Localization.Get(_StringWrapperLocalizationKey), wrapped);
        }
    }
}