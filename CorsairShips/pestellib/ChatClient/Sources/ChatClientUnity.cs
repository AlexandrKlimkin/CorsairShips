using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PestelLib.ChatCommon;
using PestelLib.NetworkUtils;
using UnityEngine;
using UnityDI;
using log4net;

namespace PestelLib.ChatClient
{
    [RequireComponent(typeof(InternetReachabilityInit))]
    public class ChatClientUnity : MonoBehaviour
    {
        private static ILog Log = LogManager.GetLogger(typeof(ChatClientUnity));

        private IInternetReachability _internetReachability;
#pragma warning disable 649, 169
        [Dependency]
        private IBadWordFilter _badWordFilter;

        private readonly ChatClient _chat = new ChatClient();
        private List<Action> _actions = new List<Action>();
        [SerializeField] private bool _dontDisconnectOnPause;
        [SerializeField] private bool _updateChannelList;
        [SerializeField] private bool _historyAsEvents;
        [SerializeField] private string _serverKey;
        [SerializeField] private bool _sendPlayerId;
#pragma warning restore 649, 169

#pragma warning disable 618
        public ClientInfo[] ChannelUsers
        {
            get { return _chat.ChannelUsers; }
        }
#pragma warning restore 618
        public ChatClient.STATE State
        {
            get { return _chat.State; }
        }
        [Obsolete("Use MainChannel")]
        public string CurrentChannel
        {
            get { return _chat.CurrentChannel; }
        }
        public bool IsConnected => _chat.IsConnected;

        public ClientInfo ClientInfo
        {
            get { return _chat.ClientInfo; }
        }

        public bool UpdateChannelList
        {
            get { return _chat.UpdateChannelList; }
            set { _chat.UpdateChannelList = value; }
        }

        public string MainChannel
        {
            get { return _chat.MainChannel; }
        }

        public string[] ConnectedChannels
        {
            get { return _chat.ExtraChannels.Concat(new [] {MainChannel}).ToArray(); }
        }

        public ClientInfo[] GetChannelUsers(string channel)
        {
            return _chat.GetChannelUsers(channel);
        }

        public event Action OnConnected = () => { };
        public event Action<string> OnJoinChannel = (s) => { };
        public event Action<string> OnLeaveChannel = (s) => { };
        public event Action OnDisconnected = () => { };
        public event Action<ChatMessage> OnReceiveChannelMessage = message => { };
        public event Action<ChatMessage> OnReceivePrivateMessage = message => { };
        public event Action<string, ClientInfo[]> OnChannelUsersListUpdate = (channel,infos) => { };
        public event Action<ClientInfo, ChatMessage[]> OnReceivePrivateMessageHistory = (c, h) => { };
        public event Action<string, ChatMessage[]> OnChannelHistory = (c, h) => { };
        public event Action<float> OnAutoBan = (time) => { };
        public event Action<ClientInfo, BanReason, int> OnBanGranted = (info, reason, time) => { };
        public event Action<ClientInfo, BanReason, int, string> OnBanGrantedOnChannel = (info, reason, time, ch) => { };
        public event Action<string, BanReason> OnBanWarning = (channel,r) => { }; 

        [Obsolete("Use OnReceiveChannelMessage")]
        public event Action<string, string> OnReceiveMessage = (fromUser, message) => { };
        [Obsolete("Use OnChannelUsersListUpdate")]
        public event Action<ClientInfo[]> OnChannelUsersUpdate = infos => { };

        public void Init(string playerName, string chatServerAddr, int chatServerPort, bool secure = false, Guid playerId = default)
        {
            if (_sendPlayerId)
                _chat.PlayerIdToSend = playerId;

            _chat.Init(playerName, chatServerAddr, chatServerPort, _internetReachability, secure);
        }

        public void RenamePlayer(string newName)
        {
            _chat.RenamePlayer(newName);
        }

        public void Connect(string channel, byte[] id)
        {
            _chat.Connect(channel, id);
        }

        public void Connect(string channel, Guid id)
        {
            Connect(channel, id.ToByteArray());
        }

        public void RequestPrivateMessageHistory()
        {
            _chat.RequestPrivateMessageHistory();
        }

        public void RequestChannelMessageHistory(string channel)
        {
            _chat.RequestChannelMessageHistory(channel);
        }

        public void JoinChannel(string channel)
        {
            _chat.JoinChannel(channel);
        }

        public void LeaveChannel(string channel)
        {
            _chat.LeaveChannel(channel);
        }

        private void Filter(ref string message, string channel)
        {
            if (_badWordFilter == null)
                return;

            var report = _badWordFilter.Filter(ref message, Application.systemLanguage);
            var reportForEn = _badWordFilter.Filter(ref message, SystemLanguage.English);

            foreach (var kv in report)
            {
                if(kv.Value < 1)
                    continue;
                if (!_chat.IsBanned)
                {
                    OnBanWarning(channel, kv.Key);
                    _chat.BadWordNotify(channel, kv.Key);
                }
                
            }

            foreach (var kv in reportForEn)
            {
                if(kv.Value < 1)
                    continue;
                if(!_chat.IsBanned)
                {
                    OnBanWarning(channel, kv.Key);
                    _chat.BadWordNotify(channel, kv.Key);
                }
            }
        }

        [Obsolete("Use SendChatMessage(string,string,Action<bool>) to explicitly define channel")]
        public void SendChatMessage(string message, Action<bool> sendCallback = null, byte[] messageMetadata = null)
        {
            Filter(ref message, _chat.MainChannel);
            _chat.SendChatMessage(_chat.MainChannel, message, sendCallback, messageMetadata);
        }

        public void SendChatMessageTo(ClientInfo sendTo, string message, Action<bool> sendCallback = null, byte[] messageMetadata = null)
        {
            _chat.SendChatMessageTo(sendTo, message, sendCallback, messageMetadata);
        }

        public void SendChatMessage(string channel, string message, Action<bool> sendCallback = null, byte[] messageMetadata = null)
        {
            Filter(ref message, channel);
            _chat.SendChatMessage(channel, message, sendCallback, messageMetadata);
        }

        public void Disconnect()
        {
            _chat.Disconnect();
        }

        private void ScheduleOnMainThread(Action action)
        {
            lock (_actions)
            {
                _actions.Add(action);
            }
        }


        #region MonoBehaviour

        public bool Inited { get; private set; }
        public Action OnInit= () => { };
        IEnumerator Start()
        {
            while (_internetReachability == null)
            {
                _internetReachability = ContainerHolder.Container.Resolve<IInternetReachability>();
                yield return null;
            }
#pragma warning disable 612, 618
            ContainerHolder.Container.BuildUp(this);
            _chat.UpdateChannelList = _updateChannelList;
            _chat.HistoryAsEvents = _historyAsEvents;
            _chat.OnChannelUsersUpdate += infos => ScheduleOnMainThread(() => OnChannelUsersUpdate(infos));
            _chat.OnChannelUsersListUpdate += (channel, infos) => ScheduleOnMainThread(() => OnChannelUsersListUpdate(channel, infos));
            _chat.OnConnected += () => ScheduleOnMainThread(OnConnected);
            _chat.OnDisconnected += () => ScheduleOnMainThread(OnDisconnected);
            _chat.OnJoinChannel += s => ScheduleOnMainThread(() => OnJoinChannel(s));
            _chat.OnLeaveChannel += s => ScheduleOnMainThread(() => OnLeaveChannel(s));
            _chat.OnReceiveMessage += (user,msg) => ScheduleOnMainThread(() => {OnReceiveMessage(user,msg);});
            _chat.OnReceiveChannelMessage += message => ScheduleOnMainThread(() => { OnReceiveChannelMessage(message); });
            _chat.OnReceivePrivateMessage += message => ScheduleOnMainThread(() => { OnReceivePrivateMessage(message); });
            _chat.OnChannelHistory += (s, protocols) => ScheduleOnMainThread(() => { OnChannelHistory(s, protocols); });
            _chat.OnReceivePrivateMessageHistory += (info, protocols) => ScheduleOnMainThread(() => { OnReceivePrivateMessageHistory(info, protocols); });
            _chat.OnAutoBan += f => ScheduleOnMainThread(() => OnAutoBan(f));
            _chat.OnBanGranted += (info, reason, time) => ScheduleOnMainThread(() => { OnBanGranted(info, reason, time);});
            _chat.OnBanGrantedOnChannel += (info, reason, arg3, arg4) => ScheduleOnMainThread(() => { OnBanGrantedOnChannel(info, reason, arg3, arg4); });
#pragma warning restore 612, 618
            Inited = true;
            OnInit();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if(hasFocus && State == ChatClient.STATE.DISCONNECTED)
                _chat.Connect();
        }

        void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (!_dontDisconnectOnPause && State == ChatClient.STATE.CONNECTED)
                {
                    Disconnect();
                }
            }
            else
            {
                if (State == ChatClient.STATE.DISCONNECTED)
                    _chat.Connect();
            }
        }

        void OnDestroy()
        {
            if (State == ChatClient.STATE.CONNECTED)
            {
                Disconnect();
            }
            _chat.Close();
        }

        private void Update()
        {
            _chat.Update();

            if (_actions.Count > 0)
            {
                lock (_actions)
                {
                    for (var i = 0; i < _actions.Count; ++i)
                    {
                        try
                        {
                            _actions[i]();
                        }
                        catch (Exception e)
                        {
                            Log.Error("Exec action failed", e);
                        }
                    }
                    _actions.Clear();
                }
            }

            // if (Input.GetKeyDown(KeyCode.Keypad1))
            // {
            //     _dbgPause = !_dbgPause;
            //     OnApplicationPause(_dbgPause);
            // }
        }

#endregion



    }

}
