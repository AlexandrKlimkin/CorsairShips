using System;
using MessagePack;

namespace PestelLib.ServerShared
{
    [MessagePackObject]
    public class ServerResponse
    {
        [Key(0)]
        public S.ResponseCode ResponseCode;
        [Key(1)]
        public Guid PlayerId;
        [Key(2)]
        public byte[] Data;
        [Key(3)]
        public byte[] ActualUserProfile;
        [Key(4)]
        public string DebugInfo;
        [Key(5)]
        public byte[] Token;
        [Key(6)]
        public int ShortId;
    }
}
