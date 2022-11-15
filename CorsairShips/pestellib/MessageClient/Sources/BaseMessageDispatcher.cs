using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net;
using MessageServer.Sources;
using Newtonsoft.Json;
using PestelLib.ServerCommon;
using ServerShared.Sources.Statistics;

namespace MessageClient.Sources
{
    public abstract class BaseMessageDispatcher : IDisposable
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(BaseMessageDispatcher));
        private readonly Dictionary<int, IMessageHandler> _messageHandlers = new Dictionary<int, IMessageHandler>();
        private readonly List<BaseMessageDispatcher> _injectedDispatchers = new List<BaseMessageDispatcher>();
        public event Action<MessageFrom> OnError = m => { };

        public class DispatcherStatistics
        {
            internal long _messagesCount;
            internal long _unhandledMessages;
            internal long _handleErrors;
            internal long _handleTime;
            internal long _handleCount;

            public long MessagesCount { get { return _messagesCount; } }
            public long HandledCount { get { return _handleCount; } }
            public long UnhandledCount { get { return _unhandledMessages; } }
            public long HandleErrors { get { return _handleErrors; } }
            public long AverageHandleTime { get { return _handleCount > 0 ? _handleTime / _handleCount : 0; } }

            public List<RequestStats> GetRequestsStats()
            {
                var list = new List<RequestStats>();
                lock (_handleTimes)
                {
                    foreach (var periodicAverageSimple in _handleTimes)
                    {
                        list.Add(new RequestStats()
                        {
                            MessageType = periodicAverageSimple.Key,
                            Stats = periodicAverageSimple.Value.GetSnapshot()
                        });
                    }
                }

                return list;
            }

            public void AddTime(int messageType, Stopwatch sw)
            {
                lock (_handleTimes)
                {
                    if (!_handleTimes.TryGetValue(messageType, out var m))
                    {
                        _handleTimes[messageType] = m = new PeriodicAverageSimple(TimeSpan.FromMinutes(1), 60000);
                    }

                    m.Add(sw.ElapsedTicks);
                }
            }

            private Dictionary<int, PeriodicAverageSimple> _handleTimes = new Dictionary<int, PeriodicAverageSimple>();
        }

        
        public class RequestStats
        {
            public int MessageType;
            public PeriodicAverageSnapshot Stats;
        }

        public DispatcherStatistics Stats { get; private set; }

        public BaseMessageDispatcher()
        {
            Stats = new DispatcherStatistics();
        }

        public void RegisterHandler(int messageType, IMessageHandler messageHandler)
        {
            _messageHandlers.Add(messageType, messageHandler);
        }

        // позволяет добавить диспатчеры более высокого уровня
        // изначально сделано для сервера друзей чтоб добавить туда кланы и не создавать новый сервер
        // и новое подключение на клиенте
        public void InjectDispatcher(BaseMessageDispatcher dispatcher)
        {
            _injectedDispatchers.Add(dispatcher);
        }

        public void Dispatch(MessageFrom message)
        {
            ++Stats._messagesCount;
            if (!_messageHandlers.TryGetValue(message.Message.Type, out var handler))
            {
                ++Stats._unhandledMessages;
                _log.Error($"No handler for message type '{message.Message.Type}'. dump={JsonConvert.SerializeObject(message.Message)}.");
                OnError(message);
                return;
            }

            var sw = Stopwatch.StartNew();
            Process(message, handler);
            Stats.AddTime(message.Message.Type, sw);
        }

        public void Dispose()
        {
            foreach (var disposable in _messageHandlers.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
            _messageHandlers.Clear();
        }

        protected void _error(MessageFrom message)
        {
            OnError(message);
        }

        protected abstract void Process(MessageFrom message, IMessageHandler handler);
    }
}
