using UnityEngine;

namespace PestelLib.SaveSystem.FileSystemStorage
{
    public class UnityPlayerPrefs : IPlayerPrefs
    {
        public void SetString(string key, string val)
        {
            PlayerPrefs.SetString(key, val);
        }

        public string GetString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public void SetInt(string key, int val)
        {
            PlayerPrefs.SetInt(key, val);
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }
    }
}