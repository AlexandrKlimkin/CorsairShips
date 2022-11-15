using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FriendsClient;
using FriendsClient.Private;
using S;
using UnityDI;

namespace FriendsClientWpf
{
    /// <summary>
    /// Interaction logic for FriendList.xaml
    /// </summary>
    public partial class FriendList : Window
    {
        [Dependency]
        private FriendsClient.Private.FriendsClient _friendsClient;

        public string Id => _friendsClient.Id;
        private List<FriendListItem> _friendItems = new List<FriendListItem>();
        private List<IncomingInviteItem> _incomingInviteItems = new List<IncomingInviteItem>();
        private List<ICloseable> _giftsItems = new List<ICloseable>();
        private string _invitationTabTemplate = "Invites {0}";
        private string _giftsTabTemplate = "Gifts {0}";

        public FriendList()
        {
            ContainerHolder.Container.BuildUp(this);
            InitializeComponent();

            var opts = Application.Current.Properties["Options"] as Options;
            if (opts != null)
            {
                this.Title += $" :: {opts.ConfigPath} :: {_friendsClient.FriendList.Me.Id} {_friendsClient.FriendList.Me.Profile.Nick}";
            }

            BuildFriendListGrid();
            BuildInviteView();
            BuildGiftView();
            UpdateTabs();

            _friendsClient.FriendList.OnNewFriend += FriendListOnNewFriend;
            _friendsClient.OnInvite += FriendsClientOnInvite;
            _friendsClient.OnInviteAccepted += FriendsClientOnInviteAccepted;
            _friendsClient.OnInviteRejected += FriendsClientOnInviteRejected;
            _friendsClient.OnInviteCanceled += FriendsClientOnInviteCanceled;
            _friendsClient.OnFriendRemoved += FriendsClientOnFriendRemoved;
            _friendsClient.OnFriendGift += FriendsClientOnFriendGift;
            _friendsClient.OnFriendGiftClaimed += FriendsClientOnFriendGiftClaimed;
            _friendsClient.FriendList.OnFriendsUpdated += OnFriendsUpdated;
            _friendsClient.FriendList.OnGiftsUpdated += OnGiftsUpdated;
            _friendsClient.FriendList.OnInvitesUpdated += OnInvitesUpdated;
        }

        private void UpdateTabs()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                tabInvites.Header = string.Format(_invitationTabTemplate, _friendsClient.FriendList.FriendInvitations.Count);
                tabGifts.Header = string.Format(_giftsTabTemplate, _friendsClient.FriendList.Gifts.Count);
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (var giftItem in _giftsItems)
            {
                giftItem.Close();
            }
            _giftsItems.Clear();

            foreach (var incomingInviteItem in _incomingInviteItems)
            {
                incomingInviteItem.Close();
            }
            _incomingInviteItems.Clear();

            foreach (var it in _friendItems)
            {
                it.Close();
            }
            _friendItems.Clear();

            _friendsClient.FriendList.OnNewFriend -= FriendListOnNewFriend;
            _friendsClient.OnInvite -= FriendsClientOnInvite;
            _friendsClient.OnInviteAccepted -= FriendsClientOnInviteAccepted;
            _friendsClient.OnInviteRejected -= FriendsClientOnInviteRejected;
            _friendsClient.OnInviteCanceled -= FriendsClientOnInviteCanceled;
            _friendsClient.OnFriendRemoved -= FriendsClientOnFriendRemoved;
            _friendsClient.OnFriendGift -= FriendsClientOnFriendGift;
            _friendsClient.OnFriendGiftClaimed -= FriendsClientOnFriendGiftClaimed;
            _friendsClient.FriendList.OnFriendsUpdated -= OnFriendsUpdated;
            _friendsClient.FriendList.OnGiftsUpdated -= OnGiftsUpdated;
            _friendsClient.FriendList.OnInvitesUpdated -= OnInvitesUpdated;
            base.OnClosing(e);
        }

        private void OnInvitesUpdated()
        {
            UpdateTabs();
        }

        private void OnGiftsUpdated()
        {
            Application.Current.Dispatcher.Invoke(BuildGiftView);
            UpdateTabs();
        }

        private void FriendsClientOnFriendGiftClaimed(FriendGiftEventMessage friendGiftEventMessage)
        {
            Application.Current.Dispatcher.Invoke(BuildGiftView);
            UpdateTabs();
        }

        private void FriendsClientOnFriendGift(FriendGiftEventMessage friendGiftEventMessage)
        {
            Application.Current.Dispatcher.Invoke(BuildGiftView);
            UpdateTabs();
        }

        private void FriendListOnNewFriend(IFriendContext friendContext)
        {
            UpdateTabs();
            Application.Current.Dispatcher.Invoke(() =>
            {
                BuildFriendListGrid();
                BuildInviteView();
                BuildGiftView();
            });
        }

        private void FriendsClientOnFriendRemoved(FriendEventMessage friendEventMessage)
        {
            Application.Current.Dispatcher.Invoke(BuildFriendListGrid);
            Application.Current.Dispatcher.Invoke(BuildGiftView);
        }

        private void FriendsClientOnInviteCanceled(FriendsInviteEventMessage friendsInviteEventMessage)
        {
            Application.Current.Dispatcher.Invoke(BuildInviteView);
            UpdateTabs();
        }

        private void FriendsClientOnInviteRejected(FriendsInviteEventMessage friendsInviteEventMessage)
        {
            Application.Current.Dispatcher.Invoke(BuildInviteView);
            UpdateTabs();
        }

        private void FriendsClientOnInviteAccepted(FriendsInviteEventMessage friendsInviteEventMessage)
        {
            Application.Current.Dispatcher.Invoke(BuildInviteView);
            UpdateTabs();
        }

        private void FriendsClientOnInvite(FriendsInviteEventMessage friendsInviteEventMessage)
        {
            Application.Current.Dispatcher.Invoke(BuildInviteView);
            UpdateTabs();
        }

        private void BuildGiftView()
        {
            foreach (var giftItem in _giftsItems)
            {
                giftItem.Close();
            }
            _giftsItems.Clear();

            var w = 0;
            var cols = 0;
            StackPanel rowPanel = new StackPanel() { Orientation = Orientation.Vertical };
            StackPanel content = null;
            GiftsView.Content = rowPanel;
            var friendsMap = _friendsClient.FriendList.Friends.ToDictionary(_ => _.FriendInfo.Id);

            foreach (var friend in _friendsClient.FriendList.Friends)
            {
                var item = new GiftItem(friend);
                if (_giftsItems.Count == 0)
                {
                    w = (int)item.ActualWidth;
                    cols = (int)(tabMyFriends.ActualWidth / w);
                    content = new StackPanel() { Orientation = Orientation.Horizontal };
                    rowPanel.Children.Add(content);
                }
                else if (_giftsItems.Count % cols == 0)
                {
                    content = new StackPanel() { Orientation = Orientation.Horizontal };
                    rowPanel.Children.Add(content);
                }

                content?.Children.Add(item);
                _giftsItems.Add(item);
            }

            foreach (var gift in _friendsClient.FriendList.Gifts)
            {
                if(friendsMap.ContainsKey(gift.FriendInfo.Id))
                    continue;
                var item = new GiftItemNonFriend(gift);

                if (_giftsItems.Count == 0)
                {
                    w = (int)item.ActualWidth;
                    cols = (int)(tabMyFriends.ActualWidth / w);
                    content = new StackPanel() { Orientation = Orientation.Horizontal };
                    rowPanel.Children.Add(content);
                }
                else if (_giftsItems.Count % cols == 0)
                {
                    content = new StackPanel() { Orientation = Orientation.Horizontal };
                    rowPanel.Children.Add(content);
                }

                content?.Children.Add(item);
                _giftsItems.Add(item);
            }
        }

        private void OnFriendsUpdated()
        {
            Application.Current.Dispatcher.Invoke(BuildFriendListGrid);
            UpdateTabs();
        }

        private void BuildInviteView()
        {
            foreach (var incomingInviteItem in _incomingInviteItems)
            {
                incomingInviteItem.Close();
            }
            _incomingInviteItems.Clear();

            var w = 0;
            var h = 0;
            var cols = 0;
            StackPanel rowPanel = new StackPanel() {Orientation = Orientation.Vertical};
            StackPanel content = null;
            InvitesView.Content = rowPanel;
            foreach (var friendInvite in _friendsClient.FriendList.FriendInvitations)
            {
                var item = new IncomingInviteItem(friendInvite);

                friendInvite.OnAnswerSent += (c, r) => Application.Current.Dispatcher.Invoke(BuildInviteView);
                friendInvite.OnExpired += (c) => Application.Current.Dispatcher.Invoke(BuildInviteView);

                if (_incomingInviteItems.Count == 0)
                {
                    w = (int)item.ActualWidth;
                    h = (int)item.ActualHeight;
                    cols = (int)(tabInvites.ActualWidth / w);
                    content = new StackPanel() {Orientation = Orientation.Horizontal};
                    rowPanel.Children.Add(content);
                }
                else if (_incomingInviteItems.Count % cols == 0)
                {
                    content = new StackPanel() {Orientation = Orientation.Horizontal};
                    rowPanel.Children.Add(content);
                }

                content?.Children.Add(item);
                _incomingInviteItems.Add(item);
            }
            
        }

        private void BuildFriendListGrid()
        {
            foreach (var it in _friendItems)
            {
                it.Close();
            }
            _friendItems.Clear();

            var w = 0;
            var cols = 0;
            StackPanel rowPanel = new StackPanel() { Orientation = Orientation.Vertical };
            StackPanel content = null;
            MyFriendsView.Content = rowPanel;

            foreach (var friend in _friendsClient.FriendList.Friends)
            {
                var item = new FriendListItem(friend);

                friend.OnRemovedFromFriends += (c) => Application.Current.Dispatcher.Invoke(BuildFriendListGrid);
                if (_friendItems.Count == 0)
                {
                    w = (int) item.ActualWidth;
                    cols = (int) (tabMyFriends.ActualWidth / w);
                    content = new StackPanel() { Orientation = Orientation.Horizontal };
                    rowPanel.Children.Add(content);
                }
                else if (_friendItems.Count % cols == 0)
                {
                    content = new StackPanel() { Orientation = Orientation.Horizontal };
                    rowPanel.Children.Add(content);
                }

                content?.Children.Add(item);
                _friendItems.Add(item);
            }
        }

        private void btnFindFriend_Click(object sender, RoutedEventArgs e)
        {
            if (!MadId.TryParse(tbFindId.Text, out var id))
            {
                MessageBox.Show("Use ###### (# - 0..9,A..Z).", "Bad id format", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _friendsClient.FriendList.FindFriend(id, _friendFindResult);
        }

        private AddFriendDialog _dialog;
        private void _friendFindResult(IFriendContext friend)
        {
            if (friend == null)
            {
                MessageBox.Show("Friend not found.", "Not found", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_dialog != null)
                {
                    _dialog.OnIviteSentResult -= OnInviteSentResult;
                    _dialog.Close();
                }
                _dialog = new AddFriendDialog();
                _dialog.Owner = this;
                _dialog.FriendContext = friend;
                _dialog.Show();
                _dialog.OnIviteSentResult += OnInviteSentResult;
            });
        }

        private void OnInviteSentResult(InviteFriendResult inviteFriendResult)
        {
            if(inviteFriendResult == InviteFriendResult.AlreadyFriend)
                BuildFriendListGrid();

            _dialog.OnIviteSentResult -= OnInviteSentResult;
            _dialog = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(lblMyId.Content as string);
        }

        private void FriendListItem_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
