using System.Collections;
using GoogleSpreadsheet;
using PestelLib.Serialization;

namespace PestelLib.SharedLogic.Defs
{
    [System.Serializable]
    public class LocalizationDef : IValidatableDef
    {
        public string Id;
		public string en_US;
        public string ru_RU;
        public string fr_FR;
        public string it_IT;
        public string de_DE;
        public string es_ES;
        public string es_MX;
        public string ja_JP;
        public string ko_KR;
        public string pt_PT;
        public string pt_BR;
        public string zh_CN;
        public string zh_TW;
        public string id_ID;

        public bool Validate(IList oldPage, IList newPage, IGameDefinitions newDefs, out string error) {
#if UNITY_EDITOR
            foreach (LocalizationDef l in newPage)
            {
                var fields = typeof(LocalizationDef).GetFields();
                foreach (var fieldInfo in fields)
                {
                    var val = fieldInfo.GetValue(l) as string;
                    if (val != null && val.Contains("{}"))
                    {
                        error = string.Format("Некорректный формат локализации id: '{0}' язык: '{1}'", l.Id, val);
                        return false;
                    }
                }
            }
        #endif
            error = null;
            return true;
        }
    }
}
