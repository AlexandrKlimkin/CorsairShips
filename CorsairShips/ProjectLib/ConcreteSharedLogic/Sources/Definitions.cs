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
        [GooglePageRef("Ships")]
        public List<ShipDef> ShipDefs = new List<ShipDef>();
        [GooglePageRef("Ships")]
        public Dictionary<string, ShipDef> ShipDefDict = new Dictionary<string, ShipDef>();
        [GooglePageRef("BankCardsDefs")]
        public List<BankCardsDef> BankCardsDefs = new List<BankCardsDef>();
        [GooglePageRef("BankCardsDefs")]
        public Dictionary<string, BankCardsDef> BankCardsDefDict = new Dictionary<string, BankCardsDef>();
        [GooglePageRef("MyTestModuleDef")]
        public List<MyTestModuleDef> MyTestModuleDefs = new List<MyTestModuleDef>();
        [GooglePageRef("MyTestModuleDef")]
        public Dictionary<string, MyTestModuleDef> MyTestModuleDefDict = new Dictionary<string, MyTestModuleDef>();
        [GooglePageRef("Property")]
        public List<PropertyDef> PropertyDefs = new List<PropertyDef>();
        [GooglePageRef("Property")]
        public Dictionary<string, PropertyDef> PropertyDefDict = new Dictionary<string, PropertyDef>();
#endregion

        public void OnAfterDeserialize()
        {
#region AUTO_GENERATED_DICT_INIT
            ShipDefDict.Clear();
            for (int i = 0; i < ShipDefs.Count; i++)
            {
                var def = ShipDefs[i];
                ShipDefDict.Add(def.Id, def);
            }
            BankCardsDefDict.Clear();
            for (int i = 0; i < BankCardsDefs.Count; i++)
            {
                var def = BankCardsDefs[i];
                BankCardsDefDict.Add(def.Id, def);
            }
            MyTestModuleDefDict.Clear();
            for (int i = 0; i < MyTestModuleDefs.Count; i++)
            {
                var def = MyTestModuleDefs[i];
                MyTestModuleDefDict.Add(def.Id, def);
            }
            PropertyDefDict.Clear();
            for (int i = 0; i < PropertyDefs.Count; i++)
            {
                var def = PropertyDefs[i];
                PropertyDefDict.Add(def.Id, def);
            }
#endregion
        }
    }
}
