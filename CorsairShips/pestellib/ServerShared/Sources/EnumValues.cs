using System;
using System.Collections.Generic;

namespace ServerShared.Sources
{
    public static class EnumValues<T>
    {
        private static readonly Dictionary<T, string> _enumToString = new Dictionary<T, string>();
        private static readonly Dictionary<string, T> _stringToEnum = new Dictionary<string, T>();

        static EnumValues()
        {
            foreach (var v in Enum.GetValues(typeof(T)) as T[])
            {
                var s = v.ToString();
                _enumToString[v] = s;
                _stringToEnum[s] = v;
            }
        }

        public static int Count()
        {
            return _enumToString.Count;
        }

        public static IEnumerable<T> Values()
        {
            return _enumToString.Keys;
        }
    }
}
