using System;
using PestelLib.ChatCommon;

namespace ChatClient.Transport
{
    public interface IChatClientTransport
    {
        event Action Disconnect;
        bool IsConnected { get; }
        void Start(string addr, int port);
        void Stop();
        void Close();
        bool SendMessage(ChatProtocol message);
        ChatProtocol ReadMessage();
    }
}