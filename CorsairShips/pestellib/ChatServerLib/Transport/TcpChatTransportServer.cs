using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ChatCommon;
using PestelLib.ChatCommon;
using PestelLib.ChatServer;
using MessageServer.Server.Tcp;
using MessageServer.Sources;
using MessageServer.Sources.Tcp;

namespace ChatServer.Transport
{
    class ChatPingHandler : IMessageHandler
    {
        private static byte[] AnsData = new byte[] { 0 };
        public void Handle(int sender, byte[] data, int tag, IAnswerContext answerContext)
        {
            answerContext.Answer(0, AnsData);
        }

        public void Error(int tag)
        {
        }
    }
    public class TcpChatTransportServer : IChatServerTransport
    {
        public ChatServerTransportStats Stats { get; }
        public bool IsConnected => _started;

        private IMessageSender _sender;
        private TcpMessageProvider _provider;
        private MessageServer.Server.MessageServer _server;
        private ChatMessageDispatcher _dispatcher;
        private bool _started;
        private ChatMessageTransform _transform;
        public TcpChatTransportServer(int port, IChatProtocolSerializer serializer, bool useEncryption)
        {
            _serializer = serializer;
            _sender = _provider = new TcpMessageProvider(port);
            _dispatcher = new ChatMessageDispatcher(this);
            _server = new MessageServer.Server.MessageServer(_provider, _dispatcher);
            _transform = useEncryption ? new ChatMessageTransform() : null;
        }

        public void Start()
        {
            _provider.Start();
            _started = true;
        }

        public void Stop()
        {
            _provider.Stop();
            _started = false;
        }

        public ChatProtocol ReceiveMessage(int timeoutMillis, out ChatConnection connection)
        {
            connection = null;
            if (!SpinWait.SpinUntil(() => !_messageQueue.IsEmpty, timeoutMillis))
                return null;

            if (!_messageQueue.TryDequeue(out var msg))
                return null;

            connection = msg.Connection;
            return msg.Message;
        }

        public void SendTo(ChatProtocol message, params ChatConnection[] connections)
        {
            var senders = connections.Select(_ => _ as TcpConnection).Where(_ => _ != null).Select(_ => _.ConnectionId).ToArray();
            var data = _serializer.Serialize(message);
            if (_transform != null)
                data = _transform.CommitTransform(data);
            _sender.Notify(senders, MessageType, data);
        }

        public void AddFilter(IMessageFiler filter)
        {
            System.Diagnostics.Debug.Assert(!IsConnected, "Can't add filter in connected state.");
            _filters.Add(filter);
        }

        private void NewMessage(byte[] data, int connectId)
        {
            if (_transform != null)
                data = _transform.RollbackTransform(data);
            var message = _serializer.Deserialize(data);
            message.Time = DateTime.UtcNow;
            _messageQueue.Enqueue(new IncomingMessage()
            {
                Message = message,
                Connection = new TcpConnection(_provider, connectId)
            });
        }


        class ChatMessageHandler : IMessageHandler
        {
            public ChatMessageHandler(TcpChatTransportServer server)
            {
                _server = server;
            }

            public void Handle(int sender, byte[] data, int tag, IAnswerContext answerContext)
            {
                _server.NewMessage(data, sender);
            }

            public void Error(int tag)
            { }

            private TcpChatTransportServer _server;
        }

        class ChatMessageDispatcher : MtMessageDispatcher
        {
            public ChatMessageDispatcher(TcpChatTransportServer server)
            {
                RegisterHandler(0, new ChatPingHandler());
                RegisterHandler(MessageType, new ChatMessageHandler(server));
            }
        }

        class IncomingMessage
        {
            public ChatProtocol Message;
            public TcpConnection Connection;
        }

        private const int MessageType = 1;
        private List<IMessageFiler> _filters = new List<IMessageFiler>();
        private ConcurrentQueue<IncomingMessage> _messageQueue = new ConcurrentQueue<IncomingMessage>();
        private readonly IChatProtocolSerializer _serializer;
    }
}
