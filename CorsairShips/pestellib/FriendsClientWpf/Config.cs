using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using PestelLib.ServerCommon.Config;

namespace FriendsClientWpf
{
    static class ConfigLoader
    {
        public static Config Instance => _instance.Value;
        public static string Path => ConfigPath.Value;

        private static Lazy<Config> _instance = new Lazy<Config>(() =>
        {
            return SimpleJsonConfigLoader.LoadConfigFromFile<Config>(Path, true);
        });

        private static Lazy<string> ConfigPath = new Lazy<string>(() =>
        {
            var opts = Application.Current.Properties["Options"] as Options;
            string filePath = null;
            if (opts != null)
                filePath = opts.ConfigPath;
            else
                filePath = "FriendsClientWpf.json";
            return filePath;
        });

        public static void Save()
        {
            File.WriteAllText(Path, JsonConvert.SerializeObject(Instance, Formatting.Indented));
        }
    }

    class Config
    {
        public Guid PlayerId = Guid.NewGuid();
        public bool Autostart = false;
        public string AuthData = string.Empty;
        public TimeSpan UpdatePeriod = TimeSpan.FromMilliseconds(16);
        public bool DebugEvents = true;
        public bool StartUpdateOnConnect = true;
        public string FriendsServerUrl = string.Empty;
        public TimeSpan BattleAutostartDelay = TimeSpan.FromMinutes(2);

        public string Duplicate(int instNo)
        {
            var name = Path.GetFileNameWithoutExtension(ConfigLoader.Path) + $"_{instNo}.json";
            if (File.Exists(name)) return name;

            var oldId = PlayerId;
            try
            {
                PlayerId = Guid.NewGuid();
                File.WriteAllText(name, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            finally 
            {
                PlayerId = oldId;
            }
            return name;
        }
    }
}
