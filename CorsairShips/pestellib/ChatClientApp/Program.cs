using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PestelLib.ChatServer;
using PestelLib.ChatCommon;
using System.Threading;

namespace ChatClientApp
{
    class ChatBot
    {
        static Random random = new Random();
        ChatClient _client;
        string _userName;
        bool _running = true;
        int _messageLen;
        int _messageDelay;
        Guid _id = Guid.NewGuid();

        public ChatClient Client => _client;
        public string Name => _userName;
        public ChatBot(string name, string channel, int messageDelay, int messageLen)
        {
            _userName = name;
            _messageLen = messageLen;
            _messageDelay = messageDelay;
            _client = new ChatClient("localhost", Consts.ChatPort, _id, name);
            _client.Login(channel);
            Task.Delay(messageDelay).ContinueWith((t) => WorkerLoop());
        }

        public void SendMessage(string message)
        {
            _client.SendChatMessage(message);
        }

        private string RandomMessage()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789АБВГДЕЁЖЗИКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
            return new string(Enumerable.Repeat(chars, _messageLen)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void WorkerLoop()
        {
            if (!_running) return;
            _client.SendChatMessage(RandomMessage());
            Task.Delay(_messageDelay).ContinueWith((t) => WorkerLoop());
        }
    }

    class Program
    {
        static ChatBot _currentBot;
        static List<ChatBot> _bots = new List<ChatBot>();
        static DateTime _startTime = DateTime.UtcNow;
        static Random _random = new Random();
        static void Main(string[] args)
        {
            while (true)
            {
                var message = Console.ReadLine();
                if (message.Length < 1)
                    continue;
                if (message[0] == ':')
                {
                    var pars = message.Substring(1).Split(' ');
                    ProcessCommand(pars[0], pars.Skip(1).ToArray());
                }
                else
                {
                    if (_currentBot == null)
                    {
                        Console.WriteLine("No selected bot");
                        continue;
                    }
                    _currentBot.SendMessage(message);
                }
            }
        }
        static int botCounter;
        static void ProcessCommand(string command, params string[] args)
        {
            switch (command)
            {
                case "create_bots":
                    var amount = int.Parse(args[0]);
                    var channel = args[1];
                    var floodPeriod = int.Parse(args[2]);
                    var messagelen = int.Parse(args[3]);
                    for (var i = 0; i < amount; i++)
                    {
                        _bots.Add(new ChatBot($"user{botCounter++}", channel, floodPeriod, messagelen));
                    }
                    break;
                case "join":
                    if (_currentBot == null)
                    {
                        Console.WriteLine("No selected bot");
                        break;
                    }
                    _currentBot.Client.EnterChannel(args[0]);
                    break;
                case "list_bots":
                    var sb = new StringBuilder();
                    sb.AppendLine("Bots list:");
                    foreach (var b in _bots)
                        sb.AppendLine(b.Name);
                    sb.AppendLine("Total " + _bots.Count);
                    Console.Write(sb.ToString());
                    break;
                case "select_bot":
                    var botName = args[0];
                    var bot = _bots.FirstOrDefault(b => b.Name == botName);
                    if (bot == null)
                    {
                        Console.WriteLine($"Bot '{botName}' not found");
                        break;
                    }
                    _currentBot = bot;
                    break;
                case "history":
                    if (_currentBot == null)
                    {
                        Console.WriteLine("No selected bot");
                        break;
                    }
                    _currentBot.Client.LoadHistory();
                    break;
                case "kill_bots":
                    foreach (var b in _bots) b.Client.Dispose();
                    _bots.Clear();
                    _currentBot = null;
                    break;
                case "dump":
                    if (_currentBot == null)
                    {
                        Console.WriteLine("No selected bot");
                        break;
                    }
                    _currentBot.Client.DumpProtocol = !_currentBot.Client.DumpProtocol;
                    break;
                case "stats":
                    var allStat = _bots.Aggregate(new ChatClientStatistics(), (acc, stat) => acc + stat.Client.Statistics);
                    Console.WriteLine($"Time: {(DateTime.UtcNow - _startTime).TotalSeconds}\nClients: {_bots.Count}\n" + allStat.ToPrettyString());
                    break;
            }
        }
    }
}
