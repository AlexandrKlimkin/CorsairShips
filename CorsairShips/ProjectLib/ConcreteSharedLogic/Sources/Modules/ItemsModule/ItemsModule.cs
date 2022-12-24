using System.Collections.Generic;
using System.Linq;
using PestelLib.SharedLogicBase;
using UnityDI;

namespace PestelLib.SharedLogic.Modules {
    public class ItemsModule : SharedLogicModule<ItemsModuleState> {
        [Dependency]
        private readonly List<ItemDef> _ItemDefs;
        
        public ScheduledAction<string, int> OnItemChanged;

        public ItemsModule() {
            OnItemChanged = new ScheduledAction<string, int>(ScheduledActionCaller);
        }

        public override void MakeDefaultState() {
            base.MakeDefaultState();
            State.Items = new List<ItemState>();
        }

        public int GetItemsCount(string itemId) {
            var itemDef = _ItemDefs.FirstOrDefault(_ => _.Id == itemId);
            if (itemDef == null)
                return 0;
            var state = State.Items.FirstOrDefault(_ => _.ItemId == itemId);
            if (state == null)
                return 0;
            return state.Count;
        }
        
        public bool CheckCanSpend(string itemId, int count) {
            if (count <= 0)
                return false;
            
            var itemDef = _ItemDefs.FirstOrDefault(_ => _.Id == itemId);
            if (itemDef == null)
                return false;
            var state = State.Items.FirstOrDefault(_ => _.ItemId == itemId);
            if (state == null)
                return false;
            return state.Count >= count;
        }
        
        [SharedCommand]
        internal void AddItem(string itemId, int count) {
            var itemDef = _ItemDefs.FirstOrDefault(_ => _.Id == itemId);
            if (itemDef == null)
                return;
            if(count <= 0)
                return;
            var existItem = State.Items.FirstOrDefault(_ => _.ItemId == itemId);
            if (existItem == null) {
                existItem = new ItemState {
                    ItemId = itemId,
                    Count = count,
                };
                    State.Items.Add(existItem);
            }
            else {
                existItem.Count += count;
            }
            OnItemChanged?.Schedule(existItem.ItemId, existItem.Count);
        }

        [SharedCommand]
        internal void SpendItem(string itemId, int count) {
            if(count <= 0)
                return;
            var canSpend = CheckCanSpend(itemId, count);
            if (canSpend) {
                var existItem = State.Items.First(_ => _.ItemId == itemId);
                existItem.Count -= count;
                OnItemChanged?.Schedule(existItem.ItemId, existItem.Count);
            }
        }
    }
}