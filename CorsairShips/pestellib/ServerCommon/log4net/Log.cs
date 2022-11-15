using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Config;

namespace PestelLib.ServerCommon
{
    public static class Log
    {
        public static string LogDirectory { get; private set; }

        private static ILog Logger = LogManager.GetLogger(typeof(Log));
        static Log()
        {
            var fi = new FileInfo("log4net.config");
            if (fi.Exists)
            {
                var logRep = LogManager.GetRepository(Assembly.GetCallingAssembly());
                XmlConfigurator.Configure(logRep, fi);
                Logger.InfoFormat("Log config: {0}", fi.FullName);
            }
            else
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                var basePath = Path.GetDirectoryName(path);
                fi = new FileInfo(Path.Combine(basePath, "log4net.config"));
                if (fi.Exists)
                {
                    var logRep = LogManager.GetRepository(Assembly.GetCallingAssembly());
                    XmlConfigurator.Configure(logRep, fi);
                    Logger.InfoFormat("Log config: {0}", fi.FullName);
                }
            }
        }

        public static void SetLogSubfolder(string subfolderName)
        {
            var h = (log4net.Repository.Hierarchy.Hierarchy) LogManager.GetRepository("log4net-default-repository");

            foreach (IAppender a in h.Root.Appenders)
            {
                if (a is FileAppender)
                {
                    FileAppender fa = (FileAppender)a;
                    // Programmatically set this to the desired location here
                    //string logFileLocation = @"C:\temp\MyFile.log";

                    // Uncomment the lines below if you want to retain the base file name
                    // and change the folder name...
                    FileInfo fileInfo = new FileInfo(fa.File);
                    LogDirectory = fileInfo.Directory.FullName + Path.DirectorySeparatorChar + subfolderName + Path.DirectorySeparatorChar;
                    if (!Directory.Exists(LogDirectory))
                    {
                        Directory.CreateDirectory(LogDirectory);
                    }
                    var logFileLocation = LogDirectory + fileInfo.Name;

                    fa.File = logFileLocation;
                    fa.ActivateOptions();
                    break;
                }
            }
        }

        public static void Init()
        {
        }
    }
}
