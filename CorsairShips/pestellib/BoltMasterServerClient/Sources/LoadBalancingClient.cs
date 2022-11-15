using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using BoltGameServerToMasterServerConnector;
using BoltTransport;
using MasterServerProtocol;
using Newtonsoft.Json;
using PestelLib.ClientConfig;
using UnityDI;
using UnityEngine;
using Message = MasterServerProtocol.Message;

namespace BoltMasterServerClient
{
    public class LoadBalancingClient : MonoBehaviour
    {
        public event Action<Exception> OnConnectionFailed = (e) => { };

        [Dependency] private Config _config;
        private ConcurrentQueue<Message> _messages = new ConcurrentQueue<Message>();

        [SerializeField] private int _port = 9998;
        [SerializeField] private int _maxPing = 150;

        private Client<LoadBalancingConnectionImplementation> _client;
        private Action<Message> _onResponse;
        private RequestServer _requestServer;

        private volatile Exception _connectionException;

        Dictionary<string, int> IpToPing = new Dictionary<string, int>();

        void Start()
        {
            ContainerHolder.Container.BuildUp(this);

            foreach (var ip in _config.LoadBalancingIP)
            {
	            StartCoroutine(PingServer(ip));
            }
        }

        IEnumerator PingServer(string ip)
        {
	        IpToPing[ip] = int.MaxValue;

	        var attemps = 3;
	        int result = 0;
	        for (var i = 0; i < attemps; i++)
	        {
		        var ping = new Ping(ip);

		        while (!ping.isDone)
		        {
			        yield return null;
		        }

		        result += ping.time;
	        }

	        IpToPing[ip] = result / attemps;

            Debug.LogFormat("Resulting ping to {0} is {1} ms", ip, IpToPing[ip]);
        }

        void OnDestroy()
        {
            CleanUpClient();
        }

        private void CleanUpClient()
        {
            if (_client != null)
            {
                _client.OnConnectionFailed -= ConnectionFailed;
                _client.Dispose();
                _client = null;
            }
        }

        private void Update()
        {
            while (_messages.TryDequeue(out var message))
            {
                Debug.Log(JsonConvert.SerializeObject(message));
                if (message is RequestServerResponse response)
                {
                    _onResponse(response);
                }
                else if (message is Error error)
                {
                    AbortConnection(new Exception
                    {
                        Source = error.Code.ToString()
                    });
                    StartCoroutine(ResendRequest());
                }
            }

            if (_connectionException != null)
            {
                AbortConnection(_connectionException);
                _connectionException = null;
            }
        }

        private void AbortConnection(Exception e)
        {
            Debug.LogError("Something went really wrong: " + e.Source);

            CleanUpClient();

            OnConnectionFailed(e);
        }

        private void ConnectionFailed(Exception e)
        {
            _connectionException = e;
        }

        public bool IsConnectionGood => FastestServer.Value < _maxPing;

        private KeyValuePair<string, int> FastestServer
        {
	        get
	        {
		        return IpToPing.OrderBy(x => x.Value).FirstOrDefault();
	        }
        }

        public void RequestServer(Action<Message> onResponse, RequestServer requestServer)
        {
	        var loadbalancingIp = FastestServer.Key;

            _requestServer = requestServer;

            _onResponse = onResponse;

            var connection = new LoadBalancingConnectionImplementation(_messages);
            _client = new Client<LoadBalancingConnectionImplementation>(
                connection,
                () => new NetworkTcpClient(loadbalancingIp, _port),
                true);

            _client.OnConnectionFailed += ConnectionFailed;

            SendRequestServer();
        }

        private void SendRequestServer()
        {
            _client.SendMessage(_requestServer);
        }

        private IEnumerator ResendRequest()
        {
            yield return new WaitForSeconds(1);
            SendRequestServer();
        }
    }
}
