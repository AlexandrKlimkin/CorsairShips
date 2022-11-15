using System;
using System.Text;
using System.Threading;
using log4net;
using MClient;
using MessageServer.Server.Tcp;
using MessageServer.Sources;
using PestelLib.ServerCommon;

namespace MServer
{
    class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            Log.Init();

            var messageProvider = new TcpMessageProvider(9001);
            messageProvider.Start();
            IMessageSender messageSender = messageProvider;
            var dispatcher = new MtMessageDispatcher();
            var answerHandler = new RegularMessageHandler();

            dispatcher.RegisterHandler(1, new RegularMessageHandler());

            var server = new MessageServer.Server.MessageServer(messageProvider, dispatcher);
            var timer = new Timer(PrintStats, messageProvider, 0, 10000);

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
            if (state is TcpMessageProvider p)
            {
                _log.Error($"BytesSent={p.Stats.BytesSent}");
                _log.Error($"BytesReceived={p.Stats.BytesReceived}");
                _log.Error($"ConnectionsTotal={p.Stats.ConnectionsTotal}");
                _log.Error($"ConnectionsCurrent={p.Stats.ConnectionsCurrent}");
                _log.Error($"MessagesCount={p.Stats.MessagesCount}");
                _log.Error($"AnswerCount={p.Stats.AnswerCount}");
                _log.Error($"RequestCount={p.Stats.RequestCount}");
                _log.Error($"NotifyCount={p.Stats.NotifyCount}");
            }
        }
    }
}
