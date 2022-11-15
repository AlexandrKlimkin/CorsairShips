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
using System.Windows.Shapes;
using PestelLib.ChatCommon;
using Newtonsoft.Json;
using System.IO;

namespace ChatAdmin
{
    /// <summary>
    /// Interaction logic for Connections.xaml
    /// </summary>
    public partial class Connections : Window
    {
        private const string ConnectionsFile = "connections.json";
        public Connection SelectedConnection { get; private set; }

        private readonly List<Connection> _connections;
        public Connections()
        {
            _connections = Load();
            InitializeComponent();
            if (_connections.Any())
            {
                _connections.Add(new Connection()
                {
                    Name = "New connection",
                    ServerAddr = "",
                    ServerPort = Consts.ChatPort,
                    Secret = Guid.NewGuid().ToString(),
                    UserName = "Admin",
                });
            }
            lbConnections.ItemsSource = _connections;
            lbConnections.SelectionChanged += LbConnectionsOnSelectionChanged;

            btnRandomId.Click += BtnRandomIdOnClick;
            btnConnect.Click += BtnConnectOnClick;

            Closing += (sender, args) => Save();
            ContentRendered += (sender, args) => lbConnections.SelectedIndex = 0;

            tbId.TextChanged += TbIdOnTextChanged;
            tbConnectionName.TextChanged += TbConnectionNameOnTextChanged;
            tbServerAddr.TextChanged += TbServerAddrOnTextChanged;
            tbServerPort.TextChanged += TbServerPortOnTextChanged;
            tbUserName.TextChanged += TbUserNameOnTextChanged;
            cbEncrypted.Checked += CbEncryptedOnChecked;

            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        private void CbEncryptedOnChecked(object o, RoutedEventArgs routedEventArgs)
        {
            SelectedConnection.Encrypted = cbEncrypted.IsChecked.HasValue && cbEncrypted.IsChecked.Value;
        }

        private void TbUserNameOnTextChanged(object o, TextChangedEventArgs textChangedEventArgs)
        {
            SelectedConnection.UserName = tbUserName.Text;
        }

        private void TbServerPortOnTextChanged(object o, TextChangedEventArgs textChangedEventArgs)
        {
            if(!int.TryParse(tbServerPort.Text, out var port))
                return;
            SelectedConnection.ServerPort = port;
        }

        private void TbServerAddrOnTextChanged(object o, TextChangedEventArgs textChangedEventArgs)
        {
            SelectedConnection.ServerAddr = tbServerAddr.Text;
        }

        private void TbIdOnTextChanged(object o, TextChangedEventArgs textChangedEventArgs)
        {
            SelectedConnection.Secret = tbId.Text;
        }

        private void TbConnectionNameOnTextChanged(object o, TextChangedEventArgs textChangedEventArgs)
        {
            SelectedConnection.Name = tbConnectionName.Text;
        }

        private void BtnConnectOnClick(object o, RoutedEventArgs routedEventArgs)
        {
            DialogResult = SelectedConnection != null;
            Close();
        }

        private void LbConnectionsOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var con = lbConnections.SelectedItem as Connection;
            SelectedConnection = con;
            if (con == null)
                return;
            tbId.Text = con.Secret.ToString();
            tbConnectionName.Text = con.Name;
            tbServerAddr.Text = con.ServerAddr;
            tbServerPort.Text = con.ServerPort.ToString();
            tbUserName.Text = con.UserName;
            cbEncrypted.IsChecked = con.Encrypted;
        }

        private void BtnRandomIdOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            tbId.Text = Guid.NewGuid().ToString();
        }

        private void Save()
        {
            var data = JsonConvert.SerializeObject(_connections.Where(_ => _.ServerAddr != ""));
            using (var f = new StreamWriter(File.Open(ConnectionsFile, FileMode.Create)))
            {
                f.Write(data);
            }
        }

        private List<Connection> Load()
        {
            if(!File.Exists(ConnectionsFile))
                return new List<Connection>();

            var json = File.ReadAllText(ConnectionsFile);
            return JsonConvert.DeserializeObject<List<Connection>>(json);
        }
    }
}
