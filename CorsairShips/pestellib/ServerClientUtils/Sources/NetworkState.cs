using System;
using System.Collections;
using PestelLib.ClientConfig;
using S;
using UnityDI;
using UnityEngine;

namespace PestelLib.ServerClientUtils
{
    public class NetworkState : MonoBehaviour
    {
        public Action OnTimeToReplaceStateOnServer = () => {};
        public Action OnTimeToSaveStateOnServer = () => { };

        public Action OnOffline = () => { };
#pragma warning disable 649
        [Dependency] private Config _config;
        [Dependency] private RequestQueue _requestQueue;
#pragma warning restore 649

        private bool _requestQueueOK = false;

        private bool isEditor = false;

        float _waitTime = 2f;
        float _currTime = 0;

        public bool OfflineMode {
            get {
                return
              _config.UseLocalState
              || NetworkReachability == NetworkReachability.NotReachable
              || !_requestQueueOK;
            }
        }

        public NetworkReachability NetworkReachability {
            get {
                if (_NetworkReachabilityOutdated) {
                    _NetworkReachability = GetNetworkReachability();
                    _NetworkReachabilityOutdated = false;
                }
                return _NetworkReachability;
            }
        }
        private bool _NetworkReachabilityOutdated;
        private NetworkReachability _NetworkReachability;

        private void Start()
        {
            ContainerHolder.Container.BuildUp(this);

            _requestQueue.OnConnectionProblem += OnConnectionProblem;
            _requestQueue.OnConnectionWarning += OnConnectionProblem;
            _requestQueue.OnComplete += OnConnectionRestored;

            isEditor = Application.isEditor;
        }

        void Update()
        {
            _currTime += Time.deltaTime;
            if (_currTime >= _waitTime)
            {
                if (_requestQueueOK && NetworkReachability == NetworkReachability.NotReachable)
                {
                    OnConnectionProblem(null, null);
                }
                _currTime = 0;
            }
        }

        private void LateUpdate() {
            _NetworkReachabilityOutdated = true;
        }

        private void OnDestroy()
        {
            _requestQueue.OnConnectionProblem -= OnConnectionProblem;
            _requestQueue.OnConnectionWarning -= OnConnectionProblem;
            _requestQueue.OnComplete-= OnConnectionRestored;
        }

        private void OnConnectionProblem(Response response, Exception exception)
        {
            if (!_requestQueueOK) return;

            if (isEditor)
                Debug.Log("OnConnectionProblem");

            _requestQueueOK = false;
            
            OnOffline();
        }

        private void OnConnectionRestored(Response r, DataCollection data)
        {
            if (_requestQueueOK) return;

            if (isEditor)
                Debug.Log("OnConnectionRestored");

            _requestQueueOK = true;

            OnTimeToReplaceStateOnServer();
            /*
            _requestQueue.SendRequest("ReplaceStateRequest", new Request { ReplaceStateRequest = new ReplaceStateRequest
            {
                State = ContainerHolder.Container.Resolve<ISharedLogic>().SerializedState
            }}, 
            (response, collection) => { }
            );
            */
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (OfflineMode) return;
            
            if (!pauseStatus && _requestQueue != null)
            {
                OnTimeToSaveStateOnServer();

                /*
                _requestQueue.SendRequest("ReplaceStateRequest", new Request
                {
                    ReplaceStateRequest = new ReplaceStateRequest
                    {
                        State = sharedLogic.SerializedState
                    }
                },
                (response, collection) => { }
                );*/
            }
        }

        private NetworkReachability GetNetworkReachability() {
            return Application.internetReachability;
        }
    }
}
