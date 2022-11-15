using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using MessageClient;
using MessagePack;
using MessageServer.Server.Tcp;
using MessageServer.Sources;
using PestelLib.ServerCommon.MessageQueue;
using PestelLib.UniversalSerializer;

namespace ProxyServerApp
{
    public static class ProxyCode
    {
        public const uint Success = 0;
        public const uint ServiceNotFound = 1;
        public const uint ServiceIsDown = 2;
    }

    [MessagePackObject()]
    public class ProxyServerRequest
    {
        [Key(0)]
        public int ServiceType;
        [Key(1)]
        public byte[] Data;
    }

    [MessagePackObject()]
    public class ProxyServerResponse
    {
        [Key(0)]
        public int ServiceType;
        [Key(1)]
        public uint ProxyCode;
    }

    class ProxyServer
    {
        public ProxyServer()
        {
            _config = ProxyServerConfigCache.Get();
            foreach (var kv in _config.Services)
            {
                var queue = MessageQueueFactory.Instance.CreateBroadcastQueue(_config.MessageQueueConnectionString, kv.Value);
                var serviceType = kv.Key;
                IService service = new MessageQueueService(queue);
                _registeredServices[kv.Key] = service;
                service.OnAnswer += async (sender, data) =>
                {
                    SendMessageTo(serviceType, sender, data);
                };
            }

            _messageSender = _messageProvider = new TcpMessageProvider(_config.Port);
            _messageProvider.Start();
            _serializer = new BinaryMessagePackSerializer();
            _dispatcher = new Dispatcher(this, _serializer);
            _messageServer = new MessageServer.Server.MessageServer(_messageProvider, _dispatcher);
        }

        private void SendMessageTo(int serviceType, int target, byte[] data)
        {
            var message = new ProxyServerRequest();
            message.ServiceType = serviceType;
            message.Data = data;
            var messageData = _serializer.Serialize(message);
            _messageSender.Notify(target, 1, messageData);
        }

        private Task<byte> Ping(int sender, byte data)
        {
            return Task.FromResult((byte)0);
        }

        private async Task<ProxyServerResponse> ProcessServiceMessage(int sender, ProxyServerRequest request)
        {
            if (!_registeredServices.TryGetValue(request.ServiceType, out var service))
            {
                return new ProxyServerResponse()
                {
                    ProxyCode = ProxyCode.ServiceNotFound
                };
            }

            var sent = await service.Process(sender, request.Data);
            if (sent)
                return new ProxyServerResponse()
                {
                    ProxyCode = ProxyCode.Success
                };

            return new ProxyServerResponse()
            {
                ProxyCode = ProxyCode.ServiceIsDown
            };
        }

        private IMessageSender _messageSender;
        private TcpMessageProvider _messageProvider;
        private ProxyServerConfig _config;
        private Dictionary<int, IService> _registeredServices;
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProxyServer));
        private Dispatcher _dispatcher;
        private MessageServer.Server.MessageServer _messageServer;
        private ISerializer _serializer;

        class Dispatcher : MtMessageDispatcher
        {
            public Dispatcher(ProxyServer server, ISerializer serializer)
            {
                _server = server;
                _serializer = serializer;

                RegisterHandler(0, new AsyncMessageHandler<byte, byte>(_serializer, _server.Ping));
                RegisterHandler(1, new AsyncMessageHandler<ProxyServerRequest, ProxyServerResponse>(_serializer, _server.ProcessServiceMessage));
            }

            private ProxyServer _server;
            private ISerializer _serializer;
        }
    }
}
