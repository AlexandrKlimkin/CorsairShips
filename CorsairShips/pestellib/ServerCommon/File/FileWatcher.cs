using System;
using System.IO;

namespace PestelLib.ServerCommon
{
    public class FileWatcher : IDisposable
    {
        FileSystemWatcher _watcher = new FileSystemWatcher();
        FileInfo _fileInfo;
        Action<FileInfo> _onChange;

        public FileWatcher(string path, Action<FileInfo> onChange)
        {
            _watcher.Path = Path.GetDirectoryName(path);
            _watcher.Filter = Path.GetFileName(path);
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            _watcher.Renamed += Watcher_Renamed;
            _watcher.Changed += Watcher_Changed;
            _watcher.EnableRaisingEvents = true;
            _onChange = onChange;
            _fileInfo = new FileInfo(path);
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            _watcher = null;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if ((e.ChangeType & WatcherChangeTypes.Changed) > 0)
                _onChange(_fileInfo);
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            _fileInfo = new FileInfo(e.FullPath);
        }
    }
}
