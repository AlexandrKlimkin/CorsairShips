using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using FriendsClient.Lobby;
using FriendsClient.Private;
using FriendsClient.Sources;
using FriendsClientWpf.Properties;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerShared;
using UnityDI;

namespace FriendsClientWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow));
        private FriendsClient.Private.FriendsClient _friendsClient;
        private Guid _playerId;
        private const string PauseText = "❚❚";
        private const string PlayerText = "▶";
        private const string StepText = "❚▶";
        private FriendList _friendList;
        private string _friendListTemplate;
        private bool _inited;
        [Dependency]
        private UpdateProviderController _updateProvider;

        public bool MuteBattleInvitations {
            get { return _friendsClient.MuteBattleInvitations; }
            set { _friendsClient.MuteBattleInvitations = value; }
        }

        static MainWindow()
        {
            Application.Current.DispatcherUnhandledException += UnhandledException;
        }

        private static void UnhandledException(object sender1, DispatcherUnhandledExceptionEventArgs args)
        {
            Log.Error(args.Exception);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _updateProvider.OnUpdate -= OnUpdateProvider;
            Application.Current.DispatcherUnhandledException -= UnhandledException;
            base.OnClosing(e);
        }

        public MainWindow()
        {
            InitDependencyInjection.Init();
            ContainerHolder.Container.BuildUp(this);

            InitializeComponent();

            _friendListTemplate = (string) btnFriendList.Content;
            _updateProvider.OnUpdate += OnUpdateProvider;

            Logger.CreateLogger(tbLog);

            if (ConfigLoader.Instance.PlayerId == Guid.Empty)
            {
                ConfigLoader.Instance.PlayerId = Guid.NewGuid();
                BindingOperations.GetBindingExpression(tbPlayerId, TextBox.TextProperty)?.UpdateTarget();
            }

            Logger.Instance.Log($"PlayerId {ConfigLoader.Instance.PlayerId}");

            CreateFriendsClient();

            if (ConfigLoader.Instance.Autostart)
            {
                Connect();
            }

            var opts = Application.Current.Properties["Options"] as Options;
            if (opts != null)
            {
                if (opts.DisableInstancing)
                {
                    tbInstanceCount.Visibility = Visibility.Hidden;
                    btnStartNewInst.Visibility = Visibility.Hidden;
                }
            }

            SetTitle();

            UpdateButtons();
        }

        private void SetTitle()
        {
            var template = "MainMenu :: {0} :: {1}";
            string config = "";
            var opts = Application.Current.Properties["Options"] as Options;
            if (opts != null)
            {
                config = opts.ConfigPath;
            }

            string user = "not connected";
            if (_friendsClient.IsConnected && _friendsClient.Initialized)
            {
                user = $"{_friendsClient.FriendList.Me.Id} {_friendsClient.FriendList.Me.Profile.Nick}";
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                Title = string.Format(template, config, user);
            });

        }

        private void OnUpdateProvider()
        {
            Application.Current.Dispatcher.Invoke(() => lblUpdateNo.Content = _updateProvider.UpdateCount.ToString());
        }

        private void CreateFriendsClient()
        {
            _inited = false;
            RemoveFriendsClient();
            _friendsClient = new FriendsClient.Private.FriendsClient(ConfigLoader.Instance.PlayerId, ConfigLoader.Instance.AuthData);
            ContainerHolder.Container.RegisterInstance(_friendsClient);

            _friendsClient.OnInitialized += FriendsClientOnInitialized;
            _friendsClient.OnConnectionError += OnConnectionError;
            _friendsClient.OnDisconnected += OnDisconnected;

            if (ConfigLoader.Instance.DebugEvents)
            {
                _friendsClient.OnInvite += LogEvent;
                _friendsClient.OnInviteAccepted += LogEvent;
                _friendsClient.OnInviteRejected += LogEvent;
                _friendsClient.OnInviteCanceled += LogEvent;
                _friendsClient.OnFriendRemoved += LogEvent;
                _friendsClient.OnFriendStatus += LogEvent;
                _friendsClient.OnFriendGift += LogEvent;
                _friendsClient.OnFriendGiftClaimed += LogEvent;
                _friendsClient.OnRoomInvite += LogEvent;
                _friendsClient.OnRoomAccept += LogEvent;
                _friendsClient.OnRoomReject += LogEvent;
                _friendsClient.OnRoomJoin += LogEvent;
                _friendsClient.OnRoomLeave += LogEvent;
                _friendsClient.OnRoomKick += LogEvent;
                _friendsClient.OnRoomInfo += LogEvent;
                _friendsClient.OnRoomStartBattle += LogEvent;
                _friendsClient.OnRoomNewHost += LogEvent;
                _friendsClient.OnRoomCountdown += LogEvent;
                _friendsClient.OnRoomGameData += LogEvent;
                _friendsClient.OnNewFriend += LogEvent;
                _friendsClient.OnProfileUpdate += LogEvent;
            }
        }

        private void OnRoomInvite(IIncomingRoomInvite incomingRoomInvite)
        {
            var result = MessageBox.Show($"{incomingRoomInvite.FriendInfo.Id}:{incomingRoomInvite.FriendInfo.Profile.Nick} invites you to join room. Accept?", "Room invite", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (incomingRoomInvite.Expired)
            {
                Logger.Instance.Log($"Invite expired.");
            }

            if (result == MessageBoxResult.Yes)
            {
                incomingRoomInvite.Accept((id, roomResult) => Logger.Instance.Log($"Room accept send result: {roomResult}"));
            }
            else
                incomingRoomInvite.Reject((id, roomResult) => Logger.Instance.Log($"Room accept send result: {roomResult}"));
        }

        private void RemoveFriendsClient()
        {
            if(_friendsClient == null) return;
            _friendsClient.Stop();
            _friendsClient.OnConnectionError -= OnConnectionError;
            _friendsClient.OnDisconnected -= OnDisconnected;
            if (_friendsClient.Lobby != null)
            {
                _friendsClient.Lobby.OnRoomInvite -= OnRoomInvite;
                _friendsClient.Lobby.OnJoinRoom -= OnJoinRoom;
            }

            _friendsClient.OnInvite -= LogEvent;
            _friendsClient.OnInviteAccepted -= LogEvent;
            _friendsClient.OnInviteRejected -= LogEvent;
            _friendsClient.OnInviteCanceled -= LogEvent;
            _friendsClient.OnFriendRemoved -= LogEvent;
            _friendsClient.OnFriendStatus -= LogEvent;
            _friendsClient.OnFriendGift -= LogEvent;
            _friendsClient.OnFriendGiftClaimed -= LogEvent;
            _friendsClient.OnRoomInvite -= LogEvent;
            _friendsClient.OnRoomAccept -= LogEvent;
            _friendsClient.OnRoomReject -= LogEvent;
            _friendsClient.OnRoomJoin -= LogEvent;
            _friendsClient.OnRoomLeave -= LogEvent;
            _friendsClient.OnRoomKick -= LogEvent;
            _friendsClient.OnRoomInfo -= LogEvent;
            _friendsClient.OnRoomStartBattle -= LogEvent;
            _friendsClient.OnRoomNewHost -= LogEvent;
            _friendsClient.OnRoomCountdown -= LogEvent;
            _friendsClient.OnRoomGameData -= LogEvent;
            _friendsClient.OnNewFriend -= LogEvent;
            _friendsClient.OnProfileUpdate -= LogEvent;
        }

        private void OnConnectionError()
        {
            Logger.Instance.Log("Connection error.");
            UpdateButtons();
        }

        private void OnDisconnected()
        {
            Logger.Instance.Log("Disconnected.");
            Application.Current.Dispatcher.Invoke(() =>
            {
                _friendList?.Close();
                _friendList = null;
                _room?.Close();
                _room = null;
                UpdateButtons();
            });
        }

        class EventConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var t = JToken.FromObject(value.ToString());
                t.WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var str = reader.ReadAsString();
                return Enum.Parse(typeof(FriendEvent), str);
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(FriendEvent);
            }
        }

        private void LogEvent(object evt)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters = new List<JsonConverter>()
            {
                new EventConverter()
            };
            Logger.Instance.Log($"Event: {JsonConvert.SerializeObject(evt, settings)}.");
        }

        private void FriendsClientOnInitialized(FriendInitResult friendInitResult)
        {
            var configDump = JsonConvert.SerializeObject(_friendsClient.Config);
            Logger.Instance.Log($"Init result '{friendInitResult}'. Config={configDump}.");
            if (friendInitResult == FriendInitResult.Success)
            {
                if (ConfigLoader.Instance.Autostart)
                {
                    _updateProvider.Resume();
                }
            }

            if (!_inited)
            {
                SetTitle();
                _friendsClient.Lobby.OnRoomInvite += OnRoomInvite;
                _friendsClient.Lobby.OnJoinRoom += OnJoinRoom;
                _friendsClient.FriendList.OnInvitesUpdated += UpdateNewCount;
                _friendsClient.FriendList.OnGiftsUpdated += UpdateNewCount;
                _inited = true;
            }

            UpdateNewCount();
        }

        private void UpdateNewCount()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var newCount = _friendsClient.FriendList.FriendInvitations.Count;
                newCount += _friendsClient.FriendList.Gifts.Count;
                if (newCount > 0)
                    btnFriendList.Content = string.Format(_friendListTemplate, newCount);
                else
                    btnFriendList.Content = string.Format(_friendListTemplate, "");
                UpdateButtons();
            });
        }

        private CreateRoom _room;
        private void OnJoinRoom(IFriendsRoom friendsRoom)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if(_room != null) _room.Close();
                _room = new CreateRoom(friendsRoom);
                _room.Owner = this;
                _room.Closed += (sender, args) =>
                {
                    _room = null;
                    UpdateButtons();
                };
                _room.Show();
            });
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void Connect()
        {
            Logger.Instance.Log($"Connecting to friends server.");
            _friendsClient.Start();
            if (ConfigLoader.Instance.StartUpdateOnConnect)
            {
                _updateProvider.Resume();
                UpdateButtons();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigLoader.Save();
        }

        private void bntNewPlayerId_Click(object sender, RoutedEventArgs e)
        {
            ConfigLoader.Instance.PlayerId = _playerId = Guid.NewGuid();
            tbPlayerId.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
            CreateFriendsClient();
            Logger.Instance.Log($"PlayerId {ConfigLoader.Instance.PlayerId}.");
        }

        private void tbUpdatePeriod_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(TimeSpan.TryParse(tbUpdatePeriod.Text, out var ts) && ts > TimeSpan.Zero)
                _updateProvider?.SetSpeed(ts);
        }

        private void btpStartStopUpdate_Click(object sender, RoutedEventArgs e)
        {
            if(_updateProvider.Paused)
                _updateProvider.Resume();
            else
                _updateProvider.Pause();
            
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            btnStartStopUpdate.Content = _updateProvider.Paused ? PlayerText : PauseText;
            btnStepUpdate.IsEnabled = _updateProvider.Paused;
            var isConnected = _friendsClient.IsConnected && _friendsClient.Initialized;
            btnConnect.IsEnabled = !isConnected;
            btnDisconnect.IsEnabled = isConnected;
            btnFriendList.IsEnabled = isConnected;
            btnCreateRoom.IsEnabled = _room == null && isConnected;
        }

        private void btnStepUpdate_Click(object sender, RoutedEventArgs e)
        {
            if(!_updateProvider.Paused) return;
            _updateProvider.Step();
        }

        private void btnFriendList_Click(object sender, RoutedEventArgs e)
        {
            if(_friendList != null) return;
            _friendList = new FriendList();
            _friendList.Show();
            btnFriendList.IsEnabled = false;
            _friendList.Closed += (o, evt) =>
            {
                _friendList = null;
                btnFriendList.IsEnabled = true;
            };
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            _friendsClient.Stop();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(!int.TryParse(tbInstanceCount.Text, out var count) || count < 1)
                return;

            for (var i = 0; i < count; i++)
            {
                var path = ConfigLoader.Instance.Duplicate(i);
                Process.Start("FriendsClientWpf.exe", $"--config={path} --disable_instancing");
            }
        }

        private void btnCreateRoom_Click(object sender, RoutedEventArgs e)
        {
            _friendsClient.Lobby.CreateRoom(ConfigLoader.Instance.BattleAutostartDelay, "", _createRoomCallback);
        }

        private void _createRoomCallback(RoomResult roomResult, IFriendsRoom friendsRoom)
        {
            Logger.Instance.Log($"Create room result: {roomResult}.");
            Application.Current.Dispatcher.Invoke(() =>
            {
                if(_room != null) _room.Close();
                _room = new CreateRoom(friendsRoom);
                _room.Owner = this;
                _room.Closed += (sender, args) =>
                {
                    _room = null;
                    UpdateButtons();
                };
                _room.Show();
            });
        }

        private void TbPlayerId_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!Guid.TryParse(tbPlayerId.Text, out var guid)) return;
            if(ConfigLoader.Instance.PlayerId == guid) return;
            ConfigLoader.Instance.PlayerId = guid;
            CreateFriendsClient();
            Application.Current.Dispatcher.Invoke(UpdateButtons);
            Logger.Instance.Log($"PlayerId {ConfigLoader.Instance.PlayerId}");
        }
    }
}
