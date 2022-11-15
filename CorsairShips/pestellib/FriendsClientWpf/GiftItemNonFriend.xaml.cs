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
using FriendsClient.Private;
using FriendsClient.Sources;
using PestelLib.Utils;
using UnityDI;

namespace FriendsClientWpf
{
    /// <summary>
    /// Interaction logic for GiftItemNonFriend.xaml
    /// </summary>
    public partial class GiftItemNonFriend : UserControl, ICloseable
    {
        private IFriendGift _giftToClaim;
        [Dependency]
        private ITimeProvider _time;
        [Dependency]
        private FriendsClient.Private.FriendsClient _client;

        public new int Width => (int)ActualWidth;

        public GiftItemNonFriend(IFriendGift gift)
        {
            ContainerHolder.Container.BuildUp(this);
            InitializeComponent();

            _giftToClaim = gift;

            UpdateButton();
            UpdateInfo();

            _client.OnFriendStatus += OnFriendStatus;
        }

        private void OnFriendStatus(FriendStatusChangedMessage evt)
        {
            if(evt.From != _giftToClaim.FriendInfo.Id)
                return;

            UpdateInfo();
        }

        public void Close()
        {
            _client.OnFriendStatus -= OnFriendStatus;
        }

        private void UpdateInfo()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lblLevel.Content = _giftToClaim.FriendInfo.Profile.Level.ToString();
                lblName.Content = _giftToClaim.FriendInfo.Profile.Nick;
                if (_giftToClaim.FriendInfo.Status == FriendStatus.Offline)
                {
                    var delta = _time.Now - _giftToClaim.FriendInfo.LastStatus;
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
                    lblStatus.Content = Helper.GetFriendStatusString(_giftToClaim.FriendInfo);
                }
            });
        }

        private void UpdateButton()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_giftToClaim == null) return;
                btnGiftButton.IsEnabled = !_giftToClaim.Claimed;
            });
        }

        private void btnGiftButton_Click(object sender, RoutedEventArgs e)
        {
            if (_giftToClaim != null)
            {
                _giftToClaim.Claim(_claimCallback);
                return;
            }
        }

        private void _claimCallback(long giftId, GiftResult giftResult, DateTime nextGift)
        {
            MainWindow.Logger.Instance.Log($"Gift claim result: {giftResult}.");
            UpdateButton();
        }
    }
}
