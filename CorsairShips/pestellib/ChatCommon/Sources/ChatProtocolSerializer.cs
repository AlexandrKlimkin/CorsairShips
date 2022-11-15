using System;
using System.Collections.Generic;
using System.Text;
using PestelLib.ChatCommon;

namespace ChatCommon
{
    public interface IChatProtocolSerializer
    {
        byte[] Serialize(ChatProtocol message);
        ChatProtocol Deserialize(byte[] data);
    }
}
