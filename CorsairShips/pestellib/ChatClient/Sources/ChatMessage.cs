using System;
using PestelLib.ChatCommon;

namespace PestelLib.ChatClient
{
    public class ChatMessage
    {
        public DateTime Time;
        public string Channel;
        public bool Private;
        public string FromToken;
        public string ToToken;
        public string FromName;
        public string Message;
        public byte[] MessageMetadata;

        public ChatMessage()
        {
        }

        public ChatMessage(ChatProtocol prot)
        {
            Time = prot.Time;
            Channel = prot.ChannelName;
            Private = prot.SendTo != null;
            FromToken = prot.ClientInfo.Token;
            ToToken = prot.SendTo;
            FromName = prot.ClientInfo.Name;
            Message = prot.Body;
            MessageMetadata = prot.BodyMetadata;
        }
    }
}
