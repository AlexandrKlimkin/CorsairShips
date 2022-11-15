namespace PestelLib.SaveSystem.FileSystemStorage
{
    public interface ILog
    {
        void LogWarning(object message);
        void LogError(object message);
        void Log(object message);

        void ServerLogError(string tag, string message);
        void ServerLogInfo(string tag, string message);
    }
}