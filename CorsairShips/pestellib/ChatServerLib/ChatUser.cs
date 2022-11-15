using System;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using Newtonsoft.Json;
using PestelLib.ChatCommon;

namespace PestelLib.ChatServer
{
    public interface ChatConnection
    {
        IPEndPoint RemoteEndPoint { get; }
        bool IsConnected { get; }
        void Close();
    }

    public class DummyChatConnection : ChatConnection
    {
        public IPEndPoint RemoteEndPoint => new IPEndPoint(0, 0);
        public bool IsConnected => true;
        public void Close()
        {
        }
    }

    public class ChatUser : ClientInfo
    {
        [JsonIgnore]
        public HashSet<string> Channels = new HashSet<string>();
        [JsonIgnore]
        public ChatConnection Connection;
        [JsonIgnore]
        public int FloodLevel;
        [JsonIgnore]
        public DateTime FloodTimeout;
        [JsonIgnore]
        public int InBanMessages;
        [JsonIgnore]
        public FilterStrikes FilterStrikes = new FilterStrikes();
        [JsonIgnore]
        public bool SuperUser;
        [JsonIgnore]
        public byte[] LastMeta;

        public override string ToString()
        {
            return string.Format("{0}:{1}", Name, Token);
        }
    }
}
