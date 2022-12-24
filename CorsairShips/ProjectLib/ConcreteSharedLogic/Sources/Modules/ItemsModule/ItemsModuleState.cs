using System.Collections.Generic;
using MessagePack;

namespace PestelLib.SharedLogic.Modules {
    
    [MessagePackObject]
    public class ItemsModuleState {
        [Key(0)]
        public List<ItemState> Items;
    }

    [MessagePackObject]
    public class ItemState {
        [Key(0)]
        public string ItemId;
        [Key(1)]
        public int Count;
    }
}