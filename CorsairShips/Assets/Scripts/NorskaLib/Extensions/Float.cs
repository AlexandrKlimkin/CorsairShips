using System.Collections;
using System.Collections.Generic;

namespace NorskaLib.Extensions
{
    public static class FloatExtensions
    {
        public static float Square(this float value)
        {
            return value * value;
        }

        public static float Cube(this float value)
        {
            return value * value * value;
        }

        public static bool IsBetweenInclusive(this float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        public static bool IsBetweenExclusive(this float value, float min, float max)
        {
            return value > min && value < max;
        }
    }
}