using System;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.MenuMain {
    public class CurrencyWidget : MonoBehaviour {
        [Dependency]
        private readonly CurrencyModule _CurrencyModule;
        
        [SerializeField]
        private CurrencyType _CurrencyType;
        [SerializeField]
        private IconLabel _IconLabel;
        [SerializeField]
        private Button _Button;

        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        private void Start() {
            _CurrencyModule.OnCurrencyChanged.Subscribe(OnCurrencyChanged);
            _Button.onClick.AddListener(OnAddButtonClick);
            var count = _CurrencyModule.GetCurrencyCount(_CurrencyType);
            _IconLabel.SetLabel(count.ToString());
        }

        private void OnDestroy() {
            _CurrencyModule.OnCurrencyChanged.Unsubscribe(OnCurrencyChanged);
            _Button.onClick.RemoveAllListeners();
        }

        private void OnCurrencyChanged(CurrencyType currencyType, int count) {
            if(currencyType != _CurrencyType)
                return;
            _IconLabel.SetLabel(count.ToString());
        }

        private void OnAddButtonClick() {
            SharedLogicCommand.CurrencyModule.AddCurrency(_CurrencyType, 100);
        }
    }
}