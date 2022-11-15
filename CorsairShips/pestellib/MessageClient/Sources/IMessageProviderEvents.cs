using System;

namespace MessageServer.Sources
{
    public interface IMessageProviderEvents
    {
        event Action<Message, IAnswerContext> OnMessage;
        event Action OnConnected;
        event Action OnConnectionError;
        event Action OnDisconnected;
        bool IsConnected { get; }
        void Start();
        void Stop();
    }
}
