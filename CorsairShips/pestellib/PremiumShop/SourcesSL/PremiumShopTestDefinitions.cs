using System.Collections.Generic;
using PestelLib.Serialization;
using PestelLib.SharedLogic.Modules;

namespace PestelLib.SharedLogic
{
    [System.Serializable]
    public class PremiumShopTestDefinitions
    {
        #region AUTO_GENERATED_DEFINITIONS
        [GooglePageRef("DailyRewards")]
        public List<PremiumShopItemDef> PremiumShopDefs;
        public Dictionary<string, PremiumShopItemDef> PremiumShopDefDict = new Dictionary<string, PremiumShopItemDef>();
        #endregion

        public void OnAfterDeserialize()
        {
            #region AUTO_GENERATED_DICT_INIT
            PremiumShopDefDict.Clear();
            for (int i = 0; i < PremiumShopDefDict.Count; i++)
            {
                var def = PremiumShopDefs[i];
                PremiumShopDefDict.Add(def.Id, def);
            }
            #endregion
        }
    }
}
