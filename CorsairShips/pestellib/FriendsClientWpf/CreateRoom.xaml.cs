using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using FriendsClient;
using FriendsClient.Lobby;
using FriendsClient.Private;
using S;
using ServerShared.Sources.Numeric;
using UnityDI;

namespace FriendsClientWpf
{
    /// <summary>
    /// Interaction logic for CreateRoom.xaml
    /// </summary>
    public partial class CreateRoom : Window
    {
        private readonly IFriendsRoom _room;

        [Dependency]
        private FriendsClient.Private.FriendsClient _client;
        private List<RoomCanInviteFriendItem> _canInviteFriendItems = new List<RoomCanInviteFriendItem>();
        private List<RoomFriendItem> _friendItems = new List<RoomFriendItem>();

        private bool _closed;
        private bool _battleStarted;

        public CreateRoom(IFriendsRoom room)
        {
            _room = room;
            ContainerHolder.Container.BuildUp(this);
            InitializeComponent();

            var opts = Application.Current.Properties["Options"] as Options;
            if (opts != null)
            {
                this.Title += $" :: {opts.ConfigPath} :: {_client.FriendList.Me.Id} {_client.FriendList.Me.Profile.Nick}";
            }

            _room.OnImHost += OnImHost;
            _room.OnStartBattle += OnStartBattle;
            _room.OnJoined += OnJoined;
            _room.OnKick += OnKickLeave;
            _room.OnLeave += OnKickLeave;
            _room.OnGameData += OnGameData;
            _room.OnCanInviteChanged += OnCanInviteChanged;

            _ = Countdown();
            UpdateLists();
            UpdateButtons();
            OnGameData(_room, _room.GameSpecificData);
        }

        private void OnCanInviteChanged(IFriendsRoom room)
        {
            Application.Current.Dispatcher.Invoke(UpdateLists);
        }

        private void OnGameData(IFriendsRoom room, string s)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var c in GameModeSelector.Children)
                {
                    var rb = c as RadioButton;
                    if(rb == null) continue;
                    if (rb.Content as string == s)
                    {
                        rb.IsChecked = true;
                    }
                }
            });
        }

        private void OnKickLeave(IFriendsRoom room, MadId madId)
        {
            var selfKick = _client.Id == madId;
            if (selfKick)
            {
                MainWindow.Logger.Instance.Log("Kicked room for inactivity.");
                Application.Current.Dispatcher.Invoke(Close);
            }

            Application.Current.Dispatcher.Invoke(UpdateLists);
        }

        private void OnJoined(IFriendsRoom room, FriendBase friendBase)
        {
            Application.Current.Dispatcher.Invoke(UpdateLists);
        }

        private void OnStartBattle(IFriendsRoom room)
        {
            MainWindow.Logger.Instance.Log($"Battle started with: {_room.GameSpecificData}, party size: {_room.Party.Count}.");
            _battleStarted = true;
        }

        private async Task Countdown()
        {
            while (!_closed)
            {
                UpdateTimer();
                if (_battleStarted)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var item in _canInviteFriendItems)
                        {
                            item.IsEnabled = false;
                        }

                        foreach (var item in _friendItems)
                        {
                            item.IsEnabled = false;
                        }
                    });
                    break;
                }

                await Task.Delay(1000);
            }
        }

        private void OnImHost(IFriendsRoom room)
        {
            Application.Current.Dispatcher.Invoke(UpdateLists);
            UpdateButtons();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _closed = true;
            _room.OnImHost -= OnImHost;
            _room.OnStartBattle -= OnStartBattle;
            _room.OnJoined -= OnJoined;
            _room.OnKick -= OnKickLeave;
            _room.OnLeave -= OnKickLeave;
            _room.OnGameData -= OnGameData;
            _room.Close();
            ClearLists();
            base.OnClosing(e);
        }

        private void UpdateTimer()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!_battleStarted)
                {
                    if (_room.BattleCountdown <= TimeSpan.Zero && cbStartAfterCountdown.IsChecked.HasValue && cbStartAfterCountdown.IsChecked.Value)
                    {
                        _room.StartBattle(_startCallback);
                    }

                    lblCountdown.Content = _room.BattleCountdown.ToString("g");
                }
                else
                {
                    GameModeSelector.IsEnabled = false;
                    btnStartBattle.IsEnabled = false;
                    lblCountdown.Content = "Battle started.";
                }
            });
        }

        private void ClearLists()
        {
            foreach (var item in _canInviteFriendItems)
            {
                item.Close();
            }

            foreach (var item in _friendItems)
            {
                item.Close();
                item.OnSelfLeave -= OnSelfLeave;
            }

            _canInviteFriendItems.Clear();
            _friendItems.Clear();

            InRoomListView.Children.Clear();
            CanInviteListView.Children.Clear();
        }

        private void UpdateLists()
        {
            ClearLists();
            
            //compat with versions < 1.4
            var partyLimit = _room.PartyLimit > 0 && _room.PartyLimit <= _room.Party.Count;
            foreach (var invitedFriend in _room.Party)
            {
                var item = new RoomFriendItem(_room, invitedFriend);
                item.OnSelfLeave += OnSelfLeave;
                InRoomListView.Children.Add(item);
                _friendItems.Add(item);
            }

            foreach (var inviteableFriend in _room.CanInvite)
            {
                var item = new RoomCanInviteFriendItem(_room, inviteableFriend, partyLimit);
                CanInviteListView.Children.Add(item);
                _canInviteFriendItems.Add(item);
            }
        }

        private void OnSelfLeave()
        {
            Close();
        }

        private void UpdateButtons()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GameModeSelector.IsEnabled = _room.ImHost;
                btnStartBattle.IsEnabled = _room.ImHost;
                cbStartAfterCountdown.IsEnabled = _room.ImHost;
            });
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if(!_room.ImHost) return;
            var b = (RadioButton) sender;
            _room.GameSpecificData = (string) b.Content;
        }

        private void btnStartBattle_Click(object sender, RoutedEventArgs e)
        {
            _room.StartBattle(_startCallback);
        }

        private void _startCallback(RoomResult result)
        {
            MainWindow.Logger.Instance.Log($"Start battle result {result}.");
            if (result == RoomResult.Success)
            {
                OnStartBattle(_room);
            }
        }
    }
}
