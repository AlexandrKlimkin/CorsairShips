using System;
using System.Collections.Concurrent;
using ChatCommon;
using log4net;
using MessageServer.Sources;
using MessageServer.Sources.Tcp;
using PestelLib.ChatCommon;
using ServerShared;
using UnityDI;

namespace ChatClient.Transport
{
    class ChatClientTransportTcp : IChatClientTransport
    {
        public event Action Disconnect = () => { };
        public bool IsConnected => _provider.IsConnected;

        public ChatClientTransportTcp(string host, int port, IChatProtocolSerializer serializer, bool useEncryption)
        {
            _serializer = serializer;
            var updateProvider = ContainerHolder.Container.Resolve<IUpdateProvider>();
            UniversalAssert.IsTrue(updateProvider != null);
            _sender = _provider = new TcpClientMessageProvider(host, port, updateProvider);
            _provider.OnMessage += OnMessage;
            _provider.OnConnected += OnConnected;
            _provider.OnDisconnected += OnDisconnected;
            _provider.OnConnectionError += OnConnectionError;
            _transform = useEncryption ? new ChatMessageTransform() : null;
        }

        public void Start(string addr, int port)
        {
            _provider.Start();
        }

        public void Stop()
        {
            _provider.Stop();
        }

        public void Close()
        {
            Stop();
        }

        public bool SendMessage(ChatProtocol message)
        {
            var data = _serializer.Serialize(message);
            if (_transform != null)
                data = _transform.CommitTransform(data);
            _sender.Notify(0, 1, data);
            return true;
        }

        public ChatProtocol ReadMessage()
        {
            if (_messageQueue.IsEmpty)
                return null;
            if (!_messageQueue.TryDequeue(out var messge))
                return null;

            return messge;
        }

        private void OnConnectionError()
        {
            Log.Error("Chat connection error");
        }

        private void OnDisconnected()
        {
        }

        private void OnConnected()
        {
        }

        private void OnMessage(Message arg1, IAnswerContext arg2)
        {
            if (arg1.Type != 1)
            {
                UniversalAssert.IsTrue(false);
                return;
            }

            try
            {
                if (_transform != null)
                    arg1.Data = _transform.RollbackTransform(arg1.Data);
                var message = _serializer.Deserialize(arg1.Data);
                _messageQueue.Enqueue(message);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private ConcurrentQueue<ChatProtocol> _messageQueue = new ConcurrentQueue<ChatProtocol>();
        private TcpClientMessageProvider _provider;
        private IMessageSender _sender;
        private ChatMessageTransform _transform;
        private readonly IChatProtocolSerializer _serializer;
        private static readonly ILog Log = LogManager.GetLogger(typeof(ChatClientTransportTcp));
    }
}
