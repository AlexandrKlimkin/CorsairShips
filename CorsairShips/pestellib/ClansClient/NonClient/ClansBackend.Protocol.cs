using MessagePack;

namespace ClansClientLib
{
    public enum ClansBackendMessageType
    {
        CreateClan,
        JoinClan,
        Find,
        LeaveClan,
        AddRandomMembers,
        RemoveClan,
        UserDelete,
        UserBanned
    }

    [MessagePackObject()]
    public class ClansBackendMessage
    {
        [Key(0)]
        public ClansBackendMessageType Type;
        [Key(1)]
        public byte[] Data;
        [Key(2)]
        public string Source;
        [Key(3)]
        public int SourceTag;
    }

    [MessagePackObject()]
    public class ClansBackendResponse
    {
        [Key(0)]
        public byte[] Data;
        [Key(1)]
        public int Tag;
    }
}
