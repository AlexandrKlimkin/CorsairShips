using System.Collections.Generic;
using PestelLib.Serialization;
using PestelLib.SharedLogicBase;
using UnityDI;

namespace PestelLib.SharedLogic.Modules
{
    public class PropertyModule : SharedLogicModule<PropertyModuleState>
    {
#pragma warning disable 649, 169
        [Dependency] private MoneyModule _moneyModule;

        [GooglePageRef("Property")]
        [Dependency] private List<PropertyDef> _defs;

        [Dependency] private Dictionary<string, PropertyDef> _propertyDict;
#pragma warning restore 649, 169

        public ScheduledAction<string> OnGotProperty;
        public ScheduledAction<string> OnBuyProperty;
        public ScheduledAction OnFailedToBuy;

        public ScheduledAction OnItemSpotted;

        public PropertyModule()
        {
            OnGotProperty = new ScheduledAction<string>(ScheduledActionCaller);
            OnBuyProperty = new ScheduledAction<string>(ScheduledActionCaller);
            OnItemSpotted = new ScheduledAction(ScheduledActionCaller);
            OnFailedToBuy = new ScheduledAction(ScheduledActionCaller);
        }

        public override void MakeDefaultState()
        {
            State = new PropertyModuleState
            {
                UnlockedLockers = new List<string>(),
                SpottedItems = new List<string>(),
                OwnedProperty = new List<string>()
            };
        }

        [SharedCommand]
        internal void BuyProperty(string propertyId)
        {
            if (string.IsNullOrEmpty(propertyId)) return;

            if (IsPropertyOwned(propertyId)) return;

            var def = _propertyDict[propertyId];

            if (def == null) return;

            bool success = false;
            if (def.PriceIngame > 0)
            {
                if (_moneyModule.Ingame >= def.PriceIngame)
                {
                    _moneyModule.SpendIngame(def.PriceIngame);
                    success = true;
                }
                else
                {
                    OnFailedToBuy.Schedule();
                }
            }
            else if (def.PriceReal > 0)
            {
                if (_moneyModule.Real >= def.PriceReal)
                {
                    _moneyModule.SpendReal(def.PriceReal);
                    success = true;
                }
                else
                {
                    OnFailedToBuy.Schedule();
                }
            }

            if (!success) return;

            SetPropertyAsOwned(def.Id);
            OnBuyProperty.Schedule(def.Id);
        }

        [SharedCommand]
        internal void SetPropertyAsOwned(string propertyId)
        {
            if (!IsPropertyOwned(propertyId))
            {
                State.OwnedProperty.Add(propertyId);
            }
            else
            {
                Log(propertyId + " is already owned");
            }
        }

        [SharedCommand]
        internal void SetSpotted(string id)
        {
            if (IsItemSpotted(id)) return;

            State.SpottedItems.Add(id);
            OnItemSpotted.Schedule();
        }

        public bool IsItemSpotted(string id)
        {
            return State.SpottedItems.Contains(id);
        }

        public bool IsPropertyOwned(string propertyId)
        {
            return string.IsNullOrEmpty(propertyId) || State.OwnedProperty.Contains(propertyId);
        }
    }
}