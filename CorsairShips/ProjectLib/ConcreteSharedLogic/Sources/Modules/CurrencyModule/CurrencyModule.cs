using System.Collections.Generic;
using System.Linq;
using PestelLib.SharedLogicBase;
using UnityDI;

namespace PestelLib.SharedLogic.Modules {
    public class CurrencyModule : SharedLogicModule<CurrencyModuleState> {

        [Dependency]
        private readonly ItemsModule _ItemsModule;
        [Dependency]
        private readonly List<CurrencyDef> _CurrencyDefs;

        public ScheduledAction<CurrencyType, int> OnCurrencyChanged;
        
        public CurrencyModule() {
            OnCurrencyChanged = new ScheduledAction<CurrencyType, int>(ScheduledActionCaller);
        }

        public int GetCurrencyCount(CurrencyType currencyType) {
            var itemId = GetItemIdByCurrencyType(currencyType);
            if (itemId == null)
                return 0;
            return _ItemsModule.GetItemsCount(itemId);
        }

        public bool CheckCanSpend(CurrencyType currencyType, int count) {
            var itemId = GetItemIdByCurrencyType(currencyType);
            if (itemId == null)
                return false;
            return _ItemsModule.CheckCanSpend(itemId, count);
        }
        
        [SharedCommand]
        internal void AddCurrency(CurrencyType currencyType, int count) {
            var itemId = GetItemIdByCurrencyType(currencyType);
            if (itemId == null)
                return;
            _ItemsModule.AddItem(itemId, count);
            var finalCount = _ItemsModule.GetItemsCount(itemId);
            OnCurrencyChanged?.Schedule(currencyType, finalCount);
        }
        
        [SharedCommand]
        internal void SpendCurrency(CurrencyType currencyType, int count) {
            var canSpend = CheckCanSpend(currencyType, count);
            if (canSpend) {
                var itemId = GetItemIdByCurrencyType(currencyType);
                _ItemsModule.SpendItem(itemId, count);
                var finalCount = _ItemsModule.GetItemsCount(itemId);
                OnCurrencyChanged?.Schedule(currencyType, finalCount);
            }
        }

        public bool TryGetCurrencyTypeByItemId(string itemId, out CurrencyType? currencyType) {
            currencyType = null;
            var firstCurrency = _CurrencyDefs.FirstOrDefault(_ => _.ItemId == itemId);
            if (firstCurrency != null)
                currencyType = firstCurrency.CurrencyType;
            return firstCurrency != null;
        }
        
        public string GetItemIdByCurrencyType(CurrencyType currencyType) {
            var def = _CurrencyDefs.FirstOrDefault(_ => _.CurrencyType == currencyType);
            return def?.ItemId;
        }
    }
}