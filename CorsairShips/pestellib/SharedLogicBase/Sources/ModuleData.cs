using MessagePack;

namespace S
{
    [MessagePackObject]
    public class ModuleData
    {
        [Key(0)]
        public string ModuleName { get; set; }

        [Key(1)]
        public byte[] Data { get; set; }
    }
}