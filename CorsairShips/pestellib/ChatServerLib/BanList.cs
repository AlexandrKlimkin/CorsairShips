using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using PestelLib.ChatCommon;
using PestelLib.ServerCommon;

namespace PestelLib.ChatServer
{
    class BanList
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BanList));
        private FileWatcher _fileWatcher;
        private FileInfo _banListFile;
        private object _sync = new object();
        private List<string> _rules = new List<string>();

        public BanList()
        {
            var path = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
            path = Path.Combine(path, "banlist.txt");
            _banListFile = new FileInfo(path);
            if (!_banListFile.Exists)
            {
                using (File.Open(_banListFile.FullName, FileMode.OpenOrCreate)) {}
            }
            _fileWatcher = new FileWatcher(_banListFile.FullName, OnChange);
        }

        private void OnChange(FileInfo fileInfo)
        {
            try
            {
                TryUpdate(fileInfo);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void TryUpdate(FileInfo fileInfo)
        {
            while (true)
            {
                try
                {
                    var newRules = new List<string>();
                    using (var f = new StreamReader(File.OpenRead(fileInfo.FullName)))
                    {
                        var l = f.ReadLine();
                        newRules.Add(l);
                    }

                    lock (_sync)
                    {
                        _rules = newRules;
                    }
                }
                catch (IOException)
                {
                }
                Thread.Sleep(100);
            }
        }

        public bool IsBanned(ClientInfo clientInfo)
        {
            bool result;
            lock (_sync)
            {
                var r = _rules.FirstOrDefault(_ => _ == clientInfo.Token);
                result = r != null;
            }
            return result;
        }
    }
}
