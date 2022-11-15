using MessagePack;

namespace ClansClientLib.DefsProxy
{
    [MessagePackObject()]
    public class ClanLevelDefProxy
    {
        [Key(0)]
        public int Level;
        [Key(1)]
        public int PlayersLimit;
    }
}
