using System;
using System.Collections.Concurrent;

namespace PestelLib.ServerCommon.MessageQueue
{
    public class MessageQueueFactory
    {
        private static readonly Lazy<MessageQueueFactory> _instance = new Lazy<MessageQueueFactory>(() => new MessageQueueFactory());
        private static readonly ConcurrentDictionary<(Type,string), object> _cache = new ConcurrentDictionary<(Type, string), object>();
        public static MessageQueueFactory Instance => _instance.Value;

        private MessageQueueFactory()
        {
        }

        /// <summary>
        /// Достает из кеша или создает новый инстанс для бродкаст очереди сообщений.
        /// 
        /// connectionString:
        ///     Для RabbitMQ следующий формат:
        ///         "rabbitmq,localhost"
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="appId">должен уникально идентифицировать реализацию IBroadcastQueue</param>
        /// <returns></returns>
        public IBroadcastQueue CreateBroadcastQueue(string connectionString, string appId)
        {
            return
                CreateQueue(connectionString, appId, "client", c =>
                {
                    if (c.Type == "rabbitmq")
                    {
                        return new RabbitBroadcastQueue(c.ConnectionString, $"{c.AppId}_EX", c.AppId);
                    }

                    throw new Exception($"Unknown queue type '{c.Type}'.");
                });
        }

        public IWorkerQueue CreateWorkerQueuePublisher(string connectionString, string appId)
        {
            return 
            CreateQueue(connectionString, appId, "pub", c =>
            {
                if (c.Type == "rabbitmq")
                {
                    return new RabbitWorkerQueue(c.ConnectionString, c.AppId, true, null);
                }

                throw new Exception($"Unknown queue type '{c.Type}'.");
            });
        }
        /// <summary>
        /// Создает очередь на получение заданий из шины сообщений.
        /// Необходимо передать реализацию IWorker которая будет обрабатывать все сообщения.
        /// </summary>
        /// <param name="connectionString">"rabbitmq,localhost"</param>
        /// <param name="appId">должен уникально идентифицировать реализацию IWorker</param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public IWorkerQueue CreateWorkerQueueConsumer(string connectionString, string appId, IWorker worker)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));
            var ctx = CreateContext(connectionString, appId);
            if (ctx.Type == "rabbitmq")
            {
                return new RabbitWorkerQueue(ctx.ConnectionString, ctx.AppId, false, worker);
            }
            throw new Exception($"Unknown queue type '{ctx.Type}'.");
        }

        private T CreateQueue<T>(string connectionString, string appId, string keyPostfix, Func<ConstructContext, T> factory) where T: class
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));

            var key = connectionString + appId + keyPostfix;
            var kt = (typeof(T), key);
            if (_cache.TryGetValue(kt, out var result))
                return result as T;

            var ctx = CreateContext(connectionString, appId);
            return _cache.GetOrAdd(kt, (c) => factory(ctx)) as T;
        }

        private ConstructContext CreateContext(string connectionString, string appId)
        {
            var sepIdx = connectionString.IndexOf(",", StringComparison.Ordinal);
            if (sepIdx < 0)
                throw new Exception($"Unknown queue type.");
            var type = connectionString.Substring(0, sepIdx).ToLower();
            var queueConn = connectionString.Substring(sepIdx + 1);

            return new ConstructContext()
            {
                Type = type,
                ConnectionString = queueConn,
                AppId = appId
            };
        }

        private class ConstructContext
        {
            public string Type;
            public string ConnectionString;
            public string AppId;
        }
    }
}
