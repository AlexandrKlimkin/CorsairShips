using System;

namespace PestelLib.Utils
{
    static public class FormatTime
    {
        public static string Format(TimeSpan timeSpan)
        {
            return string.Format("{0:D1}:{1:D2}:{2:D2}",
                (int)timeSpan.TotalHours,
                timeSpan.Minutes,
                timeSpan.Seconds
            );
        }

        public static string FormatAuto(TimeSpan remainTime, string shortFormat = "{0:D1}:{1:D2}:{2:D2}", string longFormat = "{0} d. {1} hrs")
        {
            if (remainTime.TotalDays >= 1)
            {
                //offers_time_long	{0}д {1}ч
                return string.Format(longFormat, remainTime.Days, remainTime.Hours);
            }
            else
            {
                //offers_time_short   { 0:D1}:{ 1:D2}:{ 2:D2}
                return string.Format(shortFormat, remainTime.Hours,
                    remainTime.Minutes, remainTime.Seconds);
            }
        }
    }
}
