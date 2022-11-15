using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace BoltBotNet
{
    class Program
    {
        private static List<Process> processes = new List<Process>();

        static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            Task.WaitAll(StartBotInstances());
            Console.ReadKey();
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("exit");
            foreach (var process in processes)
            {
                process.Kill(true);
            }
        }

        static async Task StartBotInstances()
        {
            for (var i = 0; i < int.Parse(GetArg("-instances")); i++)
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = GetArg("-path"),
                        Arguments = string.Format("-batchmode -nographics -bot -master {0}", GetArg("-master")),
                        UseShellExecute = false,
                    }
                };

                processes.Add(process);
                process.Start();

                await Task.Delay(3000);
            }
        }

        public static string GetArg(params string[] names)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                foreach (var name in names)
                {
                    if (args[i] == name && args.Length > i + 1)
                    {
                        return args[i + 1];
                    }
                }
            }

            return null;
        }
    }
}
