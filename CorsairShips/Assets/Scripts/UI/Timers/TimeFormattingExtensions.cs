using PestelLib.Localization;
using System;
using System.Text;

namespace UI.TimeFormatting {
    public enum TimeDisplayStart { Day, Hour, Minute, Second }
    public static class TimeFormattingExtensions {
        public const string SkipFlag = "#SKIP";
        public static string FormatTime(this TimeSpan time, int displayedItems = 2, TimeDisplayStart displayStart = TimeDisplayStart.Day, TimeDisplayStart forceStart = TimeDisplayStart.Second, string dayMarker = null, string hourMarker = null, string minuteMarker = null, string secondMarker = null, string separator = " ") {
            var displayedBits = 0;
            var result = new StringBuilder();
            if (displayStart == TimeDisplayStart.Day) {
                if (time.TotalDays > 1 || forceStart == TimeDisplayStart.Day) {
                    result.Append((int)time.TotalDays);
                    result.Append(dayMarker);
                    displayedBits++;
                }
                else {
                    displayStart = TimeDisplayStart.Hour;
                }
            }
            if (!(displayedBits >= displayedItems) && (displayedBits > 0 || displayStart == TimeDisplayStart.Hour)) {
                if (time.TotalHours > 1 || forceStart == TimeDisplayStart.Hour) {
                    result.Append(FormatBit(displayedBits > 0 ? time.Hours : time.TotalHours, displayedBits, separator));
                    result.Append(hourMarker);
                    displayedBits++;
                }
                else {
                    displayStart = TimeDisplayStart.Minute;
                }
            }
            if (!(displayedBits >= displayedItems) && (displayedBits > 0 || displayStart == TimeDisplayStart.Minute)) {
                if (time.TotalMinutes > 1 || forceStart == TimeDisplayStart.Minute) {
                    result.Append(FormatBit(displayedBits > 0 ? time.Minutes : time.TotalMinutes, displayedBits, separator));
                    result.Append(minuteMarker);
                    displayedBits++;
                }
                else {
                    displayStart = TimeDisplayStart.Second;
                }
            }
            if (!(displayedBits >= displayedItems) && (displayedBits > 0 || displayStart == TimeDisplayStart.Second)) {
                result.Append(FormatBit(displayedBits > 0 ? time.Seconds : time.TotalSeconds, displayedBits, separator));
                result.Append(secondMarker);
                displayedBits++;
            }

            return result.ToString();
        }
        private static string FormatBit(double value, int count, string separator) {
            return $"{(count > 0 ? separator : string.Empty)}{((value < 10 && count > 0) ? "0" : string.Empty)}{(int)value}";
        }


        public static string FormatTime(this TimeSpan time, ILocalization localization, int displayedItems = 2, TimeDisplayStart displayStart = TimeDisplayStart.Day, TimeDisplayStart forceStart = TimeDisplayStart.Second, string separator = " ") {
            return FormatTime(time, displayedItems, displayStart, forceStart,
                localization.Get("time/DaysShort"),
                localization.Get("time/HoursShort"),
                localization.Get("time/MinutesShort"),
                localization.Get("time/SecondsShort"),
                separator);
        }

        public static string FormatTime(this  ILocalization localization, TimeSpan time, int displayedItems = 2, TimeDisplayStart displayStart = TimeDisplayStart.Day, TimeDisplayStart forceStart = TimeDisplayStart.Second, string separator = " ") {
            return FormatTime(time, displayedItems, displayStart, forceStart,
                localization.Get("time/DaysShort"),
                localization.Get("time/HoursShort"),
                localization.Get("time/MinutesShort"),
                localization.Get("time/SecondsShort"),
                separator);
        }
    }
}