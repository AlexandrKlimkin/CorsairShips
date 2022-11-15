using System;

namespace ServerShared
{
    public static class ArrayExtensions
    {
        public static int IndexOf<T>(this T[] source, Func<T, bool> predicate)
        {
            for (var i = 0; i < source.Length; ++i)
            {
                if (predicate(source[i]))
                    return i;
            }

            return -1;
        }

        public static int IndexOf<T>(this T[] source, T needle) where T: IComparable
        {
            return source.IndexOf(_ => _.CompareTo(needle) == 0);
        }
    }
}
