using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerShared.Sources
{
    public static class DictFlattenExtensions
    {
        public static T[] Flatten<T>(this Dictionary<T, T> dict)
        {
            return dict.Select(_ => new[] {_.Key, _.Value}).SelectMany(_ => _).ToArray();
        }

        public static Dictionary<T, T> Roughen<T>(this T[] array)
        {
            var result = new Dictionary<T,T>();
            var count = array.Length / 2;
            for (var i = 0; i < count; ++i)
            {
                var idx = i * 2;
                result[array[idx]] = array[idx + 1];
            }

            return result;
        }
    }
}
