using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NorskaLib.Utilities
{
    public struct RandomUtils
    {
        public static bool Bool(float chance, out float dice)
        {
            dice = Random.Range(0, 0.99f);
            return dice < Mathf.Clamp01(chance);
        }
        public static bool Bool(float chance)
        {
            return Bool(chance, out var dice);
        }

        public static int Integer(int min, int max, int[] exeptions)
        {
            var pool = new List<int>();
            for (int i = min; i < max; i++)
            {
                var exeption = false;
                for (int j = 0; j < exeptions.Length; j++)
                    if (i == exeptions[j])
                    {
                        exeption = true;
                        break;
                    }

                if (!exeption)
                    pool.Add(i);
            }

            var index = Random.Range(0, pool.Count);

            return pool[index];
        }
        public static int IntegerInclusive(int min, int max)
        {
            return Random.Range(min, max + 1);
        }

        public static Vector3 RandomVector3()
        {
            return RandomVector3(-1, 1, -1, 1, -1, 1);
        }
        public static Vector3 RandomVector3(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            var x = Random.Range(minX, maxX);
            var y = Random.Range(minY, maxY);
            var z = Random.Range(minZ, maxZ);

            return new Vector3(x, y, z);
        }

        public static T GetRandomValue<T>(Meta<T>[] metas)
        {
            var weights = metas.Select(m => m.weight).ToArray();
            var index = GetRandomIndex(weights);
            return metas[index].value;
        }
        public static int GetRandomIndex(float[] weigths)
        {
            float weightsSum = 0;
            for (int i = 0; i < weigths.Length; i++)
                weightsSum += weigths[i];

            float roll = Random.Range(0, weightsSum);

            float lastMin = 0;
            float lastMax = 0;
            int index = -1;
            for (int i = 0; i < weigths.Length; i++)
            {
                if (i > 0)
                    lastMin += weigths[i - 1];

                lastMax += weigths[i];

                if (roll >= lastMin && roll < lastMax && !Mathf.Approximately(weigths[i], 0))
                    index = i;
            }
            return index;
        }

        public struct Meta<T>
        {
            public T value;
            public float weight;
        }
    }
}
