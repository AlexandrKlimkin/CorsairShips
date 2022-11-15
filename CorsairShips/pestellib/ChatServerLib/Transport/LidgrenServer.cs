using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ChatCommon;
using Lidgren.Network;
using log4net;
using Newtonsoft.Json;
using PestelLib.ChatCommon;
using PestelLib.ChatServer;

namespace ChatServer.Transport
{
    public class LidgrenServerTransport : IChatServerTransport
    {
        public bool IsConnected => _server?.Status == NetPeerStatus.Running;
        public void Start()
        {
            _server.Start();

            _chatServerLoop = new Thread(ChatServerLoop);
            _chatServerLoop.IsBackground = true;
            _chatServerLoop.Name = "LidgrenServerTransportThread";

            _chatServerLoop.Start();
        }

        public void Stop()
        {
            if (_server.Status == NetPeerStatus.Running)
                _server.Shutdown("Shutdown on destroy");

            _chatServerLoop.Join();
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
            var msg = _server.CreateMessage();
            var json = JsonConvert.SerializeObject(message);
            msg.Write(json);
            if (_encryption != null)
            {
                lock (_encryption)
                {
                    if (!msg.Encrypt(_encryption))
                    {
                        Log.Error(
                            $"Can't encrypt message. size={msg.LengthBytes}, raw={string.Join(":", msg.Data.Select(_ => _.ToString("X")))}");
                        return;
                    }
                }
            }

            Interlocked.Add(ref Stats.BytesSent, msg.LengthBytes);
            var lidgrenConns = connections.Select(_ => _ as LidgrenConnection).Where(_ => _ != null).Select(_ => _.NetConnection).ToArray();
             _server.SendMessage(msg, lidgrenConns, Consts.DefaultDeliveryMethod, 0);
        }

        public void AddFilter(IMessageFiler filter)
        {
            System.Diagnostics.Debug.Assert(!IsConnected, "Can't add filter in connected state.");
            _filters.Add(filter);
        }

        public ChatServerTransportStats Stats { get; }

        public LidgrenServerTransport(int port, IChatProtocolSerializer serializer, float connectionTimeout = 60f, int maxConnections = 10000, string appId = Consts.ChatAppId, bool userEncryption = false)
        {
            Stats = new ChatServerTransportStats();
            NetPeerConfiguration config = new NetPeerConfiguration(appId);
            config.ConnectionTimeout = connectionTimeout;
            config.MaximumConnections = maxConnections;
            config.Port = port;
            _server = new NetServer(config);
            _encryption = userEncryption ? Consts.Init(_server) as NetEncryption : null;
        }

        private void ChatServerLoop()
        {
            Log.Debug("ChatServerLoop enter");
            NetIncomingMessage im;
            while (IsConnected)
            {
                while ((im = _server.WaitMessage(1000)) != null)
                {
                    Interlocked.Add(ref Stats.BytesReceived, im.LengthBytes);
                    // handle incoming message
                    switch (im.MessageType)
                    {
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.ErrorMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.VerboseDebugMessage:
                            string text = im.ReadString();
                            Log.Debug(text);
                            break;

                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus) im.ReadByte();

                            if (status == NetConnectionStatus.Connected)
                                Interlocked.Increment(ref Stats.AcceptedConnections);
                            else if (status == NetConnectionStatus.Disconnected)
                                Interlocked.Increment(ref Stats.ClosedConnections);
                            string reason = im.ReadString();
                            Log.Info(NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier) + " " + status +
                                     ": " + reason);
                            break;
                        case NetIncomingMessageType.Data:
                            // incoming chat message from a client
                            try
                            {
                                var data = "";
                                if (_encryption != null)
                                {
                                    lock (_encryption)
                                    {
                                        if (!im.Decrypt(_encryption))
                                        {
                                            Log.Error(
                                                $"Can't decrypt message. size={im.LengthBytes}, raw={string.Join(":", im.Data.Select(_ => _.ToString("X")))}");
                                            break;
                                        }
                                    }
                                }

                                data = im.ReadString();

                                var muted = false;
                                foreach (var filer in _filters)
                                {
                                    muted = filer.CanRemove(data);
                                    if (muted)
                                        break;
                                }

                                if (!muted)
                                {
                                    var msg = JsonConvert.DeserializeObject<ChatProtocol>(data);
                                    msg.Time = DateTime.UtcNow;
                                    _messageQueue.Enqueue(new IncomingMessage()
                                    {
                                        Message = msg,
                                        Connection = new LidgrenConnection(im.SenderConnection)
                                    });
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Error($"Read message error from {im.SenderConnection.RemoteEndPoint}.", e);
                            }

                            break;
                        default:
                            Log.Warn("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes " +
                                     im.DeliveryMethod + "|" + im.SequenceChannel);
                            break;
                    }

                    _server.Recycle(im);
                }
            }
            Log.Debug("ChatServerLoop exit");
        }

        class IncomingMessage
        {
            public ChatProtocol Message;
            public LidgrenConnection Connection;
        }

        private List<IMessageFiler> _filters = new List<IMessageFiler>();
        private ConcurrentQueue<IncomingMessage> _messageQueue = new ConcurrentQueue<IncomingMessage>();
        private NetServer _server;
        private NetEncryption _encryption;
        private Thread _chatServerLoop;
        private static readonly ILog Log = LogManager.GetLogger(typeof(LidgrenServerTransport));
    }
}
