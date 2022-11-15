using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PestelLib.ChatCommon;

namespace PestelLib.ChatServer
{
    public class BanInfo
    {
        public string Token;
        public BanReason Reason;
        public DateTime Expiry;
    }

    public class BanRequest
    {
        public Guid PlayerId;
        public TimeSpan Period;
        public DateTime CreateTime;
    }

    interface IBanStorage
    {
        bool IsBanned(IPEndPoint endPoint);
        bool IsBanned(ChatUser user);
        BanInfo[] GetBans(ChatUser user);
        BanInfo[] GetBans(string userToken);
        DateTime GrantBan(string token, BanReason reason, TimeSpan period);
        DateTime GrantBan(ClientInfo user, BanReason reason, TimeSpan period);
        DateTime GrantBan(ClientInfo user, BanReason reason, DateTime expiry);
        Regex[] BanPatterns();
    }

    interface ITokenProvider
    {
        string FromChatUser(ChatUser user);
        string FromPlayerId(Guid playerId);
        string FromBytes(byte[] data);
    }

    public interface IBanRequestStorage
    {
        BanRequest GetBanRequest(int myId);
        void AddBanRequest(Guid playerId, TimeSpan period);
    }

    class BanStorageStub : IBanStorage, IBanRequestStorage
    {
        public bool IsBanned(IPEndPoint endPoint)
        {
            return false;
        }

        public bool IsBanned(ChatUser user)
        {
            return false;
        }

        public BanInfo[] GetBans(ChatUser user)
        {
            return new BanInfo[] {};
        }

        public BanInfo[] GetBans(string userToken)
        {
            return new BanInfo[] { };
        }

        public DateTime GrantBan(string token, BanReason reason, TimeSpan period)
        {
            return DateTime.MinValue;
        }

        public DateTime GrantBan(ClientInfo user, BanReason reason, TimeSpan period)
        {
            return DateTime.MinValue;
        }

        public DateTime GrantBan(ClientInfo user, BanReason reason, DateTime expiry)
        {
            return DateTime.MinValue;
        }

        public Regex[] BanPatterns()
        {
            return new Regex[] {};
        }

        public BanRequest GetBanRequest(int myId)
        {
            return null;
        }

        public void AddBanRequest(Guid playerId, TimeSpan period)
        {
        }
    }
}
