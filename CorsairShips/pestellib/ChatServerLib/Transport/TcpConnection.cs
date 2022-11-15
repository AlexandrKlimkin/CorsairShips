using System.Net;
using MessageServer.Server;
using PestelLib.ChatServer;

namespace ChatServer.Transport
{
    class TcpConnection : ChatConnection
    {
        public IPEndPoint RemoteEndPoint => _messageProvider.GetConnectionEndPoint(_connectionId);
        public bool IsConnected => _messageProvider.IsConnected(_connectionId);
        public int ConnectionId => _connectionId;
        public void Close()
        {
            _messageProvider.DisconnectSender(_connectionId);
        }

        public TcpConnection(IMessageProvider messageProvider, int connectionId)
        {
            _messageProvider = messageProvider;
            _connectionId = connectionId;
        }

        public override bool Equals(object obj)
        {
            if (obj is TcpConnection objCon)
                return RemoteEndPoint == objCon.RemoteEndPoint;
            return false;
        }

        private readonly IMessageProvider _messageProvider;
        private readonly int _connectionId;
    }
}
