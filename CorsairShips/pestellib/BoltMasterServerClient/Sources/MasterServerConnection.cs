using System;
using System.Collections;
using BoltTransport;
using MasterServerProtocol;
using UnityEngine;
using Newtonsoft.Json;
using UnityDI;
using UnityEngine;

namespace BoltGameServerToMasterServerConnector
{
    public class MasterServerConnection : MonoBehaviour
    {
        [Dependency] protected GameServerParameters _gameServerParameters;

        public string IPAddress = "192.168.88.243";
        public int Port = 7777;
        private Guid _gameServerId;

        private object _reportLock = new object();
        private GameServerStateReport _report;
        private bool _serverRemovedFromMaster;

        private Client<MasterServerConnectionImplementation> _client;

        protected virtual void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
            DontDestroyOnLoad(gameObject);
        }

        protected virtual IEnumerator Start()
        {
            Debug.Log("MasterServerConnection: Start waiting for parameters");
            while (!_gameServerParameters.GameServerId.HasValue || 
                   string.IsNullOrEmpty(_gameServerParameters.MasterIpAddress) ||
                   !_gameServerParameters.MasterPort.HasValue)
            {
                yield return null;
            }
            Debug.Log("MasterServerConnection: parameters received");

            _gameServerId = _gameServerParameters.GameServerId.Value;
            IPAddress = _gameServerParameters.MasterIpAddress;
            Port = _gameServerParameters.MasterPort.Value;

#if !UNITY_SERVER
            yield break;
#endif
            var connection = new MasterServerConnectionImplementation(_gameServerParameters);
            _client = new Client<MasterServerConnectionImplementation>(connection, IPAddress, Port);
            UnityEngine.Debug.Log("Create connection");
            
            while (true)
            {
                if (_serverRemovedFromMaster) yield break;

                if (_report != null)
                {
                    GameServerStateReport report;
                    lock (_reportLock)
                    {
                        report = Clone(_report);
                    }

                    report.GameServerId = _gameServerId;
                    _client.SendMessage(report);
                }

                yield return new WaitForSecondsRealtime(3);
            }
        }

        protected virtual void OnDestroy()
        {
            _client?.Dispose();
        }

        public void RemoveGameServerFromMaster()
        {
            _serverRemovedFromMaster = true;

            if (_client == null)
            {
                Debug.LogWarning("Can't remove game server from master - I don't have connection to master.");
                return;
            }

            _client.SendMessage(new RemoveGameServerFromMaster()
            {
                GameServerId = _gameServerId
            });
        }

        public void SetReport(GameServerStateReport report)
        {
            lock (_reportLock)
            {
                _report = report;
            }
        }

        void Update()
        {
            CheckAndProcessExit();
        }

        protected virtual void CheckAndProcessExit()
        {
            if (MasterServerConnectionImplementation.TimeToLeave)
            {
                Application.Quit();
            }
        }

        public static T Clone<T>(T source)
        {
            var json = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
