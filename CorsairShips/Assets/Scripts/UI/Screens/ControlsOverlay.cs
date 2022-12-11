using System;
using System.Collections;
using System.Collections.Generic;
using Game.SeaGameplay;
using Game.SeaGameplay.UI;
using UnityDI;
using UnityEngine;
using UTPLib.SignalBus;

namespace UI.Battle {
    public class ControlsOverlay : MonoBehaviour {
        [Dependency]
        private readonly ILocalShipProvider _LocalShipProvider;
        [Dependency]
        private readonly SignalBus _SignalBus;
        
        [SerializeField]
        private ActionButton _FireButton;

        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        private void Start() {
            if(_LocalShipProvider.LocalShip == null)
                _SignalBus.Subscribe<LocalPlayerShipCreatedSignal>(OnLocalPlayerShipCreatedSignal, this);
            else {
                SetupFireButton();
            }
            _SignalBus.Subscribe<ShipDieSignal>(OnShipDeadSignal, this);
        }

        private void OnDestroy() {
            _SignalBus.UnSubscribeFromAll(this);
        }

        private void SetupFireButton() {
            _FireButton.Setup(() => _LocalShipProvider.LocalShip.WeaponController.NormilizedCD);
        }

        private void OnLocalPlayerShipCreatedSignal(LocalPlayerShipCreatedSignal signal) {
            SetupFireButton();
        }

        private void OnShipDeadSignal(ShipDieSignal signal) {
            if(_LocalShipProvider.LocalShip == null)
                return;
            if(_LocalShipProvider.LocalShip != signal.Ship)
                return;
            _FireButton.Clear();
        }
    }
}
