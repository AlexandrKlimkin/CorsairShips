using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Logging.Serilog;
using CommandLine;

namespace PackageManager
{
    class Program
    {
        public static Options options = new Options();

        static void Main(string[] args)
        {
            IEnumerable<Error> errors = null;
            Parser.Default.ParseArguments<Options>(args).WithParsed((o) => options = o).WithNotParsed((e) => errors = e);

            if (errors != null)
            {
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }

                Console.ReadLine();
                return;
            }

            BuildAvaloniaApp().Start<MainWindow>();
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug();
    }
}
