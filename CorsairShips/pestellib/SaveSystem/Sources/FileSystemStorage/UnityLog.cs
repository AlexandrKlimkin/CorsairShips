using PestelLib.ServerLogClient;
using UnityEngine;

namespace PestelLib.SaveSystem.FileSystemStorage
{
    public class UnityLog : ILog
    {
        public void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        public void LogError(object message)
        {
            Debug.LogError(message);
        }

        public void Log(object message)
        {
            Debug.Log(message);
        }

        public void ServerLogError(string tag, string message)
        {
            ServerLog.LogError(tag, message);
        }

        public void ServerLogInfo(string tag, string message)
        {
            ServerLog.LogInfo(tag, message);
        }
    }
}