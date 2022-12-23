using System;
using System.Collections;
using System.Collections.Generic;
using Menu.Hagar;
using PestelLib.SharedLogic;
using UI.Widgets.Ship;
using UnityDI;
using UnityEngine;

namespace UI.Screens.MenuMain {
    public class MenuMainScreen : MonoBehaviour {
        [Dependency]
        private readonly Definitions _Defs;
        [Dependency]
        private readonly HangarService _HangarService;
        
        [SerializeField]
        private ShipsRibbon _ShipsRibbon;

        private void Start() {
            ContainerHolder.Container.BuildUp(this);
            _ShipsRibbon.SetupRibbon(_Defs.ShipDefs);
            _ShipsRibbon.WidgetClickEvent += ShipsRibbonOnWidgetClickEvent;
        }

        private void OnDestroy() {
            _ShipsRibbon.WidgetClickEvent -= ShipsRibbonOnWidgetClickEvent;
        }

        private void ShipsRibbonOnWidgetClickEvent(ShipWidget shipWidget) {
            _HangarService.SetShip(shipWidget.ShipDef.Id);
        }
    }
}
