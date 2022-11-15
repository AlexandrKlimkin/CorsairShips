using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServerShared.Sources;
using PestelLib.ServerCommon.Extensions;
using RabbitMQ.Client.Exceptions;

namespace PestelLib.ServerCommon.MessageQueue
{
    public class RabbitWorkerQueue : IWorkerQueue
    {
        public bool CanSend { get; }
        public bool CanReceive { get;}
        public bool Alive => _connection != null && (CanSend || CanReceive && _worker.Alive);

        public void SendWork(byte[] data)
        {
            if (!CanSend)
                throw new InvalidOperationException("Send is disallowed.");
            var buff = _data.Value.GetBuffer(data.Length);
            Buffer.BlockCopy(data, 0, buff, 0, data.Length);
            _channel.BasicPublish("", _appId, _publishProperties, new ReadOnlyMemory<byte>(buff, 0, data.Length));

            // Возможно стоит добавить подтверждение достаки (https://www.rabbitmq.com/confirms.html#publisher-confirms)
            // Они нужны в ситуациях когда рэббит сервер принял сообщение, но упал до того как заперсистил его. Думаю очень редкий кейс.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="appId"></param>
        /// <param name="send"></param>
        /// <param name="worker"></param>
        /// <exception cref="BrokerUnreachableException">нет соединения с рэббитом</exception>
        public RabbitWorkerQueue(string host, string appId, bool send, IWorker worker)
        {
            CanSend = send;
            CanReceive = worker != null;
            _appId = appId;
            _worker = worker;
            var factory = new ConnectionFactory { HostName = host, DispatchConsumersAsync = true};
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _publishProperties = _channel.CreateBasicProperties();
            _publishProperties.Persistent = true;
            _channel.QueueDeclare(_appId, true, false, false);
            if (CanReceive)
            {
                _receiver = new AsyncEventingBasicConsumer(_channel);
                _receiver.Received += OnMessage;
                _channel.BasicConsume(_appId, false, _receiver);
            }
        }

        private async Task OnMessage(object sender, BasicDeliverEventArgs args)
        {
            try
            {
                if (!_worker.Alive)
                {
                    return;
                }

                var result = false;
                byte[] message = null;
                try
                {
                    message = args.Body.ToArray();
                    result = await _worker.ProcessWork(message);
                }
                catch (Exception e)
                {
                    Log.Error($"Process incoming message error. message={message.ToHex()}, error={e}");
                }

                if (result)
                    _channel.BasicAck(args.DeliveryTag, true);
                else
                    _channel.BasicReject(args.DeliveryTag, true);
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _receiver.Received -= OnMessage;
            _connection = null;
            _channel = null;
        }

        private IConnection _connection;
        private IModel _channel;
        private readonly string _appId;
        private readonly IWorker _worker;
        private readonly AsyncEventingBasicConsumer _receiver;
        private readonly IBasicProperties _publishProperties;
        private readonly ThreadLocal<GrowingBuffer<byte>> _data = new ThreadLocal<GrowingBuffer<byte>>(() => new GrowingBuffer<byte>(100));
        private static readonly ILog Log = LogManager.GetLogger(typeof(RabbitWorkerQueue));
    }
}
