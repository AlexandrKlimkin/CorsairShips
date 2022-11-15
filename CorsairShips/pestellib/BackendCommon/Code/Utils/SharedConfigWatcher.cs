using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using PestelLib.ClientConfig;
using PestelLib.ServerCommon;
using log4net;

namespace Backend.Code.Utils
{
    public class SharedConfigWatcher
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(SharedConfigWatcher));
        private static readonly string Path = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/sharedConfig.json";
        private FileWatcher _watcher;
        private static Lazy<SharedConfigWatcher> _instance = new Lazy<SharedConfigWatcher>(() => new SharedConfigWatcher());
        public static SharedConfigWatcher Instance => _instance.Value;
        public SharedConfig Config { get; private set; }

        public volatile FileInfo _fileInfo;

        private SharedConfigWatcher() 
        {
            _log.Debug($"Loading shared config from {Path}.");
            _fileInfo = new FileInfo(Path);
            _watcher = new FileWatcher(Path, OnChanged);
            var configText = File.ReadAllText(Path);
            Config = JsonConvert.DeserializeObject<SharedConfig>(configText);
        }

        private void OnChanged(FileInfo fi)
        {
            _log.Debug($"Shared config change detected.");
            _fileInfo = fi;
            while (true)
            {
                try
                {
                    _log.Debug($"Reloading shared config from {_fileInfo.FullName}.");
                    var configText = File.ReadAllText(_fileInfo.FullName);
                    Config = JsonConvert.DeserializeObject<SharedConfig>(configText);
                    break;
                }
                catch (Exception e)
                {
                    _log.Error("Read shared config error.", e);
                    Thread.SpinWait(1);
                }
            }
        }
    }
}