using MessagePack;
using MessagePack.Formatters;
using S;

namespace ServerShared.Sources.Numeric
{
    public class MadIdFormatter : IMessagePackFormatter<MadId>
    {
        public static readonly MadIdFormatter Instance = new MadIdFormatter();

        public MadIdFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, MadId value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteUInt32(ref bytes, offset, value);
        }

        public MadId Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            var raw = MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
            return new MadId(raw);
        }
    }
}
