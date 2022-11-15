using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MessageServer.Sources;
using MessageServer.Sources.Tcp;
using PestelLib.ServerCommon;
using ServerShared;

namespace MClient
{
    public static class Config
    {
        public static string MachineName;
        public static TimeSpan PrintDelay;
    }

    public static class Stats
    {
        public static long UpdateTime;
        public static long UpdateInterations;
    }

    public class UpdateProvider : IUpdateProvider
    {
        public event Action OnUpdate = () => { };
        private Timer _timer;
        private int _count;
        private Stopwatch _sw;

        public UpdateProvider()
        {
            _sw = new Stopwatch();
            _timer = new Timer(Callback, null, 0, 100);
        }

        private void Callback(object state)
        {
            var r = Interlocked.Increment(ref _count);
            try
            {
                if(r != 1)
                    return;
                _sw.Restart();
                OnUpdate();
                ++Stats.UpdateInterations;
                Stats.UpdateTime += _sw.ElapsedMilliseconds;
            }
            finally
            {
                Interlocked.Decrement(ref _count);
            }
        }
    }

    public class RegularMessageHandler : IMessageHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RegularMessageHandler));

        public void Error(int tag)
        {
        }

        public void Handle(int sender, byte[] data, int tag, IAnswerContext answerContext)
        {
            log.Debug($"New message from {sender}. sz={data.Length},tag={tag},can_answer={answerContext != null}.");
            var str = Encoding.UTF8.GetString(data);
            log.Debug($"Decoded message: {str}.");
            if (answerContext != null)
            {
                var ansBytes = Encoding.UTF8.GetBytes("OK!");
                answerContext.Answer(1, ansBytes);
                log.Debug("Answer sent.");
            }
        }
    }

    class BenchmarkClient
    {
        
        private readonly UpdateProvider _updateProvider;
        private readonly MtMessageDispatcher _dispatcher;
        private readonly IMessageHandler _answerHandler;
        public IMessageSender _messageSender { get; }
        private double _sendDelay = 0.5;
        private Stopwatch _sw = new Stopwatch();
        private Random _rnd = new Random();
        private readonly MessageServer.Sources.MessageClient _client;
        private int _id;

        public BenchmarkClient(int id, UpdateProvider updateProvider, MtMessageDispatcher dispatcher, IMessageHandler answerHandler)
        {
            _id = id;
            _updateProvider = updateProvider;
            _dispatcher = dispatcher;
            _answerHandler = answerHandler;

            var tcpProvider = new TcpClientMessageProvider("localhost", 9001, updateProvider);
            _client = new MessageServer.Sources.MessageClient(tcpProvider, dispatcher);
            tcpProvider.Start();
            _messageSender = tcpProvider;
            _sw.Start();

            _ = Update();
        }

        private async Task Update()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(_sendDelay));
                _messageSender.Request(0, 1, Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()), _answerHandler);
                _sendDelay = _rnd.NextDouble();
            }
        }
    }

    class Program
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(Program));
        static List<BenchmarkClient> _clients = new List<BenchmarkClient>();
        static void Main(string[] args)
        {
            Log.Init();

            
            UpdateProvider updateProvider = new UpdateProvider();
            MtMessageDispatcher messageDispatcher = new MtMessageDispatcher();
            messageDispatcher.RegisterHandler(1, new RegularMessageHandler());
            var answerHandler = new RegularMessageHandler();

            var tcpProvider = new TcpClientMessageProvider("localhost", 9001, updateProvider);
            IMessageSender messageSender = tcpProvider;
            MessageServer.Sources.MessageClient client = new MessageServer.Sources.MessageClient(tcpProvider, messageDispatcher);
            tcpProvider.Start();


            for (var i = 0; i < 500; ++i)
            {
                _clients.Add(new BenchmarkClient(i, updateProvider, messageDispatcher, answerHandler));
            }
            var timer = new Timer(PrintStats, _clients, 0, 10000);

            while (true)
            {
                var line = Console.ReadLine();
                if(line == "exit")
                    break;
                if (line.Contains("request"))
                {
                    messageSender.Request(0, 1, Encoding.UTF8.GetBytes(line), answerHandler);
                }
                else
                {
                    messageSender.Notify(0, 1, Encoding.UTF8.GetBytes(line));
                }
            }
        }

        private static void PrintStats(object state)
        {
            long BytesReceived = 0;
            long BytesSent = 0;
            long ConnectionsTotal = 0;
            long ConnectionsCurrent = 0;
            long NotifyCount = 0;
            long RequestCount = 0;
            long AnswerCount = 0;
            long MessagesCount = 0;
            foreach (var benchmarkClient in _clients)
            {
                if (benchmarkClient._messageSender is TcpClientMessageProvider p)
                {
                    BytesReceived += p.Stats.BytesReceived;
                    BytesSent += p.Stats.BytesSent;
                    ConnectionsTotal += p.Stats.ConnectionsTotal;
                    ConnectionsCurrent += p.Stats.ConnectionsCurrent;
                    NotifyCount += p.Stats.NotifyCount;
                    RequestCount += p.Stats.RequestCount;
                    AnswerCount += p.Stats.AnswerCount;
                    MessagesCount += p.Stats.MessagesCount;
                }
            }

            _Log.Warn($"BytesSent={BytesSent}");
            _Log.Warn($"BytesReceived={BytesReceived}");
            _Log.Warn($"MessagesCount={MessagesCount}");
            _Log.Warn($"NotifyCount={NotifyCount}");
            _Log.Warn($"RequestCount={RequestCount}");
            _Log.Warn($"AnswerCount={AnswerCount}");
        }
    }
}
