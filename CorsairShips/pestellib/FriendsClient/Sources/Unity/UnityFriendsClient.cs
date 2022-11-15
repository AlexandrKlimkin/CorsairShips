#if UNITY_5_3_OR_NEWER
using System;
using FriendsClient.Lobby;
using FriendsClient.Private;
using PestelLib.ClientConfig;
using PestelLib.ServerClientUtils;
using UnityDI;
using UnityEngine;

namespace FriendsClient.Sources.Unity
{ 
    public class UnityFriendsClient : MonoBehaviour
    {
        public Action onClientCreated;

        [Dependency]
        private RequestQueue _requestQueue;

        [Dependency] private Config _config;

        public bool Enabled { get; private set; }
        public int _initRetries;

        private Private.FriendsClient _friendsClient;

        public IFriendsClient Client {
            get { return _friendsClient; }
        }

        public bool HasActiveRoom
        {
            get { return ActiveRoom != null; }
        }

        public bool IsConnected => Client?.IsConnected ?? false;

        public IFriendsRoom ActiveRoom
        {
            get
            {
                var r = _friendsClient?.Lobby?.Room;
                if (r == null || r.RoomStatus != RoomStatus.Party)
                    return null;
                return r;
            }
        }

        public void Reinit()
        {
            _friendsClient.Stop();
            _friendsClient.Start();
        }

        public bool MuteBattleInvitations
        {
            get
            {
                if (_friendsClient == null || _friendsClient.Lobby == null)
                    return false;
                return _friendsClient.Lobby.MuteBattleInvitations;
            }
            set
            {
                if (_friendsClient == null || _friendsClient.Lobby == null)
                    return;
                _friendsClient.Lobby.MuteBattleInvitations = value;
            }
        }

        void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
            Enabled = !_config.UseLocalState && _config.FriendsServerEnabled;
        }

        void Start()
        {
            if (!_config.UseLocalState && _config.FriendsServerEnabled)
            {
                _friendsClient = new Private.FriendsClient(_requestQueue.PlayerId, "");
                _friendsClient.OnDisconnected += OnDisconnected;
                _friendsClient.OnConnected += OnConnected;
                _friendsClient.OnConnectionError += OnConnectionError;
                _friendsClient.OnInitialized += OnInitialized;
                _friendsClient.Start();

                onClientCreated?.Invoke();
            }
        }

        private void OnInitialized(FriendInitResult friendInitResult)
        {
        }

        private void OnConnected()
        {
        }

        private void OnConnectionError()
        {
            Debug.LogError("Connection error.");
        }

        private void OnDisconnected()
        {
            Debug.LogWarning("Friends client disconnect.");
        }

        void OnDestroy()
        {
            if(_friendsClient == null)
                return;
            _friendsClient.OnDisconnected -= OnDisconnected;
            _friendsClient.OnConnected -= OnConnected;
            _friendsClient.OnConnectionError -= OnConnectionError;
            _friendsClient.OnInitialized -= OnInitialized;
            _friendsClient.Stop();
            _friendsClient.Dispose();
        }
    }
}
#endif