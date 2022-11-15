using PestelLib.ChatCommon;
using PestelLib.ChatServer;

namespace ChatServer.Transport
{
    public interface IChatServerTransport
    {
        ChatServerTransportStats Stats { get; }
        bool IsConnected { get; }
        void Start();
        void Stop();
        ChatProtocol ReceiveMessage(int timeoutMillis, out ChatConnection connection);
        void SendTo(ChatProtocol message, params ChatConnection[] connections);
        void AddFilter(IMessageFiler filter);
    }
}