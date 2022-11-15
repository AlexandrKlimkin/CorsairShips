using System.Collections.Generic;
using GoogleSpreadsheet;
using PestelLib.Serialization;
using PestelLib.SharedLogic.Modules;

namespace PestelLib.SharedLogic
{
    [System.Serializable]
    public class Definitions : IGameDefinitions
    {

#region AUTO_GENERATED_DEFINITIONS
        [GooglePageRef("MyTestModuleDef")]
        public List<MyTestModuleDef> MyTestModuleDefs = new List<MyTestModuleDef>();

        [GooglePageRef("BankCardsDefs")]
        public List<BankCardsDef> BankCards = new List<BankCardsDef>();

        [GooglePageRef("Property")]
        public List<PropertyDef> PropertyDefs = new List<PropertyDef>();

        public Dictionary<string, PropertyDef> PropertyDict = new Dictionary<string, PropertyDef>();
#endregion

        public void OnAfterDeserialize()
        {
#region AUTO_GENERATED_DICT_INIT
            PropertyDict.Clear();
            for (int i = 0; i < PropertyDefs.Count; i++)
            {
                var def = PropertyDefs[i];
                PropertyDict.Add(def.Id, def);
            }
#endregion
        }
    }
}
