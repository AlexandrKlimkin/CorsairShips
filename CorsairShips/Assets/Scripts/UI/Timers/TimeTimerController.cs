using System;
using UI.TimeFormatting;

namespace UI.Timers {
    public class TimeTimerController : TimerController {
        protected override string CurrentFormattedTime => TimeSpan.FromSeconds(ElapsedFlooredTime).FormatTime(Localization);
        //public string TimeFormat { get; set; } = @"mm\:ss";
    }
}