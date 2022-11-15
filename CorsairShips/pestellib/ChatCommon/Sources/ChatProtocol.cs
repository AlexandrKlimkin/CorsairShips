//using ProtoBuf;
using System;
using Newtonsoft.Json;

namespace PestelLib.ChatCommon
{
    // нельзя обфусцировать используется как JSON
    [System.Reflection.Obfuscation(Exclude = true)]
    public class ChatProtocol
    {
        public CommandType CommandType;
        public int Version;
        public DateTime Time = DateTime.UtcNow;
        public string ChannelName;
        public string Body;
        public ClientInfo ClientInfo;
        public ClientInfo[] Clients;
        public ChatProtocol[] MessageHistory;
        public string SendTo;
        public BanReason BanReason;
        public int Tag;
        public byte[] BodyMetadata;
    }
}
