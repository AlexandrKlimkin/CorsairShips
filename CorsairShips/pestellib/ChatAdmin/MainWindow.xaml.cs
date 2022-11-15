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
using System.Windows.Navigation;
using System.Windows.Shapes;
using PestelLib.ChatClient;
using PestelLib.ChatCommon;
using PestelLib.ServerCommon;

namespace ChatAdmin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Connection _connection;
        private List<ChatClientAdmin> _chats = new List<ChatClientAdmin>();
        private string[] Channels;
        public MainWindow()
        {
            InitializeComponent();

            Log.Init();
            ContentRendered += OnContentRendered;
            btnJoinRoom.Click += BtnJoinRoomOnClick;
        }

        private void JoinChannel(string channelName)
        {
            var tab = new TabItem();
            tab.Header = channelName;
            var room = new Room();
            tab.Content = room;
            tabRooms.Items.Add(tab);
            tabRooms.SelectedIndex = tabRooms.Items.Count - 1;
            var userId = _chats.Any() ? Guid.NewGuid().ToString() : _connection.Secret;
            _chats.Add(new ChatClientAdmin(_connection.UserName, _connection.ServerAddr, _connection.ServerPort, channelName, userId, room, _connection.Encrypted));
            _chats[0].ListChannels(channels => Channels = channels);
        }

        private void BtnJoinRoomOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var tcs = new TaskCompletionSource<string[]>();
            if (_chats.Count > 0)
            {
                _chats[0].ListChannels(channels => tcs.SetResult(channels));
            }

            var reloadChannels = tcs.Task.ContinueWith(_ => Channels = _.Result);
            var maxDelay = Task.Delay(500);

            Task.WhenAny(reloadChannels, maxDelay).ContinueWith(task =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var d = new InputBox("Join room", "Enter room name", Channels);
                    d.Owner = this;
                    var r = d.ShowDialog();
                    if (!r.HasValue || !r.Value)
                        return;

                    JoinChannel(d.InputValue);
                });
            });
        }

        private void OnContentRendered(object sender, EventArgs eventArgs)
        {
            if (_connection == null)
            {
                var c = new Connections();
                c.Owner = this;
                var r = c.ShowDialog();
                if (!r.HasValue || !r.Value)
                {
                    Close();
                }
                else
                {
                    _connection = c.SelectedConnection;
                    Title += $" :: {_connection.Name}";
                    JoinChannel("_admins");
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (var chatClientAdmin in _chats)
            {
                chatClientAdmin.Dispose();
            }
            base.OnClosing(e);
        }
    }
}
