using System;
using System.Windows;
using FriendsClient;
using FriendsClient.FriendList;
using FriendsClient.Private;
using UnityDI;

namespace FriendsClientWpf
{
    /// <summary>
    /// Interaction logic for AddFriendDialog.xaml
    /// </summary>
    public partial class AddFriendDialog : Window
    {
        private IFriendContext _friendContext;

        public FriendBase FriendBase { get; set; }
        public IFriendContext FriendContext
        {
            get { return _friendContext;}
            set
            {
                _friendContext = value;
                friendInfo.FriendContext = value;
                if (_invite == null)
                {
                    _invite = FriendContext.InviteFriend();
                    if (_invite.Status != InviteStatus.None)
                    {
                        s.IsEnabled = false;
                        s.Content = "Already sent";
                    }
                    else
                    {
                        _invite.OnInviteSendResult += OnInviteSendResult;
                        _invite.OnAccept += OnAccept;
                        _invite.OnReject += OnReject;
                    }
                }
            }
        }

        public event Action<InviteFriendResult> OnIviteSentResult = r => { };

        public AddFriendDialog()
        {
            ContainerHolder.Container.BuildUp(this);

            InitializeComponent();
        }

        private void s_Click(object sender, RoutedEventArgs e)
        {
            _invite.Send();
            Close();
        }

        private void OnReject(IFriendInviteContext invite)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.Logger.Instance.Log($"Invite {_invite.InviteId} rejected.");
            });
            _invite.OnAccept -= OnAccept;
            _invite.OnReject -= OnReject;
        }

        private void OnAccept(IFriendInviteContext invite)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.Logger.Instance.Log($"Invite {_invite.InviteId} accepted.");
            });
            _invite.OnAccept -= OnAccept;
            _invite.OnReject -= OnReject;
        }

        private IFriendInviteContext _invite;
        private void OnInviteSendResult(IFriendInviteContext c, InviteFriendResult r)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.Logger.Instance.Log($"{_invite.GetHashCode()}. Invite {_invite.InviteId} send result: {r}.");
                OnIviteSentResult(r);
            });
            _invite.OnInviteSendResult -= OnInviteSendResult;
        }
    }
}
