using System;
using ChatCommon;
using ChatServer.Transport;
using PestelLib.ChatServer;
using PestelLib.ServerCommon;

namespace PestelLib.ChatServerApp
{
    class Program
    {
        static ChatServer.ChatServer _server;
        static void Main(string[] args)
        {
            Log.Init();
            var serializer = new ChatProtocolJsonSerializer();
            var config = ChatServerConfigCache.Get();

            IChatServerTransport transport;
            if (config.ChatServerMessageProvider == ChatServerMessageProvider.Lidgren)
                transport = new LidgrenServerTransport(config.Port, serializer, userEncryption: config.UseMessageEncryption);
            else if(config.ChatServerMessageProvider == ChatServerMessageProvider.MessageServerTcp)
                transport = new TcpChatTransportServer(config.Port, serializer, config.UseMessageEncryption);
            else
                throw new NotSupportedException();

            _server = new ChatServer.ChatServer(transport);
            while (true)
            {
                var cmd = Console.ReadLine();
                if (cmd == "exit")
                    break;
            }
            _server.Dispose();
        }
    }
}
