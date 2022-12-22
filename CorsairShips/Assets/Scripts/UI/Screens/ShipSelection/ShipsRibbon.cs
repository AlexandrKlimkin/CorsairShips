using System;
using System.Collections.Generic;
using Game;
using PestelLib.SharedLogic.Modules;
using UnityEngine;

namespace UI.Widgets.Ship {
    public class ShipsRibbon : MonoBehaviour {
        [SerializeField]
        private ShipWidget _ShipWidget;
        [SerializeField]
        private RectTransform _WidgetsHost;

        private MonoBehaviourPool<ShipWidget> _WidgetsPool;
        
        private List<ShipWidget> _ActiveWidgets = new();

        private List<ShipDef> _ShipDefs;

        public event Action<ShipWidget> WidgetClickEvent;
        
        private void Awake() {
            _WidgetsPool = new MonoBehaviourPool<ShipWidget>(_ShipWidget, _WidgetsHost);
            Clear();
        }

        public void SetupRibbon(List<ShipDef> shipDefs) {
            _ShipDefs = shipDefs;
            Refresh();
        }

        private void Refresh() {
            Clear();
            if(_ShipDefs == null)
                return;
            foreach (var shipDef in _ShipDefs) {
                SetupWidget(shipDef);
            }
        }

        private void Clear() {
            _WidgetsPool.ReturnAllToPool();
            _ActiveWidgets.Clear();
        }
        
        private void SetupWidget(ShipDef shipDef) {
            var widget = _WidgetsPool.GetObject();
            widget.Setup(shipDef);
            _ActiveWidgets.Add(widget);
            widget.OnButtonClick += OnWidgetClick;
        }

        private void OnWidgetClick(ShipWidget shipWidget) {
            WidgetClickEvent?.Invoke(shipWidget);
        }
    }
}