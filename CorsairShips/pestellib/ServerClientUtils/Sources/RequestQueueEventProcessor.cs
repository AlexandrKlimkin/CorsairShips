using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using GoogleSpreadsheet;
using MessagePack;
using Newtonsoft.Json;
using PestelLib.ClientConfig;
using PestelLib.Localization;
using PestelLib.SaveSystem;
using PestelLib.Serialization;
using PestelLib.UI;
using PestelLib.Utils;
using S;
using UnityDI;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PestelLib.ServerClientUtils 
{
    public class RequestQueueEventProcessor : MonoBehaviour
    {
        /*
         * RequestQueueEventProcessor исторически недоступен через DependencyInjection; что бы не менять его интеграцию в каждом проекте,
         * и чтобы избежать кольцевых зависимостей в библиотеках, пришлось сделать тут статик эвент, вызываемый из CommandProcessor
         */
        public static Action<string> OnSharedLogicExceptionEvent = (s) => { };

#pragma warning disable 649
        [Dependency] private RequestQueue _requestQueue;
        [Dependency] private ILocalization _localization;
        [Dependency] private Config _config;
        [Dependency] private Gui _gui;
        [Dependency] private SharedTime _sharedTime;
        [Dependency] private IGameDefinitions _clientDefinitions;
#pragma warning restore 649

        private GenericMessageBoxScreen _connectionProblemMessageBox;        

        private GenericStatusMessage _statusMessage;

        protected bool _criticalError = false;

        public bool CriticalError
        {
            get { return _criticalError; }
        }

        private DateTime? _maintenanceBeginTimestamp;

        public string IosMarketUrl = "https://itunes.apple.com/us/app/id1085101017";
        public string AndroidMarketUrl = "https://play.google.com/store/apps/details?id=org.fie.swordplay";

        //private ServerUpdateScreen _serverUpdateScreen;

        protected virtual void Start()
        {
            ContainerHolder.Container.BuildUp(this);

            if (_config.UseLocalState) return;

            _requestQueue.OnFailed += OnFailedRequest;
            _requestQueue.OnConnectionProblem += OnConnectionProblem;
            _requestQueue.OnComplete += OnConnectionRestored;

            _requestQueue.OnDefUpdate += OnDefsUpdated;

            if (!_config.DontCheckDefinitionsEquality)
            {
                RequestServerDefs();
            }

            OnSharedLogicExceptionEvent += OnSharedLogicException;
        }

        protected virtual void OnSharedLogicException(string msg)
        {
            if (_criticalError) return;

            _criticalError = true;
            _requestQueue.Freeze();
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef
                {
                    Prefab = GenericMessageBoxScreen.DefaultPrefabPlayerIdOverride,
                    AutoHide = false,
                    Caption = _localization.Get("SystemErrorException"),
                    Description = msg
                }
            );
        }

        [Conditional("UNITY_EDITOR")]
        private void RequestServerDefs()
        {
            if (_clientDefinitions == null)
            {
                Debug.LogError("Please register IGameDefinitions in ContainerHolder.Container. Use something like that: container.RegisterCustom<IGameDefinitions>(() => container.Resolve<DefinitionsContainer>().Defs);");
                return;
            }
            _requestQueue.SendRequest("DefsRequest", new Request {DefsRequest = new DefsRequest()}, OnDefsReceived);
        }

        private void OnDefsReceived(Response response, DataCollection dataCollection)
        {
            var defs = MessagePackSerializer.Deserialize<DefsData>(dataCollection.Data);

            var serverDefinitions = (IGameDefinitions)Activator.CreateInstance(_clientDefinitions.GetType());
            DefinitionsUtils.UpdateDefinitions(defs, serverDefinitions);

            var fields = _clientDefinitions.GetType().GetFields().Where(x => x.GetCustomAttributes(typeof(GooglePageRefAttribute), false).Length > 0);

            foreach (var field in fields)
            {
                var clientValue = field.GetValue(_clientDefinitions);
                var serverValue = field.GetValue(serverDefinitions);

                var clientValueSerialized = JsonConvert.SerializeObject(clientValue, Formatting.Indented);
                var serverValueSerialized = JsonConvert.SerializeObject(serverValue, Formatting.Indented);

                if (!string.Equals(clientValueSerialized, serverValueSerialized))
                {
                    Debug.LogErrorFormat("Definitions '{0}' are different on client and server", field.Name);
                    CompareStringsInMergeTool(field.Name, serverValueSerialized, clientValueSerialized);
                }
            }
        }

        private void OnDefsUpdated(DefsData obj)
        {
            if (_criticalError) return;

            _criticalError = true;
            _requestQueue.Freeze();

            ShowDefenitionsUpdatedMessage();
        }

        private void Update()
        {
            if (TimeToMaintenance.HasValue)
            {
                TryShowStatusMessage();

                if (TimeToMaintenance.HasValue && TimeToMaintenance.Value.TotalSeconds < 0)
                {
                    TryShowMaintenanceMessagebox();
                    HideStatusMessage();
                    return;
                }
                else
                {
                    UpdateStatusMessageText(string.Format(_localization.Get("SystemMaintenanceBeginsIn"), FormatTime.Format(TimeToMaintenance.Value)));
                }
            }
        }

        private TimeSpan? TimeToMaintenance
        {
            get
            {
                if (_maintenanceBeginTimestamp.HasValue)
                {
                    return _maintenanceBeginTimestamp.Value - _sharedTime.Now;
                }

                return null;
            }
        }

        private void OnConnectionRestored(Response arg1, DataCollection arg2)
        {
            if (arg1.MaintenanceTimestamp > float.Epsilon)
            {
                _maintenanceBeginTimestamp = new DateTime(arg1.MaintenanceTimestamp);
                
                if (TimeToMaintenance.HasValue && TimeToMaintenance.Value.TotalSeconds > 0)
                {
                    Debug.Log("Maintenance begin in " + TimeToMaintenance.Value);
                }
                else
                {
                    TryShowMaintenanceMessagebox();
                }
            }

            CloseConnectionProblemMessage();
        }

        private void OnConnectionProblem(Response response, Exception exception)
        {
            if (_connectionProblemMessageBox != null) return;

            ShowConnectionProblem();
        }

        private void OnDestroy()
        {
            if (_requestQueue != null)
            {
                _requestQueue.OnFailed -= OnFailedRequest;
                _requestQueue.OnConnectionProblem -= OnConnectionProblem;
                _requestQueue.OnComplete -= OnConnectionRestored;
                _requestQueue.OnDefUpdate -= OnDefsUpdated;
            }
            OnSharedLogicExceptionEvent -= OnSharedLogicException;
        }

        private static void CompareStringsInMergeTool(string prefix, string server, string client)
        {
#if UNITY_EDITOR
            try
            {
                var serverStatePath = Application.dataPath.Replace("/Assets", "/Temp") + "/" + prefix + ".server.json";
                System.IO.File.WriteAllText(serverStatePath, server, Encoding.UTF8);

                var clientStatePath = Application.dataPath.Replace("/Assets", "/Temp") + "/" + prefix + ".client.json";
                System.IO.File.WriteAllText(clientStatePath, client, Encoding.UTF8);

                var diffTool =
                Application.dataPath.Replace("/Assets", "/") + "pestellib/Tools/WinMerge/WinMergePortable.exe";

                if (System.IO.File.Exists(diffTool))
                {
                    System.Diagnostics.Process.Start(diffTool, serverStatePath + " " + clientStatePath);
                }
                else
                {
                    UnityEngine.Debug.LogError("Can't find diff tool: " + diffTool);
                }
            }
#pragma warning disable 168
            catch (Exception e)
#pragma warning restore 168
            {
                Debug.Log("Please install WinMerge (http://winmerge.org/) to see hash mismatch details");
            }
#endif
        }

        private void OnFailedRequest(Response response, Exception exception)
        {
            Debug.LogWarning("TaskInitRequestQueue: " + JsonConvert.SerializeObject(response) + " " +
                             JsonConvert.SerializeObject(exception));

            if (_criticalError) return;

            _criticalError = true;

            _requestQueue.Freeze();
            
            if (response == null)
            {
                ShowNoResponse(exception);
                return;
            }

            switch (response.ResponseCode)
            {
                case ResponseCode.WRONG_CLIENT_VERSION:
                    ShowOldVersionDialog();
                    break;
                case ResponseCode.HASH_CHECK_FAILED:
                    var info = JsonConvert.DeserializeObject<HashMismatchInfo>(response.DebugInfo);
                    Debug.Log("Server state: " + info.Server);
                    Debug.Log("Client state: " + info.Client);
                    CompareStringsInMergeTool("UserProfile", info.Server, info.Client);
                    HashCheckMismatch(response.DebugInfo);
                    break;
                case ResponseCode.SERVER_MAINTENANCE:
                    ShowMaintenanceMessagebox();
                    break;
                case ResponseCode.LOST_COMMANDS:
                case ResponseCode.WRONG_SESSION:
                    ShowWrongSessionMessagebox(response.ResponseCode.ToString());
                    break;
                case ResponseCode.IVALID_RECEIPT:
                    ShowIvalidReceiptMessagebox(exception.Message);
                    break;
                case ResponseCode.BANNED:
                    ShowBannedMessage(response.DebugInfo);
                    break;
                default:
                    ShowDefaultCriticalServerError(exception.Message);
                    break;
            }
        }

        [Obsolete("Please override ShowWrongSessionMessagebox(string code)")]
        protected virtual void ShowWrongSessionMessagebox()
        {
            ShowWrongSessionMessagebox(string.Empty);
        }

        protected virtual void ShowWrongSessionMessagebox(string code)
        {
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef
            {
                AutoHide = false,
                Prefab = GenericMessageBoxScreen.DefaultPrefabPlayerIdOverride,
                Caption = _localization.Get("SystemServerErrorProcessingRequest"),
                Description = _localization.Get("SystemServerErrorWrongSession") + string.Format(" ({0})", code),
                CantClose = true
            });
        }

        protected virtual void ShowIvalidReceiptMessagebox(string message)
        {
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef
            {
                AutoHide = false,
                Prefab = GenericMessageBoxScreen.DefaultPrefabPlayerIdOverride,
                Caption = _localization.Get("SystemServerErrorProcessingRequest"),
                Description = message,
                CantClose = true
            });
        }

        protected virtual void ShowBannedMessage(string reason)
        {
            string desc;
            if(!string.IsNullOrEmpty(reason))
                desc = _localization.Get("SystemUserBannedDesc");
            else
                desc = _localization.Get("SystemUserBannedNoReasonDesc");
            desc += "\nID=" + _requestQueue.PlayerId + ".";
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef
            {
                AutoHide = false,
                Prefab = GenericMessageBoxScreen.DefaultPrefabPlayerIdOverride,
                Caption = _localization.Get("SystemUserBannedCaption"),
                Description = string.Format(desc, reason),
                CantClose = true
            });
        }

        private void TryShowMaintenanceMessagebox()
        {
            if (_criticalError) return;

            _criticalError = true;
            _requestQueue.Freeze();

            ShowMaintenanceMessagebox();
        }

        public void ShowOldVersionDialog()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                ShowOldVersionIos();
            }
            else
            {
                ShowOldVersionAndroid();
            }
        }


        protected virtual void ShowDefaultCriticalServerError(string message)
        {
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef
            {
                AutoHide = false,
                Prefab = GenericMessageBoxScreen.DefaultPrefabPlayerIdOverride,
                Caption = _localization.Get("SystemServerErrorProcessingRequest"),
                Description = message
            });
        }

        protected virtual void ShowMaintenanceMessagebox()
        {
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef
            {
                Prefab = GenericMessageBoxScreen.DefaultPrefabPlayerIdOverride,
                AutoHide = false,
                Caption = _localization.Get("SystemServerMaintenanceCaption"),
                Description = _localization.Get("SystemServerMaintenanceDescription"),
                CantClose = true
            });
        }

        protected virtual void ShowNoResponse(Exception exception)
        {
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef
            {
                Prefab = GenericMessageBoxScreen.DefaultPrefabPlayerIdOverride,
                AutoHide = false,
                Caption = _localization.Get("SystemErrorNoServerResponse"),
                Description = exception.Message
            });
        }

        protected virtual void HashCheckMismatch(string debugInfo)
        {
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef
            {
                Prefab = GenericMessageBoxScreen.DefaultPrefabPlayerIdOverride,
                AutoHide = false,
                Caption = _localization.Get("SystemErrorHashMismatch"),
                Description = _localization.Get("SystemErrorHashMismatchDescription"),
                CantClose = true
            });
        }

        protected virtual void ShowOldVersionAndroid()
        {
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef()
            {
                Prefab = GenericMessageBoxScreen.DefaultPrefabPlayerIdOverride,
                AutoHide = false,
                Caption = _localization.Get("SystemErrorWrongVersionCaption"),
                Description = _localization.Get("SystemErrorWrongVersionAndroid"),
                ButtonALabel = _localization.Get("SystemErrorWrongVersionUpdate"),
                ButtonAAction = () => Application.OpenURL(PlatformMarketUri),
                ShowIdentity = true,
                CantClose = true
            });
        }

        protected virtual void ShowOldVersionIos()
        {
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef()
            {
                Prefab = GenericMessageBoxScreen.DefaultPrefabPlayerIdOverride,
                AutoHide = false,
                Caption = _localization.Get("SystemErrorWrongVersionCaption"),
                Description = _localization.Get("SystemErrorWrongVersionIos"),
                ButtonALabel = _localization.Get("SystemErrorWrongVersionUpdate"),
                ButtonAAction = () => Application.OpenURL(PlatformMarketUri),
                ShowIdentity = true,
                CantClose = true
            });
        }

        protected virtual void ShowConnectionProblem()
        {
            _connectionProblemMessageBox = GenericMessageBoxScreen.Show(new GenericMessageBoxDef
            {
                Caption = _localization.Get("SystemConnectionProblemCaption"),
                Description = _localization.Get("SystemConnectionProblemDescription"),
                Prefab = GenericMessageBoxScreen.DefaultPrefabPlayerIdOverride,
                AutoHide = false,
                ButtonAAction = () =>
                {
                    _requestQueue.TryToConnectImmediately();
                    _gui.Close(_connectionProblemMessageBox.gameObject); 
                },
                ButtonALabel = _localization.Get("SystemConnectionProblemRetry")
            });
        }

        protected virtual void ShowDefenitionsUpdatedMessage()
        {
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef()
            {
                Caption =  _localization.Get("SystemDefinitionsUpdateCaption"), //"Definitions update",
                Description = _localization.Get("SystemDefinitionsUpdateDescription") //"Game definitions were updated, please restart game"
            });
        }

        protected virtual void CloseConnectionProblemMessage()
        {
            if (_connectionProblemMessageBox != null)
            {
                _gui.Close(_connectionProblemMessageBox.gameObject);
                _connectionProblemMessageBox = null;
            }
        }

        protected virtual void TryShowStatusMessage()
        {
            if (_statusMessage == null)
            {
                _statusMessage = _gui.Show<GenericStatusMessage>(GuiScreenType.ConnectionStatusOverlay);
            }
        }

        protected virtual void HideStatusMessage()
        {
            _gui.Close(_statusMessage.gameObject);
        }

        protected virtual void UpdateStatusMessageText(string message)
        {
            _statusMessage.Message = message;
        }

        [Obsolete("Use public PlatformMarketUri")]
        protected virtual string MarketUri
        {
            get
            {
                return PlatformMarketUri;
            }
        }

        public virtual string PlatformMarketUri
        {
            get
            {
                return (Application.platform == RuntimePlatform.IPhonePlayer) ? IosMarketUrl : AndroidMarketUrl;
            }
        }
    }
}