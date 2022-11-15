using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using log4net;

namespace FriendsClientWpf
{
    public partial class MainWindow
    {
        public class Logger
        {
            private static ILog _log = LogManager.GetLogger(typeof(Logger));
            private readonly TextBox _tb;
            public static Logger Instance { get; private set; }

            public static void CreateLogger(TextBox tb)
            {
                new Logger(tb);
            }

            private Logger(TextBox tb)
            {
                _tb = tb;
                Instance = this;
            }

            public void Log(string message)
            {
                var dt = DateTime.Now.ToString("HH:mm:ss.fff");
                Application.Current.Dispatcher.Invoke(() => _tb.Text = $"{dt} {message}\n{_tb.Text}");
                _log.Debug(message);
            }
        }
    }
}
