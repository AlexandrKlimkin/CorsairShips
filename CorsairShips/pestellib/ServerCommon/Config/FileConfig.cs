using System;
using System.IO;
using System.Threading;
using log4net;

namespace PestelLib.ServerCommon.Config
{
    /// <summary>
    /// Читает конфиг из файла в формате JSON от типа T.
    /// В случае изменения файла перечитывает конфиг.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FileConfig<T> : IDisposable where T: new()
    {
        /// <summary>
        /// Актуальный конфиг.
        /// </summary>
        public T Config { get; private set; }
        /// <summary>
        /// Загружает конфиг из указанного файла.
        /// Если выставлен watch то при изменении файла перезгружает его содержимое.
        /// Если filePath не сущуствет, создает с дефолтными значениями.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="watch"></param>
        public FileConfig(string filePath)
        {
            _fileInfo = new FileInfo(filePath);
            Reload();
            InitAutoReload(_fileInfo.FullName);
        }

        public void Reload()
        {
            Config = SimpleJsonConfigLoader.LoadConfigFromFile<T>(_fileInfo.FullName, true);
        }

        public void Dispose()
        {
            fileWatcher?.Dispose();
            fileWatcher = null;
        }

        /// <summary>
        /// Триггерится вотчером файла, что говорит нам о том что файл изменен
        /// пробуем перезагрузить его содержимое
        /// </summary>
        /// <param name="path"></param>
        private void TryReload(FileInfo path)
        {
            var retries = 10;

            IOException lastError = null;
            while (retries-- > 0)
            {
                try
                {
                    Reload();
                    return;
                }
                catch (IOException e)
                {
                    lastError = e;
                }
                Thread.Sleep(100);
            }

            Log.Error($"Failed to reload config '{path.FullName}'", lastError);
        }

        private void InitAutoReload(string path)
        {
            fileWatcher = new FileWatcher(path, TryReload);
        }

        private FileInfo _fileInfo;
        private FileWatcher fileWatcher;

        private static readonly ILog Log = LogManager.GetLogger(typeof(FileConfig<T>));
    }
}
