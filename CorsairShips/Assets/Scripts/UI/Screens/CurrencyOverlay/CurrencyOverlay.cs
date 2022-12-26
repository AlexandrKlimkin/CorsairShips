using System;
using System.Collections;
using System.Collections.Generic;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.MenuMain {
    public class CurrencyOverlay : MonoBehaviour {
        [Dependency]
        private readonly CurrencyModule _CurrencyModule;

        [SerializeField]
        private List<CurrencyWidget> _CurrencyList;

        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        private void Start() {
            _CurrencyList.ForEach(_ => {
                _.SetCount(_CurrencyModule.GetCurrencyCount(_.CurrencyType));
                _.OnButtonClick += OnCurrencyWidgetClick;
            });
            _CurrencyModule.OnCurrencyChanged.Subscribe(OnCurrencyChanged);
        }

        private void OnDestroy() {
            _CurrencyList.ForEach(_ => _.OnButtonClick -= OnCurrencyWidgetClick);
            _CurrencyModule.OnCurrencyChanged.Unsubscribe(OnCurrencyChanged);
        }

        private void OnCurrencyChanged(CurrencyType currencyType, int count) {
            foreach (var widget in _CurrencyList) {
                if(widget.CurrencyType != currencyType)
                    continue;
                widget.SetCount(count);
            }
        }

        private void OnCurrencyWidgetClick(CurrencyWidget widget) {
            SharedLogicCommand.CurrencyModule.AddCurrency(widget.CurrencyType, 100);
        }
    }
}
