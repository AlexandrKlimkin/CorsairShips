using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;

namespace S
{
    [MessagePackObject()]
    public class CommandContainer
    {
        [Key(1)]
        public int CommandId;

        [Key(2)]
        public string DebugInfo;

        [JsonConverter(typeof(CommandDataConverter))] [Key(3)]
        public byte[] CommandData;
    }
}