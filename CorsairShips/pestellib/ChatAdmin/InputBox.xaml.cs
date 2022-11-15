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
using PestelLib.ChatClient;

namespace ChatAdmin
{
    /// <summary>
    /// Interaction logic for InputBox.xaml
    /// </summary>
    public partial class InputBox : Window
    {
        public string InputValue { get; private set; }

        public InputBox(string title, string content, string[] channels)
        {
            InitializeComponent();

            Title = title;
            lblContent.Content = content;

            //_chat.ListChannels();

            btnOk.Click += BtnOkOnClick;
            btnCancel.Click += BtnCancelOnClick;

            if (channels != null)
            {
                foreach (var channel in channels)
                {
                    tbValue.Items.Add(channel);
                }
            }

            ContentRendered += (sender, args) => tbValue.Focus();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            tbValue.KeyDown += TbValueOnKeyDown;
        }

        private void TbValueOnKeyDown(object o, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Return)
            {
                Apply();
            }
        }

        private void BtnCancelOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }

        private void Apply()
        {
            DialogResult = true;
            InputValue = tbValue.Text;
        }

        private void BtnOkOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            Apply();
        }
    }
}
