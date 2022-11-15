using System.IO;
using MessagePack;
using PestelLib.ClientConfig;
using PestelLib.ServerShared;
using PestelLib.Utils;
using S;
using UnityDI;
using UnityEngine;

namespace PestelLib.ServerClientUtils
{
    public class RequestQueueBenchmark : MonoBehaviour
    {
#pragma warning disable 649
        [Dependency] private RequestQueue _requestQueue;

        private int _totalRequestCount;
        private BenchmarkRequest _allRequests;
#pragma warning restore 649

        [SerializeField] private int _passes = 10;

        private int _currentPass;
        private float _startTimestamp;

        void Start()
        {
            InitTestContainer();

            ContainerHolder.Container.BuildUp(this);
            
            _startTimestamp = Time.realtimeSinceStartup;

            RunNewTest();
        }

        private void RunNewTest()
        {
            PlayerPrefs.SetInt(PlayerPrefKeys.PlayerIdKey, -1);

            _requestQueue.SendRequest("InitRequest",
                new Request {InitRequest = new InitRequest()},
                OnRequestComplete
            );
        }

        public static string RequestsLogPath
        {
            get
            {
                string path = Application.persistentDataPath + "/requests.bin";
#if UNITY_IPHONE
            UnityEngine.iOS.Device.SetNoBackupFlag(path);
#endif
                return path;
            }
        }

        private void OnRequestComplete(Response r, DataCollection container)
        {
            _totalRequestCount += _allRequests.SerializedRequest.Count;

            for (var i = 0; i < _allRequests.SerializedRequest.Count; i++)
            {
                var requestBytes = _allRequests.SerializedRequest[i];
                var requestDataContainer = MessagePackSerializer.Deserialize<DataCollection>(MessageCoder.GetData(requestBytes));
                var header = MessagePackSerializer.Deserialize<Request>(requestDataContainer.Request);

                header.UserId = r.PlayerId;

                var dataCollectionUpdated = new DataCollection
                {
                    Data = requestDataContainer.Data,
                    State = requestDataContainer.State,
                    Request = MessagePackSerializer.Serialize(header)
                };

                var postDataUnsigned = MessagePackSerializer.Serialize(dataCollectionUpdated);
                var postDataSigned = MessageCoder.AddSignature(postDataUnsigned);

                bool lastRequest = (i == (_allRequests.SerializedRequest.Count - 1));

                _requestQueue.SendRequestSerialized(postDataSigned, (response, collection) =>
                {
                    if (response.ResponseCode != ResponseCode.OK)
                    {
                        Debug.LogWarning(response.ResponseCode);
                    }

                    if (lastRequest)
                    {
                        _currentPass++;

                        if (_currentPass < _passes)
                        {
                            RunNewTest();
                        }
                        else
                        {
                            Debug.Log("Total requests: " + _totalRequestCount);
                            Debug.Log("Spent time: " + (Time.realtimeSinceStartup - _startTimestamp));
                        }
                    }
                });
            }
        }

        private static void InitTestContainer()
        {
            var container = new Container();
            ContainerHolder.Container = container;
            // container.RegisterSingleton<Config>();
            container.RegisterSingleton<RequestQueue>();
            container.RegisterUnitySingleton<UpdateProvider>();
        }
    }
}
