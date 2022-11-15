using System.Diagnostics;
using log4net;

namespace PestelLib.Log.Sources.log4net
{
    public static class LogExtensions
    {
        [Conditional("DEBUG")]
        public static void DebugLog(this ILog logger, string message)
        {
            logger.Debug(message);
        }
    }
}
