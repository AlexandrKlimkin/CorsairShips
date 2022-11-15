using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Lidgren.Network;
using PestelLib.ServerShared;
using PestelLib.ChatServer;
using PestelLib.ChatCommon;
using Newtonsoft.Json;

namespace PestelLib.ChatServer
{
    public class ChatClientStatistics
    {
        public long MessagesReceivedCount;
        public long MessagesSentCount;
        public long MessagesReceivedLen;
        public long MessagesSentLen;

        private DateTime _startTime = DateTime.UtcNow;

        public void MessageSent(string message)
        {
            var secs = (DateTime.UtcNow - _startTime).TotalSeconds;
            Interlocked.Increment(ref MessagesSentCount);
            Interlocked.Exchange(ref MessagesSentLen, MessagesSentLen + message.Length);
        }

        public void MessageReceived(string message)
        {
            var secs = (DateTime.UtcNow - _startTime).TotalSeconds;
            Interlocked.Increment(ref MessagesReceivedCount);
            Interlocked.Exchange(ref MessagesReceivedLen, MessagesReceivedLen + message.Length);
        }

        public string ToPrettyString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"MessagesReceivedCount: {MessagesReceivedCount}");
            sb.AppendLine($"MessagesReceivedLen: {MessagesReceivedLen}");
            sb.AppendLine($"MessagesSentCount: {MessagesSentCount}");
            sb.AppendLine($"MessagesSentLen: {MessagesSentLen}");
            return sb.ToString();
        }

        public static ChatClientStatistics operator +(ChatClientStatistics f, ChatClientStatistics s)
        {
            return new ChatClientStatistics()
            {
                MessagesReceivedCount = f.MessagesReceivedCount + s.MessagesReceivedCount,
                MessagesReceivedLen = f.MessagesReceivedLen + s.MessagesReceivedLen,
                MessagesSentCount = f.MessagesSentCount + s.MessagesSentCount,
                MessagesSentLen = f.MessagesSentLen + s.MessagesSentLen,
            };
        }
    }
    public class ChatClient : IDisposable
    {
        private NetClient _client;
        private ClientInfo _clientInfo;
        private string _channel;
        private List<ChatProtocol> _pendings = new List<ChatProtocol>();

        public ChatClientStatistics Statistics { get; private set; }
        public bool DumpProtocol { get; set; }

        public ChatClient(string host, int port, Guid userId, string name)
        {
            Statistics = new ChatClientStatistics();
            DumpProtocol = false;
            _clientInfo = new ClientInfo();
#pragma warning disable 612
            _clientInfo.Id = userId.ToByteArray();
#pragma warning restore 612
            _clientInfo.Name = name;
            NetPeerConfiguration config = new NetPeerConfiguration(Consts.ChatAppId);
            config.AutoFlushSendQueue = true;
            _client = new NetClient(config);

            var c = new SynchronizationContext();
            _client.RegisterReceivedCallback(new SendOrPostCallback(GotMessage), c);

            _client.Start();
            _client.Connect(host, port);
        }

        public void Login(string channel)
        {
            _channel = channel;
            SendCommand(new ChatProtocol()
            {
                CommandType = CommandType.ClientLoginInform,
            });
        }

        public void EnterChannel(string channel)
        {
            _channel = channel;
            SendCommand(new ChatProtocol()
            {
                CommandType = CommandType.JoinChannel
            });
        }

        public void SendChatMessage(string message)
        {
            SendCommand(new ChatProtocol()
            {
                CommandType = CommandType.Message,
                Body = message
            });
        }

        public void LoadHistory()
        {
            SendCommand(new ChatProtocol()
            {
                CommandType = CommandType.SendHistory
            });
        }

        public void Dispose()
        {
            _client.Shutdown("Shutdown on destroy");
        }

        private void SendCommand(ChatProtocol cmd)
        {
            cmd.ChannelName = _channel;
            cmd.ClientInfo = _clientInfo;
            lock(_pendings)
                _pendings.Add(cmd);

            ProcessPendings();
        }

        private void ProcessPendings()
        {
            if (_client.ConnectionStatus != NetConnectionStatus.Connected)
                return;

            lock (_pendings)
            {
                foreach (var cmd in _pendings)
                {
                    var message = _client.CreateMessage();
                    //message.Write(CommonSerializer.Serialize(cmd));
                    var netData = Newtonsoft.Json.JsonConvert.SerializeObject(cmd, Formatting.None, Consts.JsonSerializerSettings);
                    Statistics.MessageSent(netData);
                    message.Write(netData);
                    _client.SendMessage(message, Consts.DefaultDeliveryMethod);
                }
                _pendings.Clear();
            }
        }

        private void GotMessage(object peer)
        {
            NetIncomingMessage im;
            while ((im = _client.ReadMessage()) != null)
            {
                // handle incoming message
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        string text = im.ReadString();
                        Console.WriteLine(text);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                        string reason = im.ReadString();
                        Console.WriteLine(status.ToString() + ": " + reason);
                        ProcessPendings();
                        break;
                    case NetIncomingMessageType.Data:
                        //var message = CommonSerializer.Deserialize<ChatProtocol>(im.ReadBytes(im.LengthBytes));
                        var message = im.ReadString();
                        Statistics.MessageReceived(message);
                        var command = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatProtocol>(message, Consts.JsonSerializerSettings);
                        if(DumpProtocol)
                            Console.WriteLine(message);
                        break;
                    default:
                        Console.WriteLine("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes");
                        break;
                }
                _client.Recycle(im);
            }
        }
    }
}