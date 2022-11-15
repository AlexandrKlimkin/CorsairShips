using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommandLine;
using log4net;

namespace FriendsClientWpf
{
    public class Options
    {
        [Option("config", Default = "FriendsClientWpf.json")]
        public string ConfigPath { get; set; }

        [Option("disable_instancing")]
        public bool DisableInstancing { get; set; }
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static ILog Log = LogManager.GetLogger(typeof(App));
        public Options Options { get; private set; }

        void _startup(object sender, StartupEventArgs e)
        {
            PestelLib.ServerCommon.Log.Init();

            IEnumerable<Error> errors = null;
            Parser.Default.ParseArguments<Options>(e.Args).WithParsed((o) =>
            {
                Application.Current.Properties.Add("Options", o);

            } ).WithNotParsed((er) => errors = er);

            if (errors != null)
            {
                foreach (var error in errors)
                {
                    Log.Error(error);
                }
            }
        }
    }
}
