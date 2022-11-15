using System;

namespace PestelLib.Utils
{
    public static class DateTimeUtils
    {
        public static long TotalSeconds(this DateTime dateTime)
        {
            TimeSpan t = dateTime - new DateTime(1970, 1, 1);
            long secondsSinceEpoch = (long)t.TotalSeconds;
            return secondsSinceEpoch;
        } 
    }
}