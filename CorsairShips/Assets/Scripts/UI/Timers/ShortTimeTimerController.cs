using System;
using PestelLib.Localization;
using UI.TimeFormatting;
using UnityDI;
using UnityEngine;

namespace UI.Timers {
    public class ShortTimeTimerController : TimerController {
        [SerializeField]
        private bool ShowLetters;
        [SerializeField]
        private string Separator = ":";

        private ILocalization _Localization;
        private ILocalization Localization => _Localization ??= ContainerHolder.Container.Resolve<ILocalization>();
        
        protected override string CurrentFormattedTime {
            get {
                var timeSpan = TimeSpan.FromSeconds(ElapsedFlooredTime);
                return ShowLetters
                    ? timeSpan.FormatTime(Localization, 2, TimeDisplayStart.Hour, TimeDisplayStart.Minute, Separator)
                    : timeSpan.FormatTime(2, TimeDisplayStart.Hour, TimeDisplayStart.Minute, separator: Separator);

                //return timeSpan.TotalHours >= 1
                //    ? Show24Hours(timeSpan) // timeSpan.Hours > 0 
                //    //? timeSpan.ToString(HoursMinutesFormat) 
                //    : timeSpan.ToString(MinutesSecondsFormat);
            }
        }

        //private string Show24Hours(TimeSpan timeSpan) {
        //    return ShowLetters 
        //        ? $"{(int)timeSpan.TotalHours}{Localization.Get("Menu/Chests/HoursShort")}{Separator}{EnsureLeadZero(timeSpan.Minutes)}{Localization.Get("Menu/Chests/MinutesShort")}" 
        //        : $"{(int)timeSpan.TotalHours}{Separator}{EnsureLeadZero(timeSpan.Minutes)}";
        //}
        //private static string EnsureLeadZero(int value) {
        //    return $"{(value>9?"":"0")}{value}";
        //}
        //private string HoursMinutesFormat => ShowLetters ? $@"hh'{Localization.Get("Menu/Chests/HoursShort")}'\{Separator}mm'{Localization.Get("Menu/Chests/MinutesShort")}'" : $@"hh\{Separator}mm";
        //private string MinutesSecondsFormat => ShowLetters ? $@"mm'{Localization.Get("Menu/Chests/MinutesShort")}'\{Separator}ss'{Localization.Get("Menu/Chests/SecondsShort")}'" : $@"mm\{Separator}ss";
    }
}