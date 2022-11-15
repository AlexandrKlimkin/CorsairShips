using System.Diagnostics;

namespace GameServerProtocol.Sources.Logger {
    public static class ILoggerExtensions {
        [Conditional("DEBUG")]
        public static void ConditionalLog(this ILogger logger, object message) {
            logger?.Log(message);
        }

        [Conditional("DEBUG")]
        public static void ConditionalLogError(this ILogger logger, object message) {
            logger?.LogError(message);
        }
        
        [Conditional("DEBUG")]
        public static void ConditionalLogWarning(this ILogger logger, object message) {
            logger?.LogWarning(message);
        }
    }
}