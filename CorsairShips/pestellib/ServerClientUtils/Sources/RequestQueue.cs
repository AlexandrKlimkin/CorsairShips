using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using BestHTTP;
using GoogleSpreadsheet;
using MessagePack;
using PestelLib.ClientConfig;
using PestelLib.SaveSystem;
using PestelLib.ServerShared;
using PestelLib.Utils;
using UnityDI;
using UnityEngine;
using S;
using PestelLib.SaveSystem.SQLiteStorage;

namespace PestelLib.ServerClientUtils
{
    public class RequestQueue : IDisposable, IPlayerIdProvider
    {
        public Action<DefsData> OnDefUpdate = (d) => { };
        public Action<Response, DataCollection> OnComplete = (r, d) => { };
        public Action<Response, Exception> OnFailed = (r, e) => { };
        public Action<Response, Exception> OnConnectionProblem = (response, exception) => { };
        public Action<Response, Exception> OnConnectionWarning = (response, exception) => { };

#pragma warning disable 649, 169
        [Dependency] private Config _config;
        [Dependency] private UpdateProvider _updateProvider;
        [Dependency] private IGameDefinitions _gameDefinitions;
        [Dependency] private IStorage _storage;
#pragma warning restore 649, 169

        private DefinitionsUpdater _definitionsUpdater = new DefinitionsUpdater();
        
        private const float ConnectionTimeout = 15f;
        private const int MaxAttempsToWarning = 3; //must be less than MaxAttemps

        private const float InitialDelay = 1f;
        private const float NextDelayCoeff = 1f;
        private const float MaxDelay = 1f;

        private long SessionId { get; set; }

        public bool DisableWritePlayerIdFromResponse = false;

        private List<RequestQueueItem> _requests = new List<RequestQueueItem>();
        private List<RequestQueueItem> _asyncRequests = new List<RequestQueueItem>();

        private uint SharedLogicCrc { get; set; }

        private bool _frozen;

        public bool IsFrozen
        {
            get { return _frozen; }
        }

        private bool _isFirstRequest = true;

        public RequestQueue()
        {
            StorageInitializer.TryInitStorage();
            ContainerHolder.Container.BuildUp(this);
            _updateProvider.OnUpdate += UpdateProviderOnOnUpdate;
            SharedLogicCrc = _config.SharedConfig.SharedLogicCrc;
        }

        public void Dispose()
        {
            _updateProvider.OnUpdate -= UpdateProviderOnOnUpdate;
        }

        private float CalculateDelay(int attempt)
        {
            //time grows up in geometric progression, until reach max value
            var delay = InitialDelay * (1 - Mathf.Pow(NextDelayCoeff, attempt - 1)) /
                (1 - NextDelayCoeff);
            delay = Mathf.Min(delay, MaxDelay);
            //Debug.Log(DateTime.Now + " Request delay: " + delay);
            return delay;
        }

        private NetworkType NetworkType
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android: 
                        return NetworkType.ANDROID_NETWORK;
                    case RuntimePlatform.IPhonePlayer:
                        return NetworkType.IOS_NETWORK;
                    default:
                        return NetworkType.UNDEFINED_NETWORK;
                }
            }
        }

        private string PlayerIdKey
        {
            get
            {
                return PlayerPrefKeys.PlayerIdKey;
                //два редактора теперь всегда два отдельных устройства 
                //из-за того что у них разные DeviceId и разные SQLite БД, в которых хранится PlayerId
            }
        }

        public string DeviceId
        {
            get
            {
                if (!Application.isEditor)
                    return SystemInfo.deviceUniqueIdentifier;

                //в редакторе мы хотим считать игру, запущенную с двух разных мест за два отдельных устройства
                //поэтому прибавляется dataPath
                return Application.dataPath + SystemInfo.deviceUniqueIdentifier;
            }
        }

        public Guid PlayerId
        {
            get
            {
                var guidString = _storage.GetString(PlayerIdKey, Guid.Empty.ToString());
                Guid guid;
                try
                {
                    guid = new Guid(guidString);
                }
#pragma warning disable 168
                catch (Exception e)
#pragma warning restore 168
                {
                    return Guid.Empty;
                }
                return guid;
            }
            set
            {
                _storage.SetString(PlayerIdKey, value.ToString());
            }
        }

        public int ShortId
        {
            get
            {
                var idString = _storage.GetString(PlayerPrefKeys.PlayerShortIdKey, null);
                if (int.TryParse(idString, out var r))
                    return r;
                return 0;
            }
            set
            {
                _storage.SetString(PlayerPrefKeys.PlayerShortIdKey, value.ToString());
            }
        }

        public int CurrentRequestFailureCount
        {
            get
            {
                if (_requests.Count > 0 && _requests[0] != null)
                {
                    return _requests[0].FailureCount;
                }
                else
                {
                    return 0;
                }
            }
        }

        public RequestQueueItem SendRequest(string method, Request requestHeader, Action<Response, DataCollection> onComplete, byte[] data = null, byte[] state = null, bool async = false)
        {
            if (_config.UseLocalState)
            {
                Debug.LogWarning("Send request is not allowed in offline mode");
                return null;
            }

            requestHeader.UserId = PlayerId.ToByteArray();
            requestHeader.DeviceUniqueId = DeviceId;
            requestHeader.NetworkId = NetworkType;

            requestHeader.SharedLogicVersion = (int)SharedLogicCrc;
            requestHeader.DefsVersion = _definitionsUpdater.DefinitionsVersion;

            var rg = new RequestQueueItem
            {
                Method = method,
                OnComplete = onComplete,
                Header = requestHeader,
                Data = data,
                State = state
            };

            if(async)
                _asyncRequests.Add(rg);
            else
                _requests.Add(rg);
            return rg;
        }

        public void CancelRequest(RequestQueueItem requestQueueItem)
        {
            if (requestQueueItem.Loader != null)
            {
                requestQueueItem.Loader.Abort();
            }

            _requests.Remove(requestQueueItem);
        }

        public void Freeze()
        {
            _frozen = true;
            _updateProvider.OnUpdate -= UpdateProviderOnOnUpdate;
        }

        [Obsolete]
        public void ClearAllRequests()
        {
            Debug.LogError("That method is obsolete");
        }

        public void SendRequestSerialized(byte[] postData, Action<Response, DataCollection> onComplete)
        {
            var rg = new RequestQueueItem
            {
                OnComplete = onComplete,
                PostData = postData
            };

            _requests.Add(rg);
        }

        public void TryToConnectImmediately()
        {
            if (_requests.Count > 0)
            {
                _requests[0].NextAttemptTimestamp = Time.realtimeSinceStartup + 1;
            }
        }

        private void UpdateProviderOnOnUpdate()
        {
            if (_requests.Count == 0 && _asyncRequests.Count == 0) return;

            if (_frozen)
            {
                Debug.LogWarning("Request queue was frozen");
                return;
            }

            ProcessAsync();
            if(_requests.Count == 0) return;

            var rg = _requests[0];
            ProcessRequest(rg);
            if(rg.Finished)
                _requests.RemoveAt(0);
        }

        private void ProcessRequest(RequestQueueItem rg)
        {
            if (rg.NextAttemptTimestamp > Time.realtimeSinceStartup)
            {
                return;
            }

            TryStartProcessingRequest(rg);

            var timeFromLoadingBegin = Time.realtimeSinceStartup - rg.LoadingBeginTimestamp;

            if (timeFromLoadingBegin > ConnectionTimeout)
            {
                ProcessRequestFail(rg, "Can't process request: Connection timeout. Attemp: " + rg.FailureCount, false);
                return;
            }

            if (rg.Loader.Exception != null)
            {
                ProcessRequestFail(rg, "Can't process request: " + rg.Loader.Exception.Message + " Attemp: " + rg.FailureCount, false);
                return;
            }

            if (rg.Loader.State != HTTPRequestStates.Finished)
            {
                return;
            }

            /*
            if (rg.Loader.Response.Data == null || rg.Loader.Response.Data.Length == 0)
            {
                ProcessRequestFail(rg, "Can't process request: null response. Attemp: " + rg.FailureCount);
                return;
            }*/

            DataCollection container;
            Response response;

            try
            {
                if (rg.Loader.Response.StatusCode != (int)HttpStatusCode.OK)
                {
                    ProcessRequestFail(rg, "Server return wrong response code: " + rg.Loader.Response.StatusCode + " with message: " + rg.Loader.Response.Message, true);
                    return;
                }

                container = MessagePackSerializer.Deserialize<DataCollection>(rg.Loader.Response.Data);
                response = MessagePackSerializer.Deserialize<Response>(container.Response);
            }
            catch (Exception protoException)
            {
                ProcessRequestFail(rg, "Can't process request - broken data: " + protoException.Message + " Attemp: " + rg.FailureCount, true);
                return;
            }

            if (response.ResponseCode == ResponseCode.BAD_SIGNATURE)
            {
                ProcessRequestFail(rg, "Can't process request - bad signature. Attemp: " + rg.FailureCount, true);
                return;
            }

            //Debug.Log("Request took " + (DateTime.UtcNow - rg.SendTimstamp).Value.TotalMilliseconds + " ms " + rg.Method);

            rg.Finished = true;

            if (response.DefsData != null)
            {
                if (_definitionsUpdater.IsDefsNewer(response.DefsData))
                {
                    _definitionsUpdater.SaveDefinitions(response.DefsData);

                    if (_isFirstRequest)
                    {
                        //we can update definitions silently on first request
                        _definitionsUpdater.TryUpdateDefinitions();
                    }
                    else
                    {
                        //restart game needed
                        OnDefUpdate(response.DefsData);
                    }
                }
            }

            if (_isFirstRequest)
            {
                _isFirstRequest = false;
            }

            if (response.ResponseCode == ResponseCode.WRONG_REQUEST_SERIAL_NUMBER)
            {
                const string errorMessage = "Can't process request: wrong request serial number";
                Debug.LogError(errorMessage);
                OnFailed(response, new Exception(errorMessage));
                return;
            }

            if (response.ResponseCode != ResponseCode.OK)
            {
                var errorMessage = "Can't process request - bad server response code: " + response.ResponseCode +
                    " " + response.ServerStackTrace;
                Debug.LogError(errorMessage + (string.IsNullOrEmpty(response.DebugInfo) ? "" : response.DebugInfo));
                OnFailed(response, new Exception(errorMessage));
                return;
            }

            SessionId = response.SessionId;

            //Debug.Log(DateTime.Now + " request completed. Requests in queue: " + _requests.Count);

            if (new Guid(response.PlayerId) != Guid.Empty && !DisableWritePlayerIdFromResponse)
            {
                PlayerId = new Guid(response.PlayerId);
            }

            if (response.ShortId > 0)
            {
                ShortId = response.ShortId;
                Debug.Log("SHORT ID: " + response.ShortId);
            }

            try
            {
                OnComplete(response, container);
                rg.OnComplete(response, container);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Error in processing request '{0}' - exception in OnComplete(...) event, check subscribers code {1}", rg.Method, e);
            }
        }

        private void ProcessAsync()
        {
            var pos = 0;
            while (pos < _asyncRequests.Count)
            {
                ProcessRequest(_asyncRequests[pos]);
                if (_asyncRequests[pos].Finished)
                {
                    _asyncRequests.RemoveAt(pos);
                }
                else
                {
                    ++pos;
                }
            }
        }

        /*
         * critical означает, что запросы можно повторять зачастую бессмысленно - например неправильный ответ с сервера
         * если же например connection timeout - можно пытаться снова и снова (если игра поддерживает работу без интернета)
         */
        private void ProcessRequestFail(RequestQueueItem rg, string message, bool critical)
        {
            if (critical)
            {
                Debug.LogError(message);
            }
            else
            {
                Debug.Log(message);
            }

            rg.FailureCount++;

            if (rg.FailureCount >= MaxAttempsToWarning)
            {
                /*
                 * Если игре разрешено работать в оффлайне, она может игнорировать невозможность подключения к серверу
                 * из-за некритичных ошибок
                 */
                if ((_config.EnableOfflineSharedLogic && critical) || !_config.EnableOfflineSharedLogic)
                {
                    OnConnectionProblem(null, new Exception(message));
                }
                else
                {
                    OnConnectionWarning(null, new Exception(message));
                }
            }

            //Debug.LogWarning(message);
            ScheduleRestartRequest(rg);
        }

        private void TryStartProcessingRequest(RequestQueueItem rg)
        {
            if (rg.SendTimstamp == null)
            {
                rg.SendTimstamp = DateTime.UtcNow;
            }

            if (rg.Loader == null)
            {
                RunRequest(rg);
            }
        }

        private void ScheduleRestartRequest(RequestQueueItem rg)
        {
            rg.NextAttemptTimestamp = Time.realtimeSinceStartup + CalculateDelay(rg.FailureCount);
            if (rg.Loader != null)
            {
                DisposeWWW(rg.Loader);
                rg.Loader = null;
            }
        }

        private void RunRequest(RequestQueueItem rg)
        {
            if (rg.Loader != null)
            {
                DisposeWWW(rg.Loader);
                rg.Loader = null;
            }

            rg.Header.UserId = PlayerId.ToByteArray();
            rg.Header.SessionId = SessionId;

            var requestSerialized = MessagePackSerializer.Serialize(rg.Header);
            var container = new DataCollection()
            {
                Request = requestSerialized,
                Data = rg.Data,
                State = rg.State //TODO: disable in production
            };

            requestSerialized = MessageCoder.AddSignature(MessagePackSerializer.Serialize(container));
            rg.PostData = requestSerialized;

            //var postDataString = string.Join(" ", requestSerialized.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray());
            //Debug.Log("Request: " + postDataString);

            TryStoreCommandOnDisk(requestSerialized);

            rg.Loader = new HTTPRequest(new Uri(_config.DynamicURL + "/" + rg.Method), HTTPMethods.Post);
            rg.Loader.RawData = rg.PostData;
            rg.Loader.Send();

            rg.LoadingBeginTimestamp = Time.realtimeSinceStartup;
        }

        private void DisposeWWW(HTTPRequest www)
        {
            www.Abort();
        }

        public static string RequestsLogPath
        {
            get
            {
                return Application.persistentDataPath + "/requests.bin";
            }
        }
        
        private void TryStoreCommandOnDisk(byte[] postData)
        {
            if (!_config.DebugWriteRequestsOnDisk) return;

            BenchmarkRequest allRequests;
            if (File.Exists(RequestsLogPath))
            {
                var bytes = File.ReadAllBytes(RequestsLogPath);
                allRequests = MessagePackSerializer.Deserialize<BenchmarkRequest>(bytes);
            }
            else
            {
                allRequests = new BenchmarkRequest();
            }

            allRequests.SerializedRequest.Add(postData);
            File.WriteAllBytes(RequestsLogPath, MessagePackSerializer.Serialize(allRequests));
#if UNITY_IOS
            UnityEngine.iOS.Device.SetNoBackupFlag(RequestsLogPath);
#endif
        }
    }
}
