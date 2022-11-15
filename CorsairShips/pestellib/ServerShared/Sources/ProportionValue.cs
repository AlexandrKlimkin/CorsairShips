using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PestelLib.ServerShared
{ //reference http://stackoverflow.com/questions/3655430/selection-based-on-percentage-weighting

    public class ProportionValue<T>
    {
        public float Proportion { get; set; }
        public T Value { get; set; }

        public ProportionValue<T> Clone()
        {
            return new ProportionValue<T> { Proportion = this.Proportion, Value = this.Value };
        }
    }

    public static class ProportionValue
    {
        public static ProportionValue<T> Create<T>(float proportion, T value)
        {
            return new ProportionValue<T> { Proportion = proportion, Value = value };
        }

        static Random random = new Random();

        public static T ChooseByRandom<T>(
            this IEnumerable<ProportionValue<T>> collection)
        {

            return ChooseByPredefinedRandom<T>(collection, (float)random.NextDouble());
        }

        public static T ChooseByPredefinedRandom<T>(
            this IEnumerable<ProportionValue<T>> collection, float random)
        {

            float sum = 0f;
            foreach (var item in collection)
            {
                sum += item.Proportion;
            }

            float rnd = random * sum;

            foreach (var item in collection)
            {
                if (rnd < item.Proportion)
                {
                    return item.Value;
                }
                rnd -= item.Proportion;
            }
            return default(T);
        }

        public static T ChooseByRandom<T>(this IEnumerable<T> collection, Func<T, float> proportionGetter)
        {
            return ChooseByPredefinedRandom<T>(collection, proportionGetter, (float)random.NextDouble());
        }

        public static T ChooseByPredefinedRandom<T>(this IEnumerable<T> collection, Func<T, float> proportionGetter, decimal rnd)
        {
            return ChooseByPredefinedRandom(collection, proportionGetter, (float) rnd);
        }

        public static T ChooseByPredefinedRandom<T>(this IEnumerable<T> collection, Func<T, float> proportionGetter, float rnd)
        {
            float sum = 0f;
            foreach (var item in collection)
            {
                sum += proportionGetter(item);
            }

            rnd = rnd * sum;

            foreach (var item in collection)
            {
                if (rnd < proportionGetter(item))
                {
                    return item;
                }
                rnd -= proportionGetter(item);
            }
            return default(T);
        }
    }
}