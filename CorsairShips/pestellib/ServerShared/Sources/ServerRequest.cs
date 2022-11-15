using MessagePack;

namespace PestelLib.ServerShared
{
    [MessagePackObject]
    public class ServerRequest
    {
        [Key(1)]
        public S.Request Request;
        [Key(2)]
        public byte[] Data;
        [Key(3)]
        public byte[] State;
        [Key(4)]
        public string HostAddr;
    }
}
