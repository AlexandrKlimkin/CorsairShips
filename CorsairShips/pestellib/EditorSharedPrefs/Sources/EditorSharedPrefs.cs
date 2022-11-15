using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace PestelLib.EditorSharedPreferences
{
    public static class EditorSharedPrefs
    {
        public static void SetString(string key, string value)
        {
            var config = ReadConfig();

            config.StoreValue(key, value);

            WriteConfig(config);
        }

        public static string GetString(string key, string defaultValue)
        {
            var config = ReadConfig();

            if (config == null)
                return defaultValue;

            var record = config.GetValue(key);

            if (record == null)
                return defaultValue;

            return (string)config.GetValue(key).Value;
        }

        private static EditorSharedConfig ReadConfig()
        {
            try
            {
                var configTxt = File.ReadAllText(Application.dataPath + "/Editor/editorSharedConfig.json");
                var config = JsonConvert.DeserializeObject<EditorSharedConfig>(configTxt);


                return config;
            }
            catch (FileNotFoundException ex)
            {
                Debug.LogError("Create file Assets/Editor/editorSharedConfig.json. Write {\"Prefs\": []} to the json. After google spreadsheet will tell you what to do. He can be rough sometimes. Be patient");
            }
            catch (Exception ex)
            {
                Debug.LogError("Sometimes went wrong with editorSharedConfig.json: " + ex.Message);
            }

            return null;
        }

        private static void WriteConfig(EditorSharedConfig config)
        {
            var serializedConfig = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(Application.dataPath + "/Editor/editorSharedConfig.json", serializedConfig);
        }
    }
}