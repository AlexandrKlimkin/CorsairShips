using System.Collections.Generic;
using MessagePack;

namespace PestelLib.SharedLogic.Modules
{
    [MessagePackObject()]
    public class PropertyModuleState
    {
        [Key(0)]
        public virtual List<string> OwnedProperty { get; set; }

        [Key(1)]
        public virtual List<string> UnlockedLockers { get; set; }

        [Key(2)]
        public virtual List<string> SpottedItems { get; set; }
    }
}