using UnityEngine;
using System.Collections.Generic;

namespace PestelLib.Utils
{ 
	public class PlayerPrefsCached
	{
	    private static readonly Dictionary<string, int> _intCache = new Dictionary<string, int>();

        public static void SetInt(string key, int value)
        {
            _intCache[key] = value;
            PlayerPrefs.SetInt(key, value);
        }

	    public static int GetInt(string key, int defaultValue)
	    {
	        if (!_intCache.ContainsKey(key))
	        {
                _intCache[key] = PlayerPrefs.GetInt(key, defaultValue);
	        }
            return _intCache[key];
	    }

        public static int GetInt(string key)
        {
            return GetInt(key, 0);
        }
	}
}