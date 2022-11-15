using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using PestelLib.ServerCommon;

namespace AppWatchCli
{
    class ProcessWatcher
    {
        private static ILog Log = LogManager.GetLogger(typeof(ProcessWatcher));

        private readonly Process _process;
        private readonly ProcessStartInfo _processStartInfo;
        private readonly string _name;

        public ProcessWatcher(string appPath, string args)
        {
            _name = appPath + string.Join(" ", args);
            var wd = Path.GetDirectoryName(appPath);
            _processStartInfo = new ProcessStartInfo()
            {
                FileName = appPath,
                Arguments = args,
                WorkingDirectory = wd,
            };

            _process = new Process();
            _process.StartInfo = _processStartInfo;
            _process.EnableRaisingEvents = true;
            _process.Exited += ProcessOnExited;

            Start();
        }

        private void Start()
        {
            var r = _process.Start();
            Log.Info($"'{_name}' started. WD={_processStartInfo.WorkingDirectory}");
        }

        private void ProcessOnExited(object sender, EventArgs eventArgs)
        {
            var msg = $"Process '{_name}' exited with code {_process.ExitCode}";
            if (_process.ExitCode == 0)
                Log.Info(msg);
            else
                Log.Error(msg);
            Start();
        }
    }

    class Program
    {
        private static ILog Log = LogManager.GetLogger(typeof(Program));
        private static List<ProcessWatcher> _processWatchers = new List<ProcessWatcher>();
        private static Regex _rgxAppName = new Regex("\\s*^\\\"(.*?)\\\"");

        static void Main(string[] args)
        {
            PestelLib.ServerCommon.Log.Init();

            while (true)
            {
                var command = Console.ReadLine();
                var sepPos = command.IndexOf(' ');
                ProcessWatcher pw;
                try
                {
                    var m = _rgxAppName.Match(command);
                    if (!m.Success && sepPos < 0)
                        pw = new ProcessWatcher(command, "");
                    else
                    {
                        string app;
                        var appArgs = "";
                        if (m.Success)
                        {
                            var g = m.Groups[1];
                            sepPos = g.Index + g.Length + 1;
                            app = m.Groups[1].Value;
                        }
                        else
                        {
                            app = command.Substring(0, sepPos);
                        }
                        ++sepPos;
                        if(sepPos < command.Length)
                            appArgs = command.Substring(sepPos + 1);
                        pw = new ProcessWatcher(app, appArgs);
                    }
                    _processWatchers.Add(pw);
                }
                catch (Exception e)
                {
                    Log.Error($"Cant start app '{command}'", e);
                }
            }
        }
    }
}
