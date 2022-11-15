using System;
using System.Linq;
using System.Text;
using Lidgren.Network;
using PestelLib.ServerShared;

namespace PestelLib.ChatServer
{
    public partial class ChatServer : ITokenProvider
    {
        public string FromChatUser(ChatUser user)
        {
            return user.Token;
        }

        public string FromPlayerId(Guid playerId)
        {
            if (!_config.UseMessageEncryption)
                return null;
            return FromBytes(playerId.ToByteArray());
        }

        public string FromBytes(byte[] data)
        {
            data = data.Concat(Encoding.UTF8.GetBytes(_config.Secret)).ToArray();
            var rawToken = Md5.MD5byteArray(data);
            return NetUtility.ToHexString(rawToken);
        }
    }
}
