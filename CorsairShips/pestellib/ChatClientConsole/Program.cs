using System;
using System.Collections.Generic;
using System.Threading;
using PestelLib.ChatClient;
using PestelLib.ServerCommon;

namespace ChatClientConsole
{
    class ClientContext
    {
        public ConsoleClient Client;
        public Thread UpdateThread;
    }

    class Program
    {
        private static int _count = 0;
        private static List<ClientContext> clients = new List<ClientContext>();
        static void Main(string[] args)
        {
            Log.Init();

            //var thread = new Thread(UpdateLoopSingleThread);
            //thread.Start();

            while (true)
            {
                var command = Console.ReadLine();
                var parts = command.Split(' ');
                if (parts[0] == "connect")
                {
                    var count = int.Parse(parts[1]);
                    var host = parts[2];
                    var port = int.Parse(parts[3]);
                    for (var i = 0; i < count; ++i)
                    {
                        var c = new ConsoleClient();
                        c.Start();
                        c.Init($"User{_count++}", host, port, false, Guid.NewGuid());
                        c.Connect("test channel", Guid.NewGuid());
                        var th = new Thread(UpdateLoop);
                        var ctx = new ClientContext()
                        {
                            Client = c,
                            UpdateThread = th
                        };
                        lock (clients)
                        {
                            clients.Add(ctx);
                        }
                        th.Start(ctx);
                    }
                }
                else if (parts[0] == "reconnect")
                {
                    lock (clients)
                    {
                        var amount = int.Parse(parts[1]);
                        foreach (var client in clients)
                        {
                            client.Client.Disconnect();
                            client.Client.Connect("test channel", Guid.Empty);
                        }
                    }
                }
            }
        }

        static void UpdateLoopSingleThread()
        {
            while (true)
            {
                lock (clients)
                {
                    foreach (var client in clients)
                    {
                        client.Client.Update();
                    }
                }
            }
        }

        static void UpdateLoop(object o)
        {
            var ctx = (ClientContext) o;
            while (true)
            {
                ctx.Client.Update();
                Thread.Sleep(20);
            }
        }
    }
}
