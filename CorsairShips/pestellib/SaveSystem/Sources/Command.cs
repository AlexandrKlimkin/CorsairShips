namespace PestelLib.SaveSystem
{
    public class Command
    {
        public int SerialNumber { get; set; }

        public byte[] SerializedCommandData { get; set; }

        public uint SharedLogicCrc { get; set; }

        public uint DefinitionsVersion { get; set; }

        public int? Hash { get; set; }

        public long Timestamp { get; set; }
    }
}