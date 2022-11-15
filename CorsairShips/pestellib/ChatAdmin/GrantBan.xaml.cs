using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PestelLib.ChatCommon;

namespace ChatAdmin
{

    /// <summary>
    /// Interaction logic for GrantBan.xaml
    /// </summary>
    public partial class GrantBan : Window
    {
        public int Seconds { get; private set; }
        public SendTo UserToBan { get; private set; }

        public GrantBan(List<ClientInfo> users)
        {
            InitializeComponent();

            tbPeriod.TextChanged += OnPeriodTextChanged;
            btn1m.Click += (s, e) => AddPeriod(60);
            btn5m.Click += (s, e) => AddPeriod(300);
            btn30m.Click += (s, e) => AddPeriod(1800);
            btn1h.Click += (s, e) => AddPeriod(3600);
            btn1d.Click += (s,e) => AddPeriod(86400);
            btn1w.Click += (s, e) => AddPeriod(604800);
            cbUsers.SelectionChanged += OnCbUsersSelectionChanged;
            btnCancel.Click += (s, e) => Close();

            foreach (var clientInfo in users)
            {
                cbUsers.Items.Add(new SendTo() { Client = clientInfo });
            }

            btnOk.Click += OnBtnOkClick;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            UpdateOkButton();
        }

        private void OnBtnOkClick(object sender, RoutedEventArgs routedEventArgs)
        {
            DialogResult = true;
            Close();
        }

        private void OnCbUsersSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if(selectionChangedEventArgs.AddedItems == null || selectionChangedEventArgs.AddedItems.Count < 1)
                return;

            UserToBan = (SendTo)selectionChangedEventArgs.AddedItems[0];
            UpdateOkButton();
        }

        private void UpdateOkButton()
        {
            btnOk.IsEnabled = Seconds > 0 && UserToBan != null;
        }

        private void OnPeriodTextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            if (!int.TryParse(tbPeriod.Text, out var period))
            {
                tbPeriod.Text = Seconds.ToString();
            }
            else
                Seconds = period;

            UpdateOkButton();
        }

        private void AddPeriod(int seconds)
        {
            Seconds += seconds;

            var ts = TimeSpan.FromSeconds(Seconds);
            var d = (int) ts.Days;
            var h = (int) ts.Hours;
            var m = (int) ts.Minutes;
            var s = (int) ts.Seconds;
            var humanReadable = "";
            if (d > 0)
            {
                humanReadable += $"{d} day(s)";
            }

            if (h > 0)
            {
                humanReadable += $"{h} hour(s)";
            }

            if (m > 0)
            {
                humanReadable += $"{m} min(s)";
            }

            if (s > 0 || humanReadable.Length == 0)
            {
                humanReadable += $"{s} sec(s)";
            }

            tbPeriod.Text = Seconds.ToString();

            lblHumanPeriod.Content = humanReadable;
        }
    }
}
