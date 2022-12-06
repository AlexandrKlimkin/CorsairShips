using PestelLib.Serialization;
using PestelLib.SharedLogicBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
// using PestelLib.SharedLogic.Modules.Defs;
using UnityDI;


namespace PestelLib.SharedLogic.Modules
{
    public class CommonModule
    {
        #region Defs
        
        [GooglePageRef("Ships")]
        [Dependency]
        protected List<ShipDef> Ships;
        

        #endregion

    }
}
