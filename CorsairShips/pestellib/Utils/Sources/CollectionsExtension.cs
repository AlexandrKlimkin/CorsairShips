using System;
using System.Collections.Generic;

namespace PestelLib.Utils
{ 
	public static class CollectionsExtension  {
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }

	    public static T RandomElement<T>(this IList<T> list)
	    {
	        return list[UnityEngine.Random.Range(0, list.Count)];
	    }
	}
}