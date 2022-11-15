using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MessagePack;
using PestelLib.ServerShared;
using S;
using Mono.Options;


namespace ServerBenchmark
{
    class Program
    {

        private const string Path = "..\\..\\Data\\requests.bin";

        //private const string Url = "http://localhost/submarines_server2/api/ProcessTestCommand";
        //private const string Url = "http://localhost:5000/api/ProcessTestCommand";
        private const string Url = "http://localhost:5000/api/ProcessTestCommand";
        //private const string Url = "http://deepwatersphoton.planetcommander.ru:5000/api/ProcessTestCommand";

        static void Main(string[] args)
        {
            // these variables will be set when the command line is parsed
            string Url = null;
            var Path = "..\\..\\Data\\requests.bin";
            int threads = 8;
            int requestsPerBenchmark = 200;
            var shouldShowHelp = false;

            // thses are the available options, not that they set the variables
            var options = new OptionSet {
                { "p|path=", "Path to requests.bin file", (string n) => Path = n },
                { "t|threads=", "Thread nubmer, 4 is default.", (int r) => threads = r },
                { "r|requests=", "Requests resend per thread, 200 is default", (int v) => requestsPerBenchmark = v },
                { "u|url=", "Server url, http://localhost:8888/api/ProcessTestCommand for example", (string n) => Url = n },
                { "h|help", "show this message and exit",h => shouldShowHelp = h != null }
            };

            options.Parse(args);

            if (string.IsNullOrEmpty(Url))
            {
                Console.WriteLine("Please specify URL for requests");
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            if (shouldShowHelp)
            {
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            Console.WriteLine(Directory.GetCurrentDirectory());

            var allRequests = LoadSavedRequests(Path);

            DateTime _startTimestamp = DateTime.Now;

            // One event is used for each Fibonacci object.
            ManualResetEvent[] doneEvents = new ManualResetEvent[threads];
            Benchmark[] fibArray = new Benchmark[threads];

            // Configure and start threads using ThreadPool.
            Console.WriteLine("launching {0} tasks...", threads);
            for (int i = 0; i < threads; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                var f = new Benchmark(Url, requestsPerBenchmark, allRequests, doneEvents[i]);
                fibArray[i] = f;
                var thread = new Thread(f.Start);
                thread.Start(i);
                //ThreadPool.QueueUserWorkItem(f.Start, i);
            }

            // Wait for all threads in pool to calculate.
            WaitHandle.WaitAll(doneEvents);

            TimeSpan spentTime = DateTime.Now - _startTimestamp;

            // Display the results.
            var totalRequestAmount = 0;
            for (int i = 0; i < threads; i++)
            {
                Benchmark f = fibArray[i];
                totalRequestAmount += f.RequestCount;
            }

            Console.WriteLine(totalRequestAmount + " were completed in " + spentTime);
            Console.WriteLine("Request per second: " + (totalRequestAmount / spentTime.TotalSeconds));

            Console.ReadKey();
        }

        static private List<byte[]> LoadSavedRequests(string Path)
        {
            if (File.Exists(Path))
            {
                var bytes = File.ReadAllBytes(Path);
                var allRequests = MessagePackSerializer.Deserialize<BenchmarkRequest>(bytes);
                Console.WriteLine("Request queue count: " + allRequests.SerializedRequest.Count);
                return allRequests.SerializedRequest;
            }
            return null;
        }
    }
}
