using MessagePack;

namespace ShortPlayerIdProtocol
{
    [MessagePackObject]
    public class GetShortPlayerIdRequest : BaseShortPlayerRequest
    {
        [Key(1)]
        public byte[] Guid;
    }
}