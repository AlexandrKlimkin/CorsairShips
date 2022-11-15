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
using PestelLib.Utils;
using UnityDI;

namespace FriendsClientWpf
{
    /// <summary>
    /// Interaction logic for RoomCanInviteFriendItem.xaml
    /// </summary>
    public partial class RoomCanInviteFriendItem : UserControl
    {
        private readonly IFriendsRoom _room;
        private readonly InviteableFriend _friend;
        private readonly bool _isPartyLimit;
        private const string InviteStr = "Invite";
        [Dependency]
        private ITimeProvider _time;

        public RoomCanInviteFriendItem(IFriendsRoom room, InviteableFriend friend, bool isPartyLimit)
        {
            ContainerHolder.Container.BuildUp(this);

            _room = room;
            _friend = friend;
            _isPartyLimit = isPartyLimit;
            _friend.OnStatusChanged += OnStatusChanged;
            _friend.OnRejected += OnRejected;
            _room.OnImHost += OnImHost;

            InitializeComponent();
            UpdateInfo();
            UpdateActionButton(InviteStr);
        }

        private void OnRejected()
        {
            UpdateActionButton(InviteStr);
        }

        private void OnImHost(IFriendsRoom room)
        {
            UpdateActionButton(InviteStr);
        }

        private void UpdateActionButton(string actionStr)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                btnAction.IsEnabled = !_isPartyLimit && _room.ImHost && _friend.FriendInfo.Status == FriendStatus.Online && _room.RoomStatus == RoomStatus.Party && !_friend.HasInviteCooldown;
                if (_isPartyLimit)
                {
                    btnAction.Content = "Party limit";
                }
                else if (!_room.ImHost)
                {
                    btnAction.Content = "Not alowed";
                }
                else if(_friend.FriendInfo.Status == FriendStatus.Online)
                {
                    if (_friend.HasInviteCooldown)
                    {
                        btnAction.Content = "Cooldown " + (int) _friend.InviteCooldown.TotalSeconds;
                        Task.Delay(1000).ContinueWith(t => UpdateActionButton(InviteStr));
                    }
                    else
                        btnAction.Content = actionStr;
                }
                else
                {
                    var statusStr = Helper.GetFriendStatusString(_friend.FriendInfo);
                    btnAction.Content = statusStr;
                }
            });
        }

        private void OnStatusChanged(int i)
        {
            UpdateInfo();
        }

        public void Close()
        {
            _friend.OnStatusChanged -= OnStatusChanged;
            _friend.OnRejected -= OnRejected;
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

                UpdateActionButton(InviteStr);
            });
        }

        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            btnAction.IsEnabled = false;
            _friend.Invite(_inviteCallback);
        }

        private void _inviteCallback(long roomId, RoomResult result)
        {
            MainWindow.Logger.Instance.Log($"Invite send result: {result}.");
        }
    }
}
