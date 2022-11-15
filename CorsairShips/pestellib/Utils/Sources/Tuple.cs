using System;
using System.Collections.Generic;
using System.Globalization;

namespace PestelLib.Utils
{
    public class Tuple<T1, T2> : IFormattable
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        private static readonly IEqualityComparer<T1> Item1Comparer = EqualityComparer<T1>.Default;
        private static readonly IEqualityComparer<T2> Item2Comparer = EqualityComparer<T2>.Default;

        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public override int GetHashCode()
        {
            var hc = 0;
            if (!ReferenceEquals(Item1, null))
                hc = Item1Comparer.GetHashCode(Item1);
            if (!ReferenceEquals(Item2, null))
                hc = (hc << 3) ^ Item2Comparer.GetHashCode(Item2);

            return hc;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Tuple<T1, T2>;
            if (ReferenceEquals(other, null))
                return false;

            return Item1Comparer.Equals(other.Item1) && Item2Comparer.Equals(other.Item2);
        }

        public override string ToString()
        {
            return ToString(null, CultureInfo.CurrentCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, format ?? "{0},{1}", Item1, Item2);
        }
    }
}