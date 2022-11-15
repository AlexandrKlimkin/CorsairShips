using System;
using UnityEngine;

namespace NorskaLib.Utilities
{
    public struct EnumUtils
    {
        public static T[] GetArray<T>() where T : System.Enum
        {
            var valuesArray = Enum.GetValues(typeof(T));
            var enumArray = new T[valuesArray.Length];
            for (int i = 0; i < valuesArray.Length; i++)
                enumArray[i] = (T)valuesArray.GetValue(i);

            return enumArray;
        }

        public static bool HasFlag(int mask, int layer)
        {
            return (mask & layer) != 0;
        }

        public static bool HasFlag(LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
    }
}
