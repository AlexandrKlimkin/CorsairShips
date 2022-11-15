using System;
using System.Collections.Generic;
using PestelLib.Serialization;

namespace PestelLib.SharedLogic.Defs
{
    [Serializable]
    public class LocalizationDefContainer
    {
        [GooglePageRef("Localization")]
        public List<Defs.LocalizationDef> LocalizationDef;
    }
}
