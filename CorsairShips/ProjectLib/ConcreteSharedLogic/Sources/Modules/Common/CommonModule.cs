using PestelLib.Serialization;
using System.Collections.Generic;
using UnityDI;


namespace PestelLib.SharedLogic.Modules
{
    public class CommonModule
    {
        #region Defs
        
        [GooglePageRef("Ships")]
        [Dependency]
        protected List<ShipDef> Ships;
        
        [GooglePageRef("Items")]
        [Dependency]
        protected List<ItemDef> Items;

        [GooglePageRef("Currency")]
        [Dependency]
        protected List<CurrencyDef> Currencies;
        
        [GooglePageRef("Nicknames")]
        [Dependency]
        protected List<NicknameDef> Nicknames;
        
        #endregion

    }
}
