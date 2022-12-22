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
        private ShipsRibbon _ShipsRibbon;
        
        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        private void Start() {
            _ShipsRibbon.WidgetClickEvent += OnWidgetClick;
            _ShipsRibbon.SetupRibbon(_Defs.ShipDefs);
        }

        private void OnDestroy() {
            _ShipsRibbon.WidgetClickEvent -= OnWidgetClick;
        }

        private void OnWidgetClick(ShipWidget shipWidget) {
            _ShipsSpawnService.LocalPlayerShipId = shipWidget.ShipDef.Id;
            _Gui.Close(gameObject);
            _SignalBus.FireSignal(new ShipSelectedSignal(shipWidget.ShipDef.Id));
        }
    }
}
