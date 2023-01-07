using System;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.MenuMain {
    public class CurrencyWidget : MonoBehaviour {

        [SerializeField]
        private CurrencyType _CurrencyType;
        [SerializeField]
        private IconLabel _IconLabel;
        [SerializeField]
        private Button _Button;

        public Action<CurrencyWidget> OnButtonClick;

        public CurrencyType CurrencyType => _CurrencyType;
        
        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        private void Start() {
            _Button.onClick.AddListener(OnAddButtonClick);
        }

        private void OnDestroy() {
            _Button.onClick.RemoveAllListeners();
        }

        public void Setup(CurrencyType currencyType, int count) {
            SetCount(count);
        }

        public void SetCount(int count) {
            _IconLabel.SetLabel(count.ToString());
        }

        private void OnAddButtonClick() {
            OnButtonClick?.Invoke(this);
        }
    }
}