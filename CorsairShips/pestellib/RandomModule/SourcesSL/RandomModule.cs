using System;
using System.Collections.Generic;
using PestelLib.SharedLogicBase;

namespace PestelLib.SharedLogic.Modules
{
    [System.Reflection.Obfuscation(ApplyToMembers=false)]
    public class RandomModule : SharedLogicModule<RandomModuleState>
    {
        private const int MaxSeed = 233280;
        
        public override int MakeDefaultStatePriority
        {
            get { return 1000; }
        }

        public override void MakeDefaultState()
        {
            base.MakeDefaultState();
            const int guidSize = 16;
            const int intSize = 4;

            State.RandomSeed = PestelMathf.Abs(BitConverter.ToInt32(SharedLogic.PlayerId.ToByteArray(), guidSize - intSize) % MaxSeed);
        }

        public decimal Random()
        {
            SharedCommandCallstack.CheckCallstack();
            var nextSeed = (int)((State.RandomSeed * 9301U + 49297) % MaxSeed);
            State.RandomSeed = nextSeed;
            return nextSeed / 233280M;
        }

        public int RandomInt(int max)
        {
            SharedCommandCallstack.CheckCallstack();
            return (int)(Random()*max);
        }

        public int RandomInt(int min, int max)
        {
            SharedCommandCallstack.CheckCallstack();
            int range = max - min;
            return RandomInt(range) + min;
        }

        public void RandomBytes(byte[] data)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = (byte)RandomInt(byte.MaxValue);
            }
        }

        public decimal RandomDecimal(decimal min, decimal max)
        {
            SharedCommandCallstack.CheckCallstack();
            var delta = max - min;
            if (delta < 0)
                delta = -delta;
            return min + Random() * delta;
        }

        public T ChooseByRandom<T>(IEnumerable<T> collection, Func<T, decimal> proportionGetter)
        {
            SharedCommandCallstack.CheckCallstack();

            return ChooseByPredefinedRandom<T>(collection, proportionGetter, Random());
        }
        
        public T RandomElement<T>(List<T> list)
        {
            return list[RandomInt(0, list.Count)];
        }
        
        private T ChooseByPredefinedRandom<T>(IEnumerable<T> collection, Func<T, decimal> proportionGetter, decimal rndD)
        {
            var rnd = rndD;
            var sum = 0M;
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
