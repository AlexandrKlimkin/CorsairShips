using System;
using System.Collections;
using System.IO;
using System.Text;
using BestHTTP;
using UnityDI;
using UnityEngine;
using Newtonsoft.Json;
using PestelLib.ClientConfig;
using PestelLib.ServerLogProtocol;
using PestelLib.Utils;
using UnityEngine.Assertions;

namespace PestelLib.ServerLogClient
{
    public class ServerLog : MonoBehaviour
    {
#pragma warning disable 649
        [Dependency] private ITimeProvider _sharedTime;
        [Dependency] private Config _config;
        [Dependency] private IPlayerIdProvider _requestQueue;
#pragma warning restore 649

        public static string GameName = "Undefined";

        private float _lastSyncTimestamp = 0;
        private int _resendAttemp = 0;
        public float MaxSyncDelay = 2f;
        public int MaxResendAttemps = 15;

        private LogMessagesGroup _messages = new LogMessagesGroup();
        private HTTPRequest _request;

        private static ServerLog _instance;
        private byte[] _serializedMessagesBytes;
        private int messagesToSendCount;

        private string ServerLogPath
        {
            get { return Application.persistentDataPath + "/serverLog.bin"; }
        }

        protected virtual void Awake()
        {
            ContainerHolder.Container.BuildUp(this);

            if (_config == null)
            {
                Debug.Log("Config is null! ServerLogClient will not work");
                return;
            }

            if (!_config.LogServerEnabled) return;

            if (_sharedTime == null)
            {
                Debug.Log("ITimeProvider is null! ServerLogClient will not work. You can register it in container: " +
                          "container.RegisterCustom<ITimeProvider>(() => container.Resolve<SharedTime>());");
            }

            if (_requestQueue == null)
            {
                Debug.Log("IPlayerIdProvider is null!  ServerLogClient will not work. You can register it in container: " +
                          "container.RegisterCustom<IPlayerIdProvider>(() => container.Resolve<RequestQueue>());");
            }
        }

        protected virtual IEnumerator Start()
        {
            if (_sharedTime == null || _config == null || _requestQueue == null)
            {
                yield break;
            }

            TryToRestoreMessagesFromPrevSession();

            if (!_config.LogServerEnabled)
            {
                enabled = false;
                yield break;
            }

            while (true)
            {
                if (_request != null)
                {
                    if (_request.Exception != null || _request.State == HTTPRequestStates.Error)
                    {
                        if (_resendAttemp < MaxResendAttemps)
                        {
                            _resendAttemp++;
                            //задержка до каждой следующей попытки отсылки увеличивается, что бы точно не спамить сервер запросами в случае каких-либо ошибок
                            yield return new WaitForSecondsRealtime(1f * _resendAttemp);
                            SendToServer();
                        }
                        else
                        {
                            Debug.LogError("Unable to send log to server " + _config.LogServer);
                            _request = null;
                            _resendAttemp = 0;
                        }
                    }
                    else if (_request.State == HTTPRequestStates.Finished)
                    {
                        if (_messages.Messages.Count > 0)
                        {
                            _messages.Messages.RemoveRange(0, messagesToSendCount);

                        }

                        try
                        {
                            if (File.Exists(ServerLogPath))
                            {
                                File.Delete(ServerLogPath);
                            }
                        }
                        catch
                        {
                            //ignored
                        }

                        _request = null;
                    }
                }
                else
                {
                    if (IsTimeToSync && _messages.Messages.Count > 0)
                    {
                        messagesToSendCount = _messages.Messages.Count;
                        _serializedMessagesBytes = SaveLogs();

                        _resendAttemp = 0;

                        SendToServer();
                    }
                }

                yield return new WaitForSecondsRealtime(1f);
            }
        }

        byte[] SaveLogs()
        {
            var serializedMessages = JsonConvert.SerializeObject(_messages);
            var bytes = Encoding.UTF8.GetBytes(serializedMessages);

            try
            {
                File.WriteAllBytes(ServerLogPath, bytes);
            }
            catch
            {
                //сохранение в файл не принципиально
            }

            return bytes;
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                SaveLogs();
        }

        void OnApplicationQuit()
        {
            SaveLogs();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveLogs();
            }
        }

        private void TryToRestoreMessagesFromPrevSession()
        {
            try
            {
                if (File.Exists(ServerLogPath))
                {
                    const int megabyte = 1048576;
                    var length = new FileInfo(ServerLogPath).Length;
                    if (length > megabyte)
                    {
                        Debug.Log("Server log is too big. Destroying server log");
                        File.Delete(ServerLogPath);
                        return;
                    }

                    _serializedMessagesBytes = File.ReadAllBytes(ServerLogPath);
                    var json = Encoding.UTF8.GetString(_serializedMessagesBytes);
                    var oldMessages = JsonConvert.DeserializeObject<LogMessagesGroup>(json);
                    if (oldMessages != null && oldMessages.Messages != null)
                    {
                        _messages.Messages.InsertRange(0, oldMessages.Messages);
                    }
                }
            }
            catch
            {
            }
        }

        private void SendToServer()
        {
            _request = new HTTPRequest(new Uri(_config.LogServer + "WriteLog.ashx"), HTTPMethods.Post);
            _request.RawData = _serializedMessagesBytes;
            _request.Send();
        }

        private float TimeSinceLastSync
        {
            get { return Time.realtimeSinceStartup - _lastSyncTimestamp; }
        }

        private bool IsTimeToSync
        {
            get { return TimeSinceLastSync > MaxSyncDelay; }
        }
        
        protected virtual void LogMessage(LogLevel level, string tag, string message)
        {
            if (_requestQueue == null || _sharedTime == null) return;

            if (!_config.LogServerEnabled) return;

            if (GameName == "Undefined")
            {
                Debug.Log("Please set ServerLogClient.GameName to appropriate value");
            }

            var msg = new PestelLib.ServerLogProtocol.LogMessage
            {
                BuildVersion = Application.version,
                Game = GameName,
                Message = (++MessageNumber) + " " + message,
                Platform = Platform,
                PlayerId = _requestQueue.PlayerId,
                PlayerIdString = _requestQueue.PlayerId.ToString(),
                Tag = tag,
                Time = _sharedTime.Now,
                Type = level
            };

            _messages.Messages.Add(msg);

            Debug.LogFormat("ServerLog: {0} {1}", tag, message);

#if UNITY_EDITOR
            using (var writer = new StreamWriter(File.Open(Application.dataPath + "/ServerLog.log", FileMode.Append)))
            {
                writer.WriteLine(DateTime.UtcNow + " " + tag + " " + MessageNumber + " " + message);
            }
#endif
        }

        public static void LogDebug(string tag, string message)
        {
            Log(LogLevel.Debug, tag, message);
        }

        public static void LogError(string tag, string message)
        {
            Log(LogLevel.Error, tag, message);
        }

        public static void LogInfo(string tag, string message)
        {
            Log(LogLevel.Log, tag, message);
        }

        public static void LogException(string tag, string message)
        {
            Log(LogLevel.Exception, tag, message);
        }

        private static int MessageNumber
        {
            get { return PlayerPrefs.GetInt("ServerLogMessageNumber", 0); }
            set { PlayerPrefs.SetInt("ServerLogMessageNumber", value); }
        }

        public static void Log(LogLevel level, string tag, string message)
        {
            if (_instance == null)
            {
                //так мы можем получить вместо ServerLogClient его наследника, если в какой-то игре это необходимо
                _instance = ContainerHolder.Container.Resolve<ServerLog>(); 
                if (_instance == null)
                {
                    //если не зарегистрировано ничего, можем зарегистрировать и тут же инстанцировать самостоятельно
                    ContainerHolder.Container.RegisterUnitySingleton<ServerLog>(null, true);
                    _instance = ContainerHolder.Container.Resolve<ServerLog>();
                    
                    //заодно регистрируем и сразу создаём логгер ошибок RequestQueue
                    //ContainerHolder.Container.RegisterUnitySingleton<ServerLogRequestQueueErrors>(null, true);
                    //ContainerHolder.Container.Resolve<ServerLogRequestQueueErrors>();

                    var config = ContainerHolder.Container.Resolve<Config>();
                    if (config != null && config.LogCollectUnityErrors)
                    {
                        //заодно регистрируем и создаём логгер юнити ошибок и эксепшенов
                        ContainerHolder.Container.RegisterUnitySingleton<ServerLogUnityErrorsHandler>(null, true);
                        ContainerHolder.Container.Resolve<ServerLogUnityErrorsHandler>();
                    }
                }
            }
            _instance.LogMessage(level, tag, message);
        }

        private static Platform Platform
        {
            get {
                switch (Application.platform)
                {
                    case RuntimePlatform.LinuxPlayer:
                    case RuntimePlatform.OSXPlayer:
                    case RuntimePlatform.WindowsPlayer:
                        return Platform.Standalone;

                    case RuntimePlatform.LinuxEditor:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.WindowsEditor:
                        return Platform.Editor;

                    case RuntimePlatform.Android:
                        return Platform.Android;

                    case RuntimePlatform.IPhonePlayer:
                        return Platform.iOS;
                        
                    default:
                        return Platform.Other;
                }
            }
        }
    }
}