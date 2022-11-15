using MessagePack;

namespace S
{
    [MessagePackObject()]
    public class RouletteEventsState
    {
        [Key(0)]
        public long UltraBoxEndTimestamp { get; set; }

        [Key(1)]
        public long AdsBoxTimestamp { get; set; }
    }
}
