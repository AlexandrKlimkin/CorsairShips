using MessagePack;

namespace ShortPlayerIdProtocol
{
    [MessagePackObject]
    public class GetPlayerIdResponse
    {
        [Key(1)]
        public byte[] PlayerId;
    }
}