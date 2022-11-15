using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ChatClient.Transport;
using PestelLib.ChatCommon;
using PestelLib.NetworkUtils;
using Newtonsoft.Json;
using log4net;

namespace PestelLib.ChatClient
{
    public class ChatClient : IDisposable
    {
        private const int MAX_JOBS = 10;

        public enum STATE
        {
            NONE,
            CONNECTING,
            CONNECTED,
            DISCONNECTED
        }

        private static ILog Log = LogManager.GetLogger(typeof(ChatClient));
        private IInternetReachability _internetReachability;
        private IChatClientTransport _client;
        private STATE _state = STATE.NONE;
        private string _channel;
        private List<string> _extra_channels = new List<string>();
        private byte[] _auth_data;
        private string _serverAddr;
        private int _serverPort = Consts.ChatPort;
        private string _playerName;
        private List<ChatJob> _currentJobs = new List<ChatJob>();
        private List<ChatJob> _todoJobs = new List<ChatJob>();
        private List<ChatProtocol> _incomingMessages = new List<ChatProtocol>();
        private object _incomingMessagesLocker = new object();
        private DateTime _disconnectTime = DateTime.MinValue;
        private Dictionary<string, ClientInfo[]> _channelUsers = new Dictionary<string, ClientInfo[]>();
        private volatile ClientInfo _clientInfo;
        private Dictionary<string, DateTime> _lastMsg = new Dictionary<string, DateTime>();
        private int _serverVersion;
        private DateTime _banExpiry;
        private int _nextTag;
        private Dictionary<int, Action<ChatProtocol>> _tagMap = new Dictionary<int, Action<ChatProtocol>>();
        private ManualResetEventSlim _connectAwait = new ManualResetEventSlim(false);
        private CancellationTokenSource _destroyCancelation = new CancellationTokenSource();
        private bool _secure;
        private class RequestData
        {
            public ChatProtocol Message;
            public Action<bool> SendCallback;
            public Action<ChatProtocol> AnswerCallback;
        }
        private List<RequestData> _pendingRequests = new List<RequestData>();
        private bool _disposed;

        public bool UpdateChannelList { get; set; }
        public bool HistoryAsEvents { get; set; }
        public Guid PlayerIdToSend { get; set; }

        public bool IsBanned {
            get { return DateTime.UtcNow < _banExpiry; }
        }

        [Obsolete("Use GetChannelUsers")]
        public ClientInfo[] ChannelUsers {
            get { return GetChannelUsers(_channel); }
        }

        public ClientInfo[] GetChannelUsers(string channel)
        {
            ClientInfo[] cleints;
            if (_channelUsers.TryGetValue(channel, out cleints))
                return cleints;
            return null;
        }

        public STATE State
        {
            get { return _state; }
        }

        [Obsolete("Use MainChannel")]
        public string CurrentChannel
        {
            get { return _channel; }
        }

        public string MainChannel
        {
            get { return _channel; }
        }

        public string[] ExtraChannels
        {
            get { return _extra_channels.ToArray(); }
        }

        public ClientInfo ClientInfo
        {
            get { return _clientInfo; }
        }

        public bool LeftMainChannel { get; private set; }

        public event Action OnConnected = () => { };
        public event Action<string> OnJoinChannel = (s) => { };
        public event Action<string> OnLeaveChannel = (s) => { };
        public event Action OnDisconnected = () => { };
        [Obsolete("Use OnReceiveChannelMessage")]
        public event Action<string, string> OnReceiveMessage = (fromUser, message) => { };
        public event Action<ChatMessage> OnReceiveChannelMessage = (m) => { };
        [Obsolete("Use OnChannelUsersListUpdate")]
        public event Action<ClientInfo[]> OnChannelUsersUpdate = (infos) => { };
        public event Action<string, ClientInfo[]> OnChannelUsersListUpdate = (channel, infos) => { };
        public event Action<ChatMessage> OnReceivePrivateMessage = m => { };
        public event Action<ClientInfo, ChatMessage[]> OnReceivePrivateMessageHistory = (c, h) => { };
        public event Action<string, ChatMessage[]> OnChannelHistory = (c, h) => { };
        [Obsolete("Use OnBanGranted")]
        public event Action<float> OnAutoBan = (time) => { };
        public event Action<ClientInfo, BanReason, int> OnBanGranted = (info, reason, time) => { };
        public event Action<ClientInfo, BanReason, int, string> OnBanGrantedOnChannel = (info, reason, arg3, arg4) => { };
        public event Action<string> OnServiceMessage = (m) => { };

        public ChatClient()
        {
            OnConnected += SendPendingRequests;
        }

        public bool IsConnected => _client?.IsConnected ?? false;

        IChatClientTransport GetOrCreateChatClient()
        {
            if (_client != null)
                return _client;
            return _client = new ChatClientTransportLidgren(_secure);
        }

        public void Init(string playerName, string chatServerAddr, int chatServerPort, IInternetReachability internetReachability, bool secure)
        {
            _secure = secure;
            GetOrCreateChatClient();
            //_client = new ChatClientTransportTcp(chatServerAddr, chatServerPort, new ChatProtocolJsonSerializer(), secure);
            _playerName = playerName;
            _serverAddr = chatServerAddr;
            _serverPort = chatServerPort;
            _internetReachability = internetReachability;
            _client.Disconnect += Disconnect;
        }

        public void RenamePlayer(string newName)
        {
            _playerName = newName;
            if (_clientInfo != null)
            {
                _clientInfo.Name = _playerName;
            }
        }

        public void Connect()
        {
            if(_channel == null)
                throw new ArgumentNullException();
            if(_auth_data == null)
                throw new ArgumentNullException();

            Connect(_channel, _auth_data);
        }

        /// <summary>
        /// Connect and join main channel
        /// </summary>
        /// <param name="channel">main channel</param>
        /// <param name="auth_data">some const client secret (using PlayerId is less secure, but possible)</param>
        public void Connect(string channel, byte[] auth_data)
        {
            if (_state != STATE.DISCONNECTED && _state != STATE.NONE)
                return;
            _channel = channel;
            _auth_data = auth_data;
            _state = STATE.CONNECTING;
            GetOrCreateChatClient();
            _todoJobs.Add(new ChatJob(ConnectJobWork, OnConnectCompleted));
        }

        public void RequestChannelMessageHistory(string channel)
        {
            SendRequest(new ChatProtocol() { CommandType = CommandType.SendHistory, ChannelName = channel });
        }

        public void RequestPrivateMessageHistory()
        {
            SendRequest(new ChatProtocol(){ CommandType = CommandType.SendPrivateHistory});
        }

        public void JoinChannel(string channelName)
        {
            SendRequest(new ChatProtocol() {CommandType = CommandType.JoinChannel, ChannelName = channelName });
        }

        public void LeaveChannel(string channel)
        {
            SendRequest(new ChatProtocol() {CommandType = CommandType.LeaveChannel, ChannelName = channel});
        }

        public void SendChatMessageTo(ClientInfo sendTo, string message, Action<bool> sendCallback = null, byte[] messageMetadata = null)
        {
            SendChatMessageInt(message, sendCallback: sendCallback, sendTo: sendTo, messageMetadata: messageMetadata);
        }

        public void SendChatMessage(string channel, string message, Action<bool> sendCallback = null, byte[] messageMetadata = null)
        {
            SendChatMessageInt(message, channel, sendCallback, messageMetadata: messageMetadata);
        }

        public void BanUser(ClientInfo user, int period)
        {
            SendRequest(new ChatProtocol() {CommandType = CommandType.BanUser, Clients = new[] { user }, Body = period.ToString()});
        }

        public void ListChannels(Action<string[]> callback)
        {
            Action<ChatProtocol> ansProc = (p) =>
            {
                try
                {
                    var result = JsonConvert.DeserializeObject<string[]>(p.Body);
                    callback(result);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            };

            SendRequest(new ChatProtocol()
            {
                CommandType = CommandType.ListChannels
            }, answer: ansProc);
        }

        public void BadWordNotify(string channel, BanReason reason)
        {
            var message = new ChatProtocol()
            {
                CommandType = CommandType.MessageFilterReport,
                ChannelName = channel,
                BanReason = reason
            };
            SendRequest(message);
        }


        private void SendChatMessageInt(string message, string channel = null, Action<bool> sendCallback = null, ClientInfo sendTo = null, byte[] messageMetadata = null)
        {
            if (_state != ChatClient.STATE.CONNECTED)
            {
                if ((DateTime.UtcNow - _disconnectTime) > TimeSpan.FromSeconds(5))
                    Connect(_channel, _auth_data);
            }
            if (message.Length > 1000)
            {
                message = message.Substring(0, 1000);
            }
            var protocol = new ChatProtocol()
            {
                CommandType = CommandType.Message,
                Body = message,
                BodyMetadata = messageMetadata,
            };
            if (channel != null)
                protocol.ChannelName = channel;
            if (sendTo != null)
                protocol.SendTo = sendTo.Token;

            if (sendCallback == null)
                sendCallback = (b) => { };

            SendRequest(protocol, (b) => SendMessageToSelf(b, protocol, sendCallback));
        }

        private void SendMessageToSelf(bool sendResult, ChatProtocol packet, Action<bool> sendCallback)
        {
            if (_serverVersion > 0)
            {
                sendCallback(sendResult);
                return;
            }

            if (!sendResult)
            {
                Log.Error("Send message failed. dump=" + JsonConvert.SerializeObject(packet));
                sendCallback(false);
                return;
            }

            ProcessPacket(packet);
            sendCallback(true);
        }

        public void Disconnect()
        {
            if (_state == ChatClient.STATE.CONNECTED)
            {
                OnLeaveChannel(_channel);
                LeftMainChannel = true;
                foreach (var extraChannel in _extra_channels)
                {
                    OnLeaveChannel(extraChannel);
                }
                _extra_channels.Clear();
            }

            _state = ChatClient.STATE.DISCONNECTED;
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }

            OnDisconnect();
        }

        public void Update()
        {
            int jobIndex = 0;
            ChatProtocol newMessage;
            if (_client != null)
            {
                while ((newMessage = _client.ReadMessage()) != null)
                {
                    _incomingMessages.Add(newMessage);
                }
            }

            while (jobIndex < _currentJobs.Count)
            {
                if (_currentJobs[jobIndex].jobDone)
                {
                    _currentJobs[jobIndex].NotifyComplete();
                    _currentJobs.RemoveAt(jobIndex);
                }
                else
                {
                    jobIndex++;
                }
            }

            if (_todoJobs.Count > 0 && _currentJobs.Count < MAX_JOBS)
            {
                ChatJob job = _todoJobs[0];
                _todoJobs.RemoveAt(0);
                _currentJobs.Add(job);

                //Start a new thread

                ThreadPool.QueueUserWorkItem((object o) => job.DoJob());
            }


            for (int i = 0; i < _incomingMessages.Count; ++i)
            {
                ProcessPacket(_incomingMessages[i]);
            }

            _incomingMessages.Clear();
        }

        #region ChatMainLoop

        

        private bool ProcessTaggedPacket(ChatProtocol packet)
        {
            if (packet.Tag == 0)
                return false;

            Action<ChatProtocol> callback;
            if (!_tagMap.TryGetValue(packet.Tag, out callback))
                return false;

            _tagMap.Remove(packet.Tag);
            callback(packet);
            return true;
        }

        private void ProcessPacket(ChatProtocol packet)
        {
            if (ProcessTaggedPacket(packet))
                return;

            // Debug.Log("ProcessPacket: " + packet.CommandType);
            switch (packet.CommandType)
            {
                case (CommandType.LoginResult):
                    if (packet.Body.ToLower() == "true")
                    {
                        if (packet.ClientInfo != null)
                        {
                            _serverVersion = 1;
                            Log.DebugFormat("Chat token " + packet.ClientInfo.Token);
                            _clientInfo = ClientInfo.Create(packet.ClientInfo.Token, _playerName);
                        }
                        else
                        {
                            _clientInfo = new ClientInfo()
                            {
#pragma warning disable 612
                                Id = _auth_data,
#pragma warning restore 612
                                Name = _playerName
                            };
                        }
                        OnConnected();
                        LeftMainChannel = false;
                        OnJoinChannel(packet.ChannelName);
                        Log.DebugFormat("Chat server version {0}", _serverVersion);
                        var channel = packet.ChannelName != null ? packet.ChannelName : _channel; // old chat support
                        SendRequest(new ChatProtocol() { CommandType = CommandType.SendHistory, ChannelName = channel });
                        RequestPrivateMessageHistory();
                        foreach (var ch in _extra_channels)
                        {
                            JoinChannel(ch);
                        }
                    }
                    else
                    {
                        Disconnect();
                    }
                    break;
                case CommandType.JoinedChannel:
                    _extra_channels.Add(packet.ChannelName);
                    OnJoinChannel(packet.ChannelName);
                    break;
                case (CommandType.Message):
                    // Debug.Log("Receive: " + packet.Body);
                    if (packet.ChannelName != null)
                    {
                        DateTime dt;
                        if(!_lastMsg.TryGetValue(packet.ChannelName, out dt) || dt < packet.Time)
                            _lastMsg[packet.ChannelName] = packet.Time;
                    }

                    var m = new ChatMessage(packet);
                    if (m.Private)
                        OnReceivePrivateMessage(m);
                    else
                    {
                        if (packet.ClientInfo.Token == _clientInfo.Token)
                        {
                            _banExpiry = DateTime.MinValue;
                        }
                        OnReceiveChannelMessage(m);
                        OnReceiveMessage(m.FromName, m.Message);
                    }
                    break;
                case CommandType.ClientsChanged:
                    if (UpdateChannelList)
                    {
                        Log.Debug("Requesting channel clients list");
                        ListChannelClients(packet.ChannelName);
                    }
                    break;
                case CommandType.SendClientList:
                    if (_serverVersion == 0)
                        packet.ChannelName = _channel;

                    _channelUsers[packet.ChannelName] = packet.Clients;
                    OnChannelUsersListUpdate(packet.ChannelName, packet.Clients);
                    if(packet.ChannelName == _channel || string.IsNullOrEmpty(packet.ChannelName))
                        OnChannelUsersUpdate(packet.Clients);
                    break;
                case (CommandType.SendHistory):
                    if (packet.MessageHistory != null)
                    {
                        if (HistoryAsEvents)
                        {
                            var messages = packet.MessageHistory.Select(_ => new ChatMessage(_)).ToArray();
                            OnChannelHistory(packet.ChannelName, messages);
                            break;
                        }

                        var dt = DateTime.MinValue;
                        if (packet.ChannelName != null)
                            _lastMsg.TryGetValue(packet.ChannelName, out dt);
                        // Debug.Log("History: " + packet.MessageHistory.Length);
                        for (int i = 0; i < packet.MessageHistory.Length; ++i)
                        {
                            var msg = packet.MessageHistory[i];
                            if (msg.Time <= dt)
                                continue;
                            ProcessPacket(msg);
                        }
                    }
                    break;
                case CommandType.SendPrivateHistory:
                    if (packet.ClientInfo != null && packet.MessageHistory != null)
                    {
                        var messages = packet.MessageHistory.Select(_ => new ChatMessage(_)).ToArray();
                        OnReceivePrivateMessageHistory(packet.ClientInfo, messages);
                    }
                    break;
                case CommandType.LeftChannel:
                    _extra_channels.Remove(packet.ChannelName);
                    LeftMainChannel |= packet.ChannelName == _channel;
                    OnLeaveChannel(packet.ChannelName);
                    break;
                case CommandType.StopFlood:
                {
                    var time = 0f;
                    if (float.TryParse(packet.Body, out time))
                    {
                        OnAutoBan(time);
                        OnBanGranted(_clientInfo, BanReason.Flood, (int)time);
                        OnBanGrantedOnChannel(_clientInfo, BanReason.Flood, (int) time, packet.ChannelName);
                    }
                }
                    break;
                case CommandType.BanGranted:
                {
                    var time = 0;
                    if (int.TryParse(packet.Body, out time))
                    {
                        if (_clientInfo.Token == packet.ClientInfo.Token)
                        {
                            var expiryTime = DateTime.UtcNow + TimeSpan.FromSeconds(time);
                            if (expiryTime > _banExpiry)
                                _banExpiry = expiryTime;
                        }

                        OnBanGranted(packet.ClientInfo, packet.BanReason, time);
                        OnBanGrantedOnChannel(packet.ClientInfo, packet.BanReason, time, packet.ChannelName);
                    }
                }
                    break;
                case CommandType.ServiceMessage:
                    OnServiceMessage(packet.Body);
                    break;
            }
        }

        #endregion

        #region internal Events
        private void OnConnectCompleted(bool success)
        {
            if (!success)
            {
                OnDisconnected();
                _state = STATE.DISCONNECTED;
                return;
            }

            //Debug.Log("OnConnectCompleted: " + _state);
            if (_state == ChatClient.STATE.CONNECTING && !string.IsNullOrEmpty(_channel))
            {
                _state = ChatClient.STATE.CONNECTED;
                SendRequest(new ChatProtocol() { CommandType = CommandType.ClientLoginInform, ChannelName = _channel });
            }
            _connectAwait.Set();
        }

        private void OnSendCompleted(bool success)
        {
            if (!success)
                Log.Error("Can't send chat message");
            // Debug.Log("Send completed");
        }

        private void OnDisconnect()
        {
            if(!_disposed)
                _connectAwait.Reset();
            _state = ChatClient.STATE.DISCONNECTED;
            _disconnectTime = DateTime.UtcNow;
            OnDisconnected();
        }

        #endregion

        #region Jobs
        private bool ConnectJobWork(ChatJob job)
        {
            _client.Start(_serverAddr, _serverPort);
            Thread.Sleep(1500);

            if (_client == null)
                return false;
            return _client.IsConnected;
        }

        private bool SendDataJob(ChatJob job)
        {
            var packet = job.ChatProtocol;
            if (packet.CommandType == CommandType.ClientLoginInform)
            {
                packet.ClientInfo = new ClientInfo()
                {
#pragma warning disable 612
                    Id = _auth_data, // old chat support
                    Name = _playerName,
                    AuthData = _auth_data,
                    PlayerId = PlayerIdToSend
#pragma warning restore 612
                };
            }
            else
            {
                SpinWait.SpinUntil(() => _clientInfo != null);
                packet.ClientInfo = _clientInfo.Copy();
            }

            packet.Version = 1;

            _connectAwait.Wait(_destroyCancelation.Token);

            Log.Debug($"CHAT _client.SendMessage {JsonConvert.SerializeObject(packet)}");
            _client.SendMessage(packet);

            return true;
        }
        #endregion

        private void ListChannelClients(string channel)
        {
            var protocol = new ChatProtocol()
            {
                CommandType = CommandType.SendClientList,
                ChannelName = channel
            };
            SendRequest(protocol);
        }

        private void SendRequest(ChatProtocol packet, Action<bool> sendCallback = null, Action<ChatProtocol> answer = null)
        {
            SendRequest(new RequestData()
            {
                Message = packet,
                SendCallback = sendCallback,
                AnswerCallback = answer
            });
        }

        private void SendPendingRequests()
        {
            if (_state != ChatClient.STATE.CONNECTED)
                return;

            for (var i = _pendingRequests.Count - 1; i >= 0; i--)
            {
                if(SendRequest(_pendingRequests[i]))
                    _pendingRequests.RemoveAt(i);
            }
        }

        private bool SendRequest(RequestData data)
        {
            if (_state != ChatClient.STATE.CONNECTED)
            {
                _pendingRequests.Add(data);
                return false;
            }

            var packet = data.Message;
            if (packet == null)
                return false;

            var sendCallback = data.SendCallback;
            if (sendCallback == null)
                sendCallback = OnSendCompleted;

            if (data.AnswerCallback != null)
            {
                packet.Tag = ++_nextTag;
                _tagMap[packet.Tag] = data.AnswerCallback;
            }

            _todoJobs.Add(new ChatJob(SendDataJob, sendCallback, packet));
            return true;
        }

        public void Close()
        {
            _destroyCancelation.Cancel();
            if (_client != null)
            {
                try
                {
                    _client.Close();
                }
                catch (Exception ex)
                {
                    Log.Error("Exception when trying to close socket: " + ex.Message);
                }
            }
            Dispose();
        }

        public void Dispose()
        {
            if(_client != null)
                _client.Disconnect -= Disconnect;
            _connectAwait?.Dispose();
            _destroyCancelation?.Dispose();
            _disposed = true;
        }
    }
}
