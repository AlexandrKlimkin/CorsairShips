using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServerShared.Sources;
using PestelLib.ServerCommon.Extensions;
using log4net;
using RabbitMQ.Client.Exceptions;

namespace PestelLib.ServerCommon.MessageQueue
{
    public class RabbitBroadcastQueue : IBroadcastQueue, IDisposable
    {
        public event Action<byte[]> OnIncomingMessage = _ => { };

        /// <exception cref="BrokerUnreachableException">нет соединения с рэббитом</exception>
        public RabbitBroadcastQueue(string host, string exchangeName, string appId)
        {
            var id = Guid.NewGuid();
            _host = host;
            _instanceId = id.ToByteArray();
            _queueName = $"{appId}_{id}";
            _broadcastExchangeName = exchangeName;
            CreateConsumer();
            CreatePublisher();
        }

        public void SendMessage(byte[] data)
        {
            try
            {
                var sz = data.Length + _instanceId.Length;
                var buff = _data.Value.GetBuffer(sz);
                Buffer.BlockCopy(_instanceId, 0, buff, 0, _instanceId.Length);
                Buffer.BlockCopy(data, 0, buff, _instanceId.Length, data.Length);
                lock (_channelPublish)
                {
                    _channelPublish.BasicPublish(_broadcastExchangeName, "",
                        body: new ReadOnlyMemory<byte>(buff, 0, sz));
                }
            }
            catch (AlreadyClosedException e)
            {
                Log.Error(e);
                CreatePublisher();
            }
        }

        public void Dispose()
        {
            _channelConsume?.Dispose();
            _channelPublish?.Dispose();
            _connectionConsume?.Dispose();
            _connectionPublish?.Dispose();
            _receiver.Received -= OnMessage;
            _connectionConsume = null;
            _connectionPublish = null;
            _channelConsume = null;
            _channelPublish = null;
        }

        private void CreateConsumer()
        {
            if (_receiver != null)
                _receiver.Received -= OnMessage;

            var factory = new ConnectionFactory { HostName = _host };
            _connectionConsume = factory.CreateConnection();
            _channelConsume = _connectionConsume.CreateModel();
            _channelConsume.ExchangeDeclare(_broadcastExchangeName, ExchangeType.Fanout);
            var consumeQueueName = _queueName + "Consume";
            _channelConsume.QueueDeclare(consumeQueueName);
            _channelConsume.QueueBind(consumeQueueName, _broadcastExchangeName, "");
            _receiver = new EventingBasicConsumer(_channelConsume);
            _receiver.Received += OnMessage;
            _channelConsume.BasicConsume(consumeQueueName, true, _receiver);
        }

        private void CreatePublisher()
        {
            var factory = new ConnectionFactory { HostName = _host };
            _connectionPublish = factory.CreateConnection();
            _channelPublish = _connectionPublish.CreateModel();
            var publishQueueName = _queueName + "Publish";
            _channelPublish.QueueDeclare(publishQueueName);
        }

        private void OnMessage(object sender, BasicDeliverEventArgs args)
        {
            var id = args.Body.Slice(0, _instanceId.Length);
            if(_instanceId.AsSpan().SequenceEqual(id.Span))
                return;
            byte[] message = null;
            try
            {
                message = args.Body.Slice(_instanceId.Length).ToArray();
                OnIncomingMessage(message);
            }
            catch (Exception e)
            {
                Log.Error($"Process incoming message error. message={message.ToHex()}, error={e}");
            }
        }

        private IConnection _connectionConsume;
        private IConnection _connectionPublish;
        private IModel _channelConsume;
        private IModel _channelPublish;
        private EventingBasicConsumer _receiver;
        private readonly string _host;
        private readonly string _queueName;
        private readonly byte[] _instanceId;
        private readonly string _broadcastExchangeName;
        private readonly ThreadLocal<GrowingBuffer<byte>> _data = new ThreadLocal<GrowingBuffer<byte>>(() => new GrowingBuffer<byte>(100));
        private static readonly ILog Log = LogManager.GetLogger(typeof(RabbitBroadcastQueue));
    }
}