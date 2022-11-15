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
using FriendsClient.Sources;
using PestelLib.Utils;
using UnityDI;

namespace FriendsClientWpf
{
    /// <summary>
    /// Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class FriendListItem : UserControl
    {
        [Dependency]
        private ITimeProvider _time;
        private readonly IFriendContext _context;
        private Task _updateTask;

        public FriendListItem(IFriendContext context)
        {
            ContainerHolder.Container.BuildUp(this);
            _context = context;
            _context.OnStatusChanged += OnStatusChanged;
            _context.OnFriendInfoChanged += UpdateData;
            InitializeComponent();
            UpdateData(_context);
        }

        public void Close()
        {
            _context.OnStatusChanged -= OnStatusChanged;
            _context.OnFriendInfoChanged -= UpdateData;
        }

        private void OnStatusChanged(IFriendContext ctx, int status)
        {
            Application.Current.Dispatcher.Invoke(() => UpdateData(_context));
        }

        private void UpdateData(IFriendContext ctx)
        {
            lblLevel.Content = _context.FriendInfo.Profile.Level.ToString();
            lblName.Content = _context.FriendInfo.Profile.Nick;
            if (_context.FriendInfo.Status == FriendStatus.Offline)
            {
                var delta = _time.Now - _context.FriendInfo.LastStatus;
                var updatePeriod = TimeSpan.FromMinutes(1);
                if (delta >= TimeSpan.FromDays(1))
                    lblStatus.Content = $"Offline. Last seen {delta.Days} day(s) ago.";
                else if(delta >= TimeSpan.FromHours(1))
                    lblStatus.Content = $"Offline. Last seen {delta.Hours} hour(s) ago.";
                else if(delta >= TimeSpan.FromMinutes(1))
                    lblStatus.Content = $"Offline. Last seen {delta.Minutes} min(s) ago.";
                else
                {
                    lblStatus.Content = $"Offline. Last seen {delta.Seconds} sec(s) ago.";
                    updatePeriod = TimeSpan.FromSeconds(1);
                }

                if(_updateTask != null && !_updateTask.IsCompleted)
                    _updateTask.Dispose();
                _updateTask = Task.Delay(updatePeriod).ContinueWith(_ =>
                {
                    _updateTask = null;
                    Application.Current.Dispatcher.Invoke(() => UpdateData(_context));
                });
            }
            else
            {
                lblStatus.Content = Helper.GetFriendStatusString(_context.FriendInfo);
            }
        }

        private void btnDeleteFriend_Click(object sender, RoutedEventArgs e)
        {
            var r = MessageBox.Show($"Remove friend {_context.FriendInfo.Id}:{_context.FriendInfo.Profile.Nick}?",
                "Delete friend", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(r == MessageBoxResult.Yes)
                _context.RemoveFriend();
        }
    }
}
