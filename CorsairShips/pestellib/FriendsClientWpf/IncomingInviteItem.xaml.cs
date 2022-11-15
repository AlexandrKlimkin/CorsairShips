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
using FriendsClient.FriendList;
using FriendsClient.FriendList.Concrete;
using FriendsClient.Private;
using FriendsClient.Sources;
using PestelLib.Utils;
using UnityDI;

namespace FriendsClientWpf
{
    /// <summary>
    /// Interaction logic for IncomingInviteItem.xaml
    /// </summary>
    public partial class IncomingInviteItem : UserControl
    {
        private readonly IIncomingFriendInvite _invite;
        [Dependency]
        private ITimeProvider _time;

        public IncomingInviteItem(IIncomingFriendInvite invite)
        {
            ContainerHolder.Container.BuildUp(this);
            _invite = invite;
            InitializeComponent();

            UpdateInfo();
            _invite.OnAnswerSent += OnAnswerSent;
            _invite.OnStatusChanged += InviteOnStatusChanged;
            _invite.OnExpired += OnExpired;

            _ = ExpiryCountdown();
        }

        private void OnExpired(IIncomingFriendInvite inv)
        {
            MainWindow.Logger.Instance.Log($"Invite from {_invite.FriendInfo.Id}:{_invite.FriendInfo.Profile.Nick} expired.");
            Application.Current.Dispatcher.Invoke(() =>
            {
                btnAccept.IsEnabled = false;
                btnReject.IsEnabled = false;
                lblExpire.Content = "Expired";
            });
        }

        private void InviteOnStatusChanged(IIncomingFriendInvite inv, int i)
        {
            UpdateInfo();
        }

        private async Task ExpiryCountdown()
        {
            while (!_invite.Expired && _invite.SendResult == InviteFriendResult.None)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    {
                        if(_invite.Expired)
                            lblExpire.Content = "Expired";
                        else if (_invite.SendResult != InviteFriendResult.None)
                            lblExpire.Content = "Answer sent";
                        else
                            lblExpire.Content = $"Will expire in {_invite.ExpireTime - _time.Now}.";
                    });

                await Task.Delay(1000);
            }
        }

        private void OnAnswerSent(IIncomingFriendInvite inv, InviteFriendResult inviteFriendResult)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                btnAccept.IsEnabled = false;
                btnReject.IsEnabled = false;
                lblExpire.Content = "Answer sent";
            });
        }

        public void Close()
        {
            _invite.OnAnswerSent -= OnAnswerSent;
            _invite.OnStatusChanged -= InviteOnStatusChanged;
            _invite.OnExpired -= OnExpired;
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            btnAccept.IsEnabled = false;
            btnReject.IsEnabled = false;
            _invite.Accept(_inviteAnsCallback);
        }

        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            btnAccept.IsEnabled = false;
            btnReject.IsEnabled = false;
            _invite.Reject(_inviteAnsCallback);
        }

        private void _inviteAnsCallback(long inviteId, InviteFriendResult result)
        {
            MainWindow.Logger.Instance.Log($"Invite answer result: {result}");
        }

        private void UpdateInfo()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lblLevel.Content = _invite.FriendInfo.Profile.Level.ToString();
                lblName.Content = _invite.FriendInfo.Profile.Nick;
                if (_invite.FriendInfo.Status == FriendStatus.Offline)
                {
                    var delta = _time.Now - _invite.FriendInfo.LastStatus;
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
                    lblStatus.Content = Helper.GetFriendStatusString(_invite.FriendInfo);
                }
            });
        }
    }
}
