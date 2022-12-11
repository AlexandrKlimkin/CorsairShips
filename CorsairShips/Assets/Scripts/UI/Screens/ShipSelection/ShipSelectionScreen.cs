using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Game.SeaGameplay.Spawn;
using PestelLib.SharedLogic;
using PestelLib.SharedLogic.Modules;
using PestelLib.UI;
using UI.Screens.ShipSelection;
using UI.Widgets.Ship;
using UnityDI;
using UnityEngine;
using UTPLib.SignalBus;

namespace UI.Screens {
    public class ShipSelectionScreen : MonoBehaviour {
        [Dependency]
        private readonly Definitions _Defs;
        [Dependency]
        private readonly ShipsSpawnService _ShipsSpawnService;
        [Dependency]
        private readonly Gui _Gui;
        [Dependency]
        private SignalBus _SignalBus;
        
        [SerializeField]
        private ShipWidget _ShipWidget;
        [SerializeField]
        private RectTransform _WidgetsHost;

        private MonoBehaviourPool<ShipWidget> _WidgetsPool;

        private List<ShipWidget> _ActiveWidgets = new();

        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
            _WidgetsPool = new MonoBehaviourPool<ShipWidget>(_ShipWidget, _WidgetsHost);
        }

        private void Start() {
            _WidgetsPool.ReturnAllToPool();
            _ActiveWidgets.Clear();
            foreach (var shipDef in _Defs.ShipDefs) {
                SetupWidget(shipDef);
            }
        }

        private void SetupWidget(ShipDef shipDef) {
            var widget = _WidgetsPool.GetObject();
            widget.Setup(shipDef);
            _ActiveWidgets.Add(widget);
            widget.OnButtonClick += OnWidgetClick;
        }

        private void OnWidgetClick(ShipWidget shipWidget) {
            _ShipsSpawnService.LocalPlayerShipId = shipWidget.ShipDef.Id;
            _Gui.Close(gameObject);
            _SignalBus.FireSignal(new ShipSelectedSignal(shipWidget.ShipDef.Id));
        }
    }
}
