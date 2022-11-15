using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using log4net;
using MessagePack;
using MessageServer.Sources.Sockets;
using ServerShared;
using ServerShared.Sources;

namespace MessageServer.Sources.Tcp
{
    public class TcpClientMessageProvider : IMessageProviderEvents, IMessageSender, IDisposable
    {
        private PingHandler _pingHandler = new PingHandler();
        public const int MaxConnectionRetries = 5;
        public static readonly TimeSpan MaxReconnectDelay = TimeSpan.FromSeconds(5);
        private TimeSpan _connectionRetryDelay = TimeSpan.FromSeconds(1);
        public static readonly TimeSpan PingInterval = TimeSpan.FromSeconds(1);
        public static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(15);
        private static readonly ILog _log = LogManager.GetLogger(typeof(TcpClientMessageProvider));
        private readonly string _host;
        private readonly int _port;
        private volatile  TcpClient _client;
        private volatile SocketContext _socketContext;
        private DateTime _lastConnect = DateTime.MinValue;
        private volatile int _connectionRetry = 0;
        private GrowingBuffer<byte> _buffer = new GrowingBuffer<byte>(2);
        private bool _started;
        private int _tag;
        private Dictionary<int, MessageAwaiterContext> _answerAwaiters = new Dictionary<int, MessageAwaiterContext>();
        private volatile bool _connectionInProgress;
        private Queue<Action> _syncQueue = new Queue<Action>();
        private DateTime _lastPing;
        private byte[] _emptyPingMessage;
        private volatile bool _isValid;
        private volatile bool _isConnected;
        private Queue<Message> _pendingMessages = new Queue<Message>();

        public event Action<Message, IAnswerContext> OnMessage = (message, actx) => { };

        public event Action OnConnected = () => { };
        public event Action OnConnectionError = () => { };
        public event Action OnDisconnected = () => { };
        public bool IsConnected
        {
            get { return _isConnected; }
        }

        public MessageStatistics Stats { get; }
        /// <summary>
        /// false if no connection or error occurred
        /// </summary>
        public bool IsValid
        {
            get { return _isValid; }
        }

        public TcpClientMessageProvider(string host, int port, IUpdateProvider updateProvider)
        {
            _emptyPingMessage = MessagePackSerializer.Serialize((byte) 0);
            Stats = new MessageStatistics();
            _host = host;
            _port = port;
            _client = new TcpClient();
            updateProvider.OnUpdate += Update;
        }

        public void Start()
        {
            ValidateConnection();
            _started = true;
        }

        public void Stop()
        {
            var wasConnected = IsConnected;
            _started = false;
            _client.Close();
            _client = new TcpClient();
            _isValid = false;
            _isConnected = false;
            _pingHandler.Reset();
            if (_socketContext != null)
            {
                try { _socketContext.Dispose(); } catch {}
                _socketContext = null;
            }

            if (wasConnected)
                OnDisconnected();
        }

        private bool ValidateConnection()
        {
            if (_client.Connected)
                return _socketContext != null;
            if (_connectionInProgress)
                return false;

            if (IsConnected)
            {
                _isConnected = false;
                OnDisconnected();
            }

            if(DateTime.UtcNow - _lastConnect < _connectionRetryDelay)
                return false; // connection retry delay

            ++_connectionRetry;
            _lastConnect = DateTime.UtcNow;
            try
            {
                _log.Debug($"Connecting friends server at {_host}:{_port} ({_client?.Client?.AddressFamily}). retry={_connectionRetry}.");
                _client.BeginConnect(_host, _port, ConnectionEnd, null);
                _connectionInProgress = true;
            }
            catch (Exception e)
            {
                _log.Warn("Connection error.", e);
                _isValid = false;
                _isConnected = false;
                _connectionInProgress = false;
                ResetClient();
                OnConnectionError();
            }
            return false;
        }

        private void ResetClient()
        {
            foreach (var ans in _answerAwaiters)
            {
                _log.Warn($"ResetClient. Cancel request with tag {ans.Key}. execTime={DateTime.UtcNow - ans.Value.QueueTime}.");
                ans.Value.Handler.Error(ans.Key);
                if (ans.Value is IDisposable d)
                    d.Dispose();
            }
            _answerAwaiters.Clear();
            if (_connectionRetry % 2 == 0)
                _client = new TcpClient(AddressFamily.InterNetwork);
            else
                _client = new TcpClient(AddressFamily.InterNetworkV6);
        }

        private void ConnectionEnd(IAsyncResult result)
        {
            try
            {
                _client.EndConnect(result);
                _client.Client.NoDelay = true;
                _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                _connectionRetry = 0;
                _isValid = true;
                _isConnected = true;
                _socketContext = new SocketContext(_client.Client);
                _pingHandler.Reset();
                lock (_syncQueue) _syncQueue.Enqueue(OnConnected);
                _connectionRetryDelay = TimeSpan.FromSeconds(1);
            }
            catch (Exception e)
            {
                _log.Warn("Connection error.", e);
                _isValid = false;
                _isConnected = false;

                lock (_syncQueue) _syncQueue.Enqueue(OnConnectionError);
                ResetClient();
            }
            finally
            {
                _connectionInProgress = false;
            }
        }

        private void Ping()
        {
            if (DateTime.UtcNow - _lastPing < PingInterval)
                return;
            if (_pingHandler.Busy)
            {
                if (_pingHandler.BusyTime >= ConnectionTimeout)
                {
                    _log.Error("No answer from friends server. Initiating reconnect...");
                    Stop();
                    Start();
                }
                return;
            }

            try
            {
                _lastPing = DateTime.UtcNow;
                var tag = Request(1, 0, _emptyPingMessage, _pingHandler);
                _pingHandler.Register(tag);
            }
            catch (Exception e)
            {
                _log.Error("Cant send ping message.", e);
            }
        }

        private void Update()
        {
            if (_syncQueue.Count > 0)
            {
                lock (_syncQueue)
                {
                    while (_syncQueue.Count > 0)
                    {
                        var action = _syncQueue.Dequeue();
                        action();
                    }
                }
            }

            if (_answerAwaiters.Any(_ => DateTime.UtcNow - _.Value.QueueTime > ConnectionTimeout))
            {
                var expiredRequests = _answerAwaiters.Where(_ => DateTime.UtcNow - _.Value.QueueTime > ConnectionTimeout).Select(_ => _.Key).ToArray();
                for (var i = 0; i < expiredRequests.Length; ++i)
                {
                    if(!_answerAwaiters.TryGetValue(expiredRequests[i], out var ctx))
                        continue;
                    _log.Warn($"Request with tag {expiredRequests[i]} expired. execTime={DateTime.UtcNow - ctx.QueueTime}.");
                    ctx.Handler.Error(expiredRequests[i]);
                    _answerAwaiters.Remove(expiredRequests[i]);
                }
            }

            if(!_started)
                return;
            if (_connectionRetry == MaxConnectionRetries)
            {
                _connectionRetryDelay = MaxReconnectDelay;
            }
            if(!ValidateConnection())
                return;
            if (!IsValid)
                return;
            while (_pendingMessages.Count > 0)
            {
                var pmsg = _pendingMessages.Peek();
                var s = _socketContext.SendMessage(pmsg);
                _isValid = s > 0;
                if (!_isValid) return;
                Interlocked.Add(ref Stats.BytesSent, s);
                _pendingMessages.Dequeue();
            }
            Ping();
            if(_socketContext == null || !_socketContext.IsValid) return;
            int bytesRead;
            try
            {
                if (!_socketContext.Socket.Poll(1000, SelectMode.SelectRead)) return;
            }
            catch (Exception e)
            {
                _log.Warn("Read socket error.", e);
                ResetClient();
                return;
            }

            var msg = _socketContext.ReadMessage(out bytesRead);
            if (msg.Data == null && !_socketContext.IsValid)
            {
                ResetClient();
                return;
            }
            Interlocked.Increment(ref Stats.MessagesCount);
            Interlocked.Add(ref Stats.BytesReceived, bytesRead);
            IAnswerContext answerContext = null;
            if (msg.Tag > 0 && !msg.Answer)
                answerContext = new TcpAnswerContext(() => _socketContext, msg.Tag, Stats); // client can change sockets due to disconnects, so we will retrieve socket then we a about to send answer
            if (!msg.Answer)
                OnMessage(msg, answerContext);
            else
            {
                MessageAwaiterContext ctx;
                IMessageHandler handler = null;
                if (!_answerAwaiters.TryGetValue(msg.Tag, out ctx))
                    _log.Error($"Answer handler for tag {msg.Tag} not found.");
                else
                {
                    _answerAwaiters.Remove(msg.Tag);
                    handler = ctx.Handler;
                }

                try
                {
                    if (handler != null)
                        handler.Handle(_socketContext.Id, msg.Data, msg.Tag, null);
                }
                catch (Exception e)
                {
                    _log.Error($"Error while handling answer.", e);
                }
            }
        }

        public void Notify(int target, int type, byte[] data)
        {
            SendMessage(type, data, 0);
            Interlocked.Increment(ref Stats.NotifyCount);
        }

        public void Notify(int[] targets, int type, byte[] data)
        {
            foreach (var target in targets)
            {
                Notify(target, type, data);
            }
        }

        public int Request(int target, int type, byte[] data, IMessageHandler answerHandler)
        {
            var tag = ++_tag;
            if (answerHandler != null)
                _answerAwaiters[tag] = new MessageAwaiterContext() {Handler = answerHandler};

            SendMessage(type, data, tag);
            Interlocked.Increment(ref Stats.RequestCount);
            if (!_isValid)
            {
                if (answerHandler != null)
                {
                    _log.Warn($"Send request failed. type={type}, tag={tag}.");
                    answerHandler.Error(tag);
                }
            }

            return tag;
        }

        private void SendMessage(int type, byte[] data, int tag)
        {
            var msg = new Message();
            msg.Type = type;
            msg.Data = data;
            msg.Tag = tag;
            if (_socketContext != null)
            {
                var s = _socketContext.SendMessage(msg);
                _isValid = s > 0;
                Interlocked.Add(ref Stats.BytesSent, s);
            }
            else
            {
                _pendingMessages.Enqueue(msg);
            }
        }

        public void Dispose()
        {
            if(_client != null)
                _client.Dispose();

            if(_socketContext != null)
                _socketContext.Dispose();

            _client = null;
            _socketContext = null;
        }

        class MessageAwaiterContext
        {
            public DateTime QueueTime = DateTime.UtcNow;
            public IMessageHandler Handler;
        }
    }
}
