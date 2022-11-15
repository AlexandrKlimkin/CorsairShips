using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PestelLib.ChatClient;
using PestelLib.ChatCommon;
using PestelLib.NetworkUtils.Sources.InternetReachability;
using PestelLib.ServerCommon.Threading;

namespace ChatAdmin
{
    public class SendTo
    {
        public ClientInfo Client;

        public override string ToString()
        {
            return ChatClientAdmin.RgxTags.Replace(Client.Name, "");
        }
    }

    class ChatClientAdmin : DisposableLoop
    {
        private static PestelLib.ChatClient.ChatClient _chat;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        public static Regex RgxTags = new Regex("<.*?>");

        private readonly string _channel;
        private Room _room;
        private Dispatcher _main;
        private ClientInfo SendTo;
        private ClientInfo[] _usersOnline;
        private HashSet<ClientInfo> _usersMap = new HashSet<ClientInfo>();
        private string myToken;
        private volatile bool _worker;
        private static int _instances;

        public ChatClientAdmin(string playerName, string host, int port, string channel, string secret, Room room, bool encrypted)
        {
            _channel = channel;
            _room = room;
            var newInst = _chat == null;
            if (newInst)
            {
                _chat = new PestelLib.ChatClient.ChatClient();
                _chat.Init(playerName, host, port, new InternetReachabilityStub(), encrypted);
            }

            _chat.UpdateChannelList = true;
            _chat.HistoryAsEvents = true;
            _chat.OnConnected += () => ServiceMessage($"Connected to {host}:{port}");
            _chat.OnConnected += () => myToken = _chat.ClientInfo.Token;
            _chat.OnDisconnected += () => ServiceMessage("Disconnected");
            _chat.OnJoinChannel += s => ServiceMessage($"Connected to channel {s}");
            _chat.OnLeaveChannel += s => ServiceMessage($"Leave channel {s}");
            _chat.OnReceiveChannelMessage += ChatMessage;
            _chat.OnChannelHistory += OnOnChannelHistory;
            if (newInst)
            {
                _chat.Connect(channel, Encoding.UTF8.GetBytes(secret));
            }
            else
            {
                _chat.JoinChannel(channel);
                _chat.RequestChannelMessageHistory(channel);
            }

            _chat.OnReceivePrivateMessageHistory += OnReceivePrivateMessageHistory;
            _chat.OnChannelUsersListUpdate += OnChannelUsersListUpdate;
            _chat.OnBanGranted += (c, r, p) => ServiceMessage($"User '{c.Name}:{c.Token}' banned. Reason={r},Period={p}sec");
            _chat.OnReceivePrivateMessage += OnReceivePrivateMessage;
            _chat.OnServiceMessage += ServiceMessage;
            _main = Dispatcher.CurrentDispatcher;

            _room.cbUsers.SelectionChanged += CbUsersOnSelectionChanged;
            _room.tbMessage.KeyDown += TbMessageOnKeyDown;
            _room.btnSend.Click += BtnSendOnClick;
            _room.btnBan.Click += BtnBanOnClick;

            Interlocked.Increment(ref _instances);
        }

        public void ListChannels(Action<string[]> callback)
        {
            _chat.ListChannels(callback);
        }

        private void TbMessageOnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Return)
            {
                SendMessage();
            }
        }

        private void BtnBanOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var banWnd = new GrantBan(_usersMap.Where(_ => _.Token != myToken).ToList());
            banWnd.Owner = Application.Current.MainWindow;
            var r = banWnd.ShowDialog();
            if(!r.HasValue || !r.Value)
                return;

            _chat.BanUser(banWnd.UserToBan.Client,banWnd.Seconds);
        }

        private void OnReceivePrivateMessage(ChatMessage chatMessage)
        {
            ServiceMessage($"Private message from '{chatMessage.FromName}:{chatMessage.FromToken}' '{chatMessage.Message}'");
        }

        private void SendMessage()
        {
            var message = _room.tbMessage.Text;
            if (SendTo == null)
            {
                _chat.SendChatMessage(_channel, message, b => ServiceMessage($"Message send result={b}"));
            }
            else
            {
                _chat.SendChatMessageTo(SendTo, message, b => ServiceMessage($"Private message send result={b}"));
            }
            _room.tbMessage.Text = string.Empty;
        }

        private void BtnSendOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            SendMessage();
        }

        private void CbUsersOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if(selectionChangedEventArgs.AddedItems.Count < 1)
                return;
            if (selectionChangedEventArgs.AddedItems[0] is string)
            {
                SendTo = null;
                return;
            }

            var item = (SendTo)selectionChangedEventArgs.AddedItems[0];
            SendTo = item.Client;
        }

        private void OnChannelUsersListUpdate(string s1, ClientInfo[] clientInfos)
        {
            if(_channel != s1)
                return;

            _main.Invoke(() =>
            {
                foreach (var ci in clientInfos)
                {
                    _usersMap.Add(ci);
                }
                _usersOnline = clientInfos;
                _room.cbUsers.Items.Clear();
                _room.cbUsers.Items.Add("All");
                ClientInfo selected = null;
                if (_room.cbUsers.SelectedItem != null)
                {
                    selected = (ClientInfo) _room.cbUsers.SelectedItem;
                }
                else
                {
                    _room.cbUsers.SelectedIndex = 0;
                }

                _room.tbUsers.Clear();
                foreach (var clientInfo in clientInfos)
                {
                    _room.tbUsers.Text += clientInfo.Name + "\n";
                    var idx = _room.cbUsers.Items.Add(new SendTo() {Client = clientInfo});
                    if (selected != null && clientInfo.Token == selected.Token)
                    {
                        _room.cbUsers.SelectedIndex = idx;
                    }
                }
            });
        }

        private void OnReceivePrivateMessageHistory(ClientInfo info, ChatMessage[] chatProtocols)
        {
            ServiceMessage($"Private message history from '{info.Name}:{info.Token}'");
            foreach (var chatProtocol in chatProtocols)
            {
                OnReceivePrivateMessage(chatProtocol);
            }
        }

        private void OnOnChannelHistory(string s1, ChatMessage[] chatProtocols)
        {
            if(_channel != s1)
                return;
            ServiceMessage($"History for channel '{s1}'. count={chatProtocols.Length}");
            foreach (var chatProtocol in chatProtocols)
            {
                ChatMessage(chatProtocol);
            }
        }

        protected override void Update(CancellationToken cancellationToken)
        {
            if (!_worker)
            {
                _semaphore.Wait();
                _worker = true;
            }

            _chat?.Update();
            if(cancellationToken.IsCancellationRequested)
                return;
            Thread.Sleep(1);
        }

        private void ChatMessage(ChatMessage msg)
        {
            if(msg.Channel != _channel)
                return;
            ServiceMessage($"'{RgxTags.Replace(msg.FromName, "")}:{msg.ToToken}': {msg.Message}", msg.Time);
        }

        private void AddText(string message)
        {
            var atEnd = _room.tbMessages.IsScrolledToEnd();
            _room.tbMessages.Text += message;
            if (atEnd)
            {
                _room.tbMessages.CaretIndex = _room.tbMessages.Text.Length;
                _room.tbMessages.ScrollToEnd();
            }
        }

        private void ServiceMessage(string message, DateTime dt)
        {
            Application.Current.Dispatcher.Invoke(() => AddText($"{dt.ToLocalTime()}: {message}\n"));
        }

        private void ServiceMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(() => AddText($"{DateTime.Now}: {message}\n"));
        }

        public override void Dispose()
        {
            if (_worker)
            {
                _semaphore.Release();
                _worker = false;
            }

            Interlocked.Decrement(ref _instances);

            if (_instances == 0)
            {
                _chat.Disconnect();
            }

            base.Dispose();
        }
    }
}
