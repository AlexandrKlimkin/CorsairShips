using System;

namespace PestelLib.ServerCommon.Db.Auth
{
    public class AuthToken
    {
        public Guid TokenId;
        public Guid PlayerId;
        public DateTime AuthTime;
        public DateTime ExpiryTime;
        public string Address;
    }

    public interface ITokenStore
    {
        AuthToken GetByTokenId(Guid tokenId);
        AuthToken GetByPlayerId(Guid playerId);
    }

    public interface ITokenStoreWriter
    {
        AuthToken CreateToken(Guid playerId, TimeSpan ttl, string clientAddress);
    }
}
