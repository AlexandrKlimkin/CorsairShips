using System.Collections.Generic;
using MessagePack;

namespace PestelLib.SharedLogicBase
{
    [MessagePackObject]
    public class CommandBatch
    {
        [Key(0)] public List<Command> commandsList = new List<Command>();

        [Key(1)] public int controlHash;

        [Key(2)] public uint SharedLogicCrc;

        [Key(3)] public uint DefinitionsVersion;

        [Key(4)] public bool IsEditor;
    }

    [MessagePackObject]
    public class Command
    {
        [Key(0)] public long Timestamp;

        [Key(1)] public int SerialNumber;

        [Key(2)] public byte[] SerializedCommandData;
    }
}