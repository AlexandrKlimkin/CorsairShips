using System;

namespace PestelLib.SharedLogic.Modules {
    [Serializable]
    public class ShipDef {
        public string Id;
        public string ModelId;
        public string ConfigId;
        public string NameLocKey;
        public string DescriptionLocKey;
        public int KillPoints;
    }
}