using System;
using System.Collections.Generic;
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
using FriendsClient.Lobby;
using FriendsClient.Private;
using FriendsClient.Sources;
using log4net.Repository.Hierarchy;
using PestelLib.Utils;
using UnityDI;

namespace FriendsClientWpf
{
    /// <summary>
    /// Interaction logic for RoomFriendItem.xaml
    /// </summary>
    public partial class RoomFriendItem : UserControl
    {
        private readonly IFriendsRoom _room;
        private readonly InvitedFriend _friend;
        private const string RemoveStr = "Remove";
        [Dependency]
        private ITimeProvider _time;

        [Dependency]
        private FriendsClient.Private.FriendsClient _client;
        public event Action OnSelfLeave = () => { };

        public RoomFriendItem(IFriendsRoom room, InvitedFriend friend)
        {
            ContainerHolder.Container.BuildUp(this);

            _room = room;
            _friend = friend;
            InitializeComponent();
            UpdateInfo();
            _friend.OnStatusChanged += OnStatusChanged;
            _room.OnImHost += OnImHost;
            UpdateActionButton(RemoveStr);
        }

        private void OnImHost(IFriendsRoom room)
        {
            UpdateActionButton(RemoveStr);
        }

        private void UpdateActionButton(string actionStr)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var self = _client.Id == _friend.FriendInfo.Id;
                var statusValid = _friend.FriendInfo.Status == FriendStatus.Online ||
                                  _friend.FriendInfo.Status == FriendStatus.InRoom;
                btnAction.IsEnabled = (_room.ImHost || self) && statusValid && _room.RoomStatus == RoomStatus.Party;
                if (self)
                {
                    btnAction.Content = "Leave";
                }
                else if (!_room.ImHost)
                {
                    btnAction.Content = "Not alowed";
                }
                else if (statusValid)
                {
                    btnAction.Content = actionStr;
                }
                else
                {
                    var statusStr = Helper.GetFriendStatusString(_friend.FriendInfo);
                    btnAction.Content = statusStr;
                }
            });
        }

        private void OnStatusChanged(InvitedFriend f, int i)
        {
            UpdateInfo();
        }

        public void Close()
        {
            _friend.OnStatusChanged -= OnStatusChanged;
            _room.OnImHost -= OnImHost;
        }

        private void UpdateInfo()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lblLevel.Content = _friend.FriendInfo.Profile.Level.ToString();
                lblName.Content = _friend.FriendInfo.Profile.Nick;
                if (_friend.FriendInfo.Status == FriendStatus.Offline)
                {
                    var delta = _time.Now - _friend.FriendInfo.LastStatus;
                    var updatePeriod = TimeSpan.FromMinutes(1);
                    if (delta >= TimeSpan.FromDays(1))
                        lblStatus.Content = $"Offline. Last seen {delta.Days} day(s) ago.";
                    else if (delta >= TimeSpan.FromHours(1))
                        lblStatus.Content = $"Offline. Last seen {delta.Hours} hour(s) ago.";
                    else if (delta >= TimeSpan.FromMinutes(1))
                        lblStatus.Content = $"Offline. Last seen {delta.Minutes} min(s) ago.";
                    else
                    {
                        lblStatus.Content = $"Offline. Last seen {delta.Seconds} sec(s) ago.";
                        updatePeriod = TimeSpan.FromSeconds(1);
                    }

                    Task.Delay(updatePeriod).ContinueWith(_ => { UpdateInfo(); });
                }
                else
                {
                    lblStatus.Content = Helper.GetFriendStatusString(_friend.FriendInfo);
                }

                UpdateActionButton(RemoveStr);
            });
        }

        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            var self = _client.Id == _friend.FriendInfo.Id;
            btnAction.IsEnabled = false;
            if(self)
                _room.Leave(leaveCallback);
            else
                _friend.Kick(kickCallback);
        }

        private void leaveCallback(bool success)
        {
            if(!success) return;
            OnSelfLeave();
        }

        private void kickCallback(long roomId, RoomResult result)
        {
            MainWindow.Logger.Instance.Log($"{_friend.FriendInfo.Id} kick result: {result}");
        }
    }
}
