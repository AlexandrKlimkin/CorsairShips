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
using FriendsClient;
using FriendsClient.FriendList;
using FriendsClient.Private;
using FriendsClient.Sources;
using PestelLib.Utils;
using UnityDI;

namespace FriendsClientWpf
{
    /// <summary>
    /// Interaction logic for GiftItem.xaml
    /// </summary>
    public partial class GiftItem : UserControl, ICloseable
    {
        private const string SendGift = "Send gift";
        private const string ClaimGift = "Claim gift";
        private readonly IFriendContext _context;
        private IFriendGift _giftToClaim;
        [Dependency]
        private ITimeProvider _time;

        public new int Width => (int) ActualWidth;

        public GiftItem(IFriendContext context)
        {
            ContainerHolder.Container.BuildUp(this);
            _context = context;
            InitializeComponent();
            UpdateButton();
            UpdateInfo();

            _context.OnGift += _gift;
            _context.OnStatusChanged += (c, i) => UpdateInfo();
        }

        public void Close()
        {
            _context.OnGift -= _gift;
        }

        private void _gift(IFriendContext friend, IFriendGift gift)
        {
            UpdateButton();
        }

        private void UpdateInfo()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                lblLevel.Content = _context.FriendInfo.Profile.Level.ToString();
                lblName.Content = _context.FriendInfo.Profile.Nick;
                if (_context.FriendInfo.Status == FriendStatus.Offline)
                {
                    var delta = _time.Now - _context.FriendInfo.LastStatus;
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
                    lblStatus.Content = Helper.GetFriendStatusString(_context.FriendInfo);
                }
            });
        }

        private void UpdateButton()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if(_context == null) return;
                if (_context.Gifts.Count > 0)
                {
                    _giftToClaim = _context.Gifts.First();
                    btnGiftButton.Content = ClaimGift;
                    btnGiftButton.IsEnabled = true;
                    return;
                }

                _giftToClaim = null;
                btnGiftButton.Content = SendGift;
                btnGiftButton.IsEnabled = _context.CanSendGift;
                if (!btnGiftButton.IsEnabled)
                {
                    var delta = _context.FriendInfo.NextGift - _time.Now;
                    btnGiftButton.Content = $"Next in {delta:g}";
                    Task.Delay(1000).ContinueWith(_ => Application.Current.Dispatcher.Invoke(UpdateButton));
                }
            });
        }

        private void btnGiftButton_Click(object sender, RoutedEventArgs e)
        {
            if (_giftToClaim != null)
            {
                _giftToClaim.Claim(_claimCallback);
                return;
            }

            btnGiftButton.IsEnabled = false;
            _context.SendGift(0, (id, result, n) => UpdateButton());
        }

        private void _claimCallback(long giftId, GiftResult giftResult, DateTime nextGift)
        {
            MainWindow.Logger.Instance.Log($"Gift claim result: {giftResult}.");
            UpdateButton();
        }
    }
}
