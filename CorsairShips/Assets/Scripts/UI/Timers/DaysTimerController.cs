using System;
using PestelLib.Localization;
using UI.TimeFormatting;
using UnityDI;
using UnityEngine;

namespace UI.Timers {
    public class DaysTimerController : TimerController {
        [SerializeField]
        private string Separator = ":";
        [SerializeField]
        private string DaysLocalizationKey;

        private TimeSpan _AddMinute = new TimeSpan(0,0,0,58);

        protected override string CurrentFormattedTime {
            get {
                var timeSpan = TimeSpan.FromSeconds(ElapsedFlooredTime);
                return timeSpan.Days > 0
                    ? timeSpan.FormatTime(Localization, 1, separator: Separator)
                    : (timeSpan + _AddMinute).FormatTime(Localization,2, TimeDisplayStart.Hour, TimeDisplayStart.Hour, Separator);
            }
        }
    }
}
