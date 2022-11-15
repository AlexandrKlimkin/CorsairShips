using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ServerLogBenchmark
{
    class Program
    {
        public const int Iterations = 20000;
        public const int Threads = 12;
        public static readonly byte[] Data = Encoding.UTF8.GetBytes("{\"Game\":\"warplanes\",\"Errors\":[\"Definitions 'planesData' are different on client and server\"]}");

        static void Main(string[] args)
        {


            var sw = System.Diagnostics.Stopwatch.StartNew();
            A().Wait();
            Console.WriteLine("Elapsed time: " + sw.ElapsedMilliseconds);
            Console.WriteLine("Requests per second: " + (Iterations * Threads / sw.Elapsed.TotalSeconds));
            Console.ReadKey();
        }

        static async Task A()
        {
            List<Task> tasks = new List<Task>();
            for (var i = 0; i < Threads; i++)
            {
                tasks.Add(MainAsync());
            }

            await Task.WhenAll(tasks);
        }


        static async Task MainAsync()
        {
            var client = new HttpClient();
            
            for (var i = 0; i < Iterations; i++)
            {
                ByteArrayContent byteContent = new ByteArrayContent(Data);
                await client.PostAsync("http://localhost/logger/CountLogError.ashx", byteContent);
            }
        }
    }
}