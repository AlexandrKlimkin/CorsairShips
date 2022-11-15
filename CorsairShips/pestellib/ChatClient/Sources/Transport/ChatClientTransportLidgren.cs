using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Lidgren.Network;
using log4net;
using Newtonsoft.Json;
using PestelLib.ChatCommon;

namespace ChatClient.Transport
{
    public class ChatClientTransportLidgren : IChatClientTransport, IDisposable
    {
        public ChatClientTransportLidgren(bool useEncryption)
        {
            NetPeerConfiguration config = new NetPeerConfiguration(Consts.ChatAppId);
            config.AutoFlushSendQueue = true;
            _client = new NetClient(config);
        }

        public event Action Disconnect = () => { };
        public bool IsConnected => _client.ConnectionStatus == NetConnectionStatus.Connected;

        public void Start(string addr, int port)
        {
            _connectWaiter.Reset();
            _client.Start();
            _client.Connect(addr, port);
            
            _thread = new Thread(MessageLoop);
            _thread.Name = "PCChatMessageLoop";
            _thread.IsBackground = true;
            _thread.Start();

            _connectWaiter.WaitOne();

            _encryption = Consts.Init(_client.ServerConnection.Peer) as NetEncryption;
        }

        public void Stop()
        {
            _client.Shutdown("Client request");
            _thread.Join();
        }

        public void Close()
        {
            try
            {
                _client.Shutdown("Shutdown by Disconnect() call");
            }
            catch (Exception)
            {
                //TODO:
            }
            _client = null;
        }

        public bool SendMessage(ChatProtocol message)
        {
            string serialized = JsonConvert.SerializeObject(message);

            var lidMessage = _client.CreateMessage();
            lidMessage.Write(serialized);
            SpinWait.SpinUntil(() => _client != null);
            if (_encryption != null)
            {
                lock (_encryption)
                {
                    if (!_encryption.Encrypt(lidMessage))
                    {
                        Log.Error(
                            $"Cant encrypt message. size={lidMessage.LengthBytes}, raw={string.Join(":", lidMessage.Data.Select(_ => _.ToString("X")))}");
                        return false;
                    }
                }
            }
            var r = _client.SendMessage(lidMessage, Consts.DefaultDeliveryMethod);
            return true;
        }

        public ChatProtocol ReadMessage()
        {
            if (_messages.IsEmpty)
                return null;
            if (!_messages.TryDequeue(out var message))
                return null;
            return message;
        }

        public void Dispose()
        {
            _client.Shutdown("Dispose");
            _connectWaiter?.Dispose();
        }

        private void MessageLoop()
        {
            var client = _client;
            while (client.Status == NetPeerStatus.Running)
            {
                NetIncomingMessage im;
                while ((im = client.WaitMessage(1000)) != null)
                {
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
                            NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                            if (status == NetConnectionStatus.Connected)
                                _connectWaiter.Set();

                            if (status == NetConnectionStatus.Disconnected)
                                Disconnect();

                            string reason = im.ReadString();
                            Log.Debug(status.ToString() + ": " + reason);

                            break;
                        case NetIncomingMessageType.Data:
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
                            var chat = im.ReadString();

                            try
                            {
                                var message = JsonConvert.DeserializeObject<ChatProtocol>(chat);

                                if (message != null)
                                {
                                    _messages.Enqueue(message);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Error(chat, e);
                            }

                            break;
                        default:
                            Log.WarnFormat("Unhandled type: {0} {1} bytes", im.MessageType, im.LengthBytes);
                            break;
                    }
                    client.Recycle(im);
                }
            }
        }

        private Thread _thread;
        private NetClient _client;
        private NetEncryption _encryption;
        private AutoResetEvent _connectWaiter = new AutoResetEvent(false);
        private ConcurrentQueue<ChatProtocol> _messages = new ConcurrentQueue<ChatProtocol>();

        private static readonly ILog Log = LogManager.GetLogger(typeof(ChatClientTransportLidgren));
        
    }
}
