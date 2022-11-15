using System;
using System.Collections.Generic;
using System.Text;

namespace PackageManager
{
    enum LogLevel
    {
        None,
        Error,
        Warning
    }

    static class Logger
    {
        public static Action<LogLevel, string> OnLog = (level, s) => { };

        private static void Log(LogLevel level, string message)
        {
            OnLog(level, message);
        }

        public static void LogError(string message)
        {
            Log(LogLevel.Error, message);
        }

        public static void LogWarn(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public static void LogDebug(string message)
        {
            Log(LogLevel.None, message);
        }
    }
}
