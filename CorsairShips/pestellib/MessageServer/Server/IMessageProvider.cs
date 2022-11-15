using System;
using System.Net;
using MessageServer.Sources;

namespace MessageServer.Server
{
    public interface IMessageProvider
    {
        void DisconnectSender(int senderId);
        event Action<int> OnSenderDisconnect;
        MessageFrom GetMessage();
        MessageServerStats Stats { get; }
        bool IsConnected(int connectionId);
        IPEndPoint GetConnectionEndPoint(int connectionId);
    }
}
