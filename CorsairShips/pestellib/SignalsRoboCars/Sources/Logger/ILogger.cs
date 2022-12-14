namespace GameServerProtocol.Sources.Logger {
    public interface ILogger {
        void Log(object message);
        void LogError(object message);
        void LogWarning(object message);
    }
}