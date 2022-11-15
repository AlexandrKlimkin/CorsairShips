using System.Collections.Generic;
using MessagePack;

namespace PestelLib.SharedLogic.Modules
{
    [MessagePackObject]
    public class RouletteModuleState
    {
        [Key(0)]
        public int Keys { get; set; }

        [Key(1)]
        public float MegaChestProgress { get; set; }

        [Key(2)]
        public List<KeyValuePair<string, long>> LastClaimsByType;
    }
}