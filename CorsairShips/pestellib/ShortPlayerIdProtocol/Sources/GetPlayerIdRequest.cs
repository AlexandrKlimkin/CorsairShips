using MessagePack;

namespace ShortPlayerIdProtocol
{
    [MessagePackObject]
    public class GetPlayerIdRequest : BaseShortPlayerRequest
    {
        [Key(1)]
        public int ShortPlayerId;
    }
}