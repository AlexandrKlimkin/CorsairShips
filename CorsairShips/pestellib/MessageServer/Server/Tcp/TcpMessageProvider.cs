using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;
using MessageServer.Sources;
using MessageServer.Sources.Sockets;
using MessageServer.Sources.Tcp;
using PestelLib.ServerCommon.Tcp;
using PestelLib.ServerCommon.Threading;
using System.Threading.Tasks;

namespace MessageServer.Server.Tcp
{
    public interface IMessageTypeStringGetter
    {
        string GetMessageTypeString(int type);
    }

    class StubMessageTypeStringGetter : IMessageTypeStringGetter
    {
        public string GetMessageTypeString(int type)
        {
            return type.ToString();
        }
    }

    public class EnumMessageTypeStringGetter<T> : IMessageTypeStringGetter where T: System.Enum
    {
        public string GetMessageTypeString(int type)
        {
            if (Enum.IsDefined(typeof(T), type))
            {
                return Enum.GetName(typeof(T), type);
            }


            return type.ToString();
        }
    }

    public class TcpMessageProvider : DefaultTcpServer, IMessageProvider, IMessageSender
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(TcpMessageProvider));
        private object _sync = new object();
        private Dictionary<Socket, SocketContext> _socketContexts = new Dictionary<Socket, SocketContext>();
        private Dictionary<int, SocketContext> _socketContextsById = new Dictionary<int, SocketContext>();
        private Dictionary<int, MessageHandlerContext> _answerAwaiters = new Dictionary<int, MessageHandlerContext>();
        private ManualResetEventSlim _event = new ManualResetEventSlim();
        private HashSet<int> _disconnectList = new HashSet<int>();
        private MessageReader _messageReader;
        private int _tag;
        private TimeSpan _readTimeout = TimeSpan.FromMinutes(1);
        private IMessageTypeStringGetter _messageTypeStringGetter;

        public MessageServerStats Stats { get; private set; }
        public bool IsConnected(int connectionId)
        {
            lock (_sync)
            {
                if (_socketContextsById.TryGetValue(connectionId, out var ctx))
                    return ctx.IsValid;
            }

            return false;
        }

        public IPEndPoint GetConnectionEndPoint(int connectionId)
        {
            lock (_sync)
            {
                try
                {
                    if (_socketContextsById.TryGetValue(connectionId, out var ctx))
                        return ctx.Socket.RemoteEndPoint as IPEndPoint;
                }
                catch { }
            }

            return null;
        }

        public bool IsValid { get; private set; }

        public void WaitConnection()
        {
            if(_socketContexts.Count > 0)
                return;
            _event.Wait();
        }

        public void DisconnectSender(int senderId)
        {
            lock (_sync)
            {
                _disconnectList.Add(senderId);
            }
        }

        public event Action<int> OnSenderDisconnect
        {
            add => _messageReader.OnSenderDisconnect += value;
            remove => _messageReader.OnSenderDisconnect -= value;
        }

        public MessageFrom GetMessage()
        {
            return _messageReader.WaitMessage();
        }

        public TcpMessageProvider(int port, IMessageTypeStringGetter messageTypeStringGetter = null) : base(port)
        {
            IsValid = true;
            _messageTypeStringGetter = messageTypeStringGetter ?? new StubMessageTypeStringGetter();
            Stats = new MessageServerStats();
            _messageReader = new MessageReader(this);
        }

        public void Notify(int target, int type, byte[] data)
        {
            SocketContext ctx;
            lock (_sync)
            {
                if (!_socketContextsById.TryGetValue(target, out ctx))
                {
                    _log.Error($"Can't send message. Target {target} not found.");
                    return;
                }
            }
            var msg = new Message();
            msg.Type = type;
            msg.Data = data;
            var s = ctx.SendMessage(msg);
            Interlocked.Add(ref Stats.BytesSent, s);
            Interlocked.Increment(ref Stats.NotifyCount);
            Stats.NotifySent(type);
            if (s == 0)
                _log.Error($"Notify not sent. type={type},target={target},valid={ctx.IsValid}.");
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
            var tag = Interlocked.Increment(ref _tag);
            SocketContext ctx;
            lock (_sync)
            {
                if (!_socketContextsById.TryGetValue(target, out ctx))
                {
                    _log.Error($"Can't send message. Target {target} not found.");
                    return 0;
                }
                if(answerHandler != null)
                    _answerAwaiters.Add(tag, new MessageHandlerContext() { Type = type, Handler = answerHandler });
            }
            var msg = new Message();
            msg.Type = type;
            msg.Data = data;
            msg.Tag = tag;
            var s = ctx.SendMessage(msg);
            Interlocked.Add(ref Stats.BytesSent, s);
            Interlocked.Increment(ref Stats.RequestCount);
            Interlocked.Exchange(ref Stats.RequestQueueSize, _answerAwaiters.Count);
            if (s == 0)
                _log.Error($"Request not sent. type={type},target={target},valid={ctx.IsValid}.");
            return tag;
        }

        protected override void OnNewClient(TcpClient client)
        {
            Interlocked.Increment(ref Stats.ConnectionsTotal);
            Interlocked.Increment(ref Stats.ConnectionsCurrent);
            var readContext = new SocketContext(client.Client);
            _log.Debug($"New connection from {client.Client.RemoteEndPoint}. Id={readContext.Id}.");
            lock (_sync)
            {
                _socketContexts.Add(readContext.Socket, readContext);
                _socketContextsById.Add(readContext.Id, readContext);
            }
            Interlocked.Exchange(ref Stats.ContextsListSize, _socketContexts.Count);
            Interlocked.Exchange(ref Stats.IdMapSize, _socketContextsById.Count);
            _event.Set();
        }

        /// <summary>
        /// Reads messages from all connected sockets through select()
        /// Using separate thread
        /// </summary>
        class MessageReader : DisposableLoop
        {
            private readonly TcpMessageProvider _provider;
            private ConcurrentBag<MessageFrom> _messages = new ConcurrentBag<MessageFrom>();
            private ManualResetEventSlim _eventNewMessages = new ManualResetEventSlim();
            private bool _initialized;

            public MessageReader(TcpMessageProvider provider)
            {
                _provider = provider;
                _initialized = true;
            }

            public MessageFrom WaitMessage()
            {
                if (_messages.IsEmpty)
                {
                    _eventNewMessages.Wait();
                }
                MessageFrom msg;
                if (_messages.TryTake(out msg))
                    return msg;
                if(_messages.IsEmpty)
                    _eventNewMessages.Reset();
                return new MessageFrom();
            }

            public event Action<int> OnSenderDisconnect = s => { };

            List<Socket> read = new List<Socket>();
            List<Socket> error = new List<Socket>();
            HashSet<Socket> removeList = new HashSet<Socket>();
            protected override void Update(CancellationToken cancellationToken)
            {
                if(!_initialized) return;
                // wait if there is not connections yet
                _provider.WaitConnection();
                read.Clear();
                error.Clear();
                removeList.Clear();
                var disconnectCount = 0;

                // preparing sockets list to select read
                lock (_provider._sync)
                {
                    disconnectCount = _provider._disconnectList.Count;
                    foreach (var ctxKV in _provider._socketContexts)
                    {
                        var socket = ctxKV.Value.Socket;
                        var inactivityTimeout = DateTime.UtcNow - ctxKV.Value.LastReadTime > _provider._readTimeout;
                        var hash = socket.GetHashCode();

                        if(inactivityTimeout)
                            _log.Error($"Socket {ctxKV.Value.Id} inactivity timeout.");
                        if (!socket.Connected || !ctxKV.Value.IsValid || inactivityTimeout)
                        {
                            _provider._disconnectList.Add(hash);
                            ctxKV.Value.Close();
                        }
                        else if (_provider._disconnectList.Contains(hash))
                        {
                            ctxKV.Value.Close();
                        }
                        else
                        {
                            read.Add(socket);
                            error.Add(socket);
                        }
                    }
                }

                try
                {
                    if(read.Count > 0 || error.Count > 0)
                        Socket.Select(read, null, error, 1000000);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
                if (error.Count > 0)
                {
                    for (var i = 0; i < error.Count; ++i)
                    {
                        _log.Debug($"{error[i].GetHashCode()} has error.");
                        removeList.Add(error[i]);
                    }
                }

                // nothing to read and no closing sockets
                if (read.Count == 0 && removeList.Count == 0 && disconnectCount == 0)
                    return;

                if (read.Count > 0)
                {
                    //_log.Debug($"{read.Count} sockets has data to read.");
                    // search for appropriate read-ready contexts
                    SocketContext[] contexts = new SocketContext[read.Count];
                    lock (_provider._sync)
                    {
                        for (var i = 0; i < read.Count; ++i)
                        {
                            contexts[i] = _provider._socketContexts[read[i]];
                        }
                    }

                    for (var i = 0; i < contexts.Length; ++i)
                    {
                        int bytesRead = 0;
                        // trying to read message from socket
                        var msg = contexts[i].ReadMessage(out bytesRead);
                        // no message read
                        if (msg.Data == null)
                            continue;

                        Interlocked.Increment(ref _provider.Stats.MessagesCount);
                        Interlocked.Add(ref _provider.Stats.BytesReceived, bytesRead);
                        var msgFrom = new MessageFrom();
                        msgFrom.Message = msg;
                        msgFrom.Sender = contexts[i].Socket.GetHashCode();
                        // tagged messages can be answered so we will give AnswerContext for that
                        if (msg.Tag > 0 && !msg.Answer)
                            msgFrom.AnswerContext = new TcpAnswerContext(contexts[i], msg.Tag, _provider.Stats);
                        if (!msg.Answer)
                            _messages.Add(msgFrom);
                        else // answers processed separately from messages
                        {
                            MessageHandlerContext ctx;
                            lock (_provider._sync)
                            {
                                if (!_provider._answerAwaiters.TryGetValue(msg.Tag, out ctx))
                                    _log.Error($"Answer handler for tag {msg.Tag} not found.");
                                else
                                {
                                    _provider._answerAwaiters.Remove(msg.Tag);
                                    Interlocked.Exchange(ref _provider.Stats.RequestQueueSize, _provider._answerAwaiters.Count);
                                }
                            }

                            try
                            {
                                ctx?.Handler.Handle(contexts[i].Id, msg.Data, msg.Tag, null);
                            }
                            catch (Exception e)
                            {
                                _log.Error($"Error while handling answer.", e);
                            }
                        }

                        // log if not ping message
                        if (msgFrom.Message.Type > 0)
                        {
                            var messageTypeString = _provider._messageTypeStringGetter.GetMessageTypeString(msgFrom.Message.Type);
                            _log.Debug(
                                $"New message {messageTypeString} from {msgFrom.Sender}. sz={msgFrom.Message.Data.Length},tag={msgFrom.Message.Tag}.");
                        }

                        _eventNewMessages.Set();
                    }
                }

                // remove closed sockets
                lock (_provider._sync) {
                    foreach (var ctxKV in _provider._socketContexts)
                    {
                        var hash = ctxKV.Value.Socket.GetHashCode();
                        var markedForDisconnect = _provider._disconnectList.Contains(hash);
                        var connected = ctxKV.Value.Socket.Connected;
                        if (ctxKV.Value.IsValid && !markedForDisconnect && connected)
                            continue;
                        if(!connected)
                            _log.Debug($"{hash} not connected.");
                        ctxKV.Value.Close();
                        removeList.Add(ctxKV.Value.Socket);
                        if (markedForDisconnect)
                        {
                            _provider._disconnectList.Remove(hash);
                        }
                    }

                    if (removeList.Count > 0)
                    {
                        _log.Debug($"{removeList.Count} sockets will be removed.");
                        Interlocked.Add(ref _provider.Stats.ConnectionsCurrent, -removeList.Count);
                    }

                    foreach (var k in removeList)
                    {
                        SocketContext ctx;
                        if (_provider._socketContexts.TryGetValue(k, out ctx))
                        {
                            _provider._socketContextsById.Remove(ctx.Id);
                            _provider._socketContexts.Remove(k);
                            Task.Run(() => OnSenderDisconnect(ctx.Id));
                        }
                    }

                    if (_provider._socketContexts.Count == 0)
                    {
                        _provider._event.Reset();
                    }

                    Interlocked.Exchange(ref _provider.Stats.ContextsListSize, _provider._socketContexts.Count);
                    Interlocked.Exchange(ref _provider.Stats.IdMapSize, _provider._socketContextsById.Count);
                }
            }
        }
    }
}
