using System;

namespace RCUtils.Sources.Extensions {
    public static class TimestampExtensions {
        public static int TimeStampsAbsIncrement(int last, int next) {
            if (last > 0 && next < 0) //to prevent int overflow
                return (int.MaxValue - last) + (next - int.MinValue);
            return Math.Abs(next - last);
        }
    }
}