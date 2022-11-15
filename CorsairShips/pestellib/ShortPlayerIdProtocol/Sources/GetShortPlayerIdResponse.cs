using MessagePack;

namespace ShortPlayerIdProtocol
{
    [MessagePackObject]
    public class GetShortPlayerIdResponse
    {
        [Key(1)]
        public int ShortPlayerId;
    }
}