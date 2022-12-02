using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.SeaGameplay;
using Game.SeaGameplay.GameModes;
using TMPro;
using UnityDI;
using UnityEngine;
using UTPLib.SignalBus;

namespace UI.Battle {
    public class DeathMatchOverlay : MonoBehaviour {
        [Dependency]
        private readonly SignalBus _SignalBus;
        [Dependency]
        private readonly DeathMatchService _DeathMatchService;
        
        [SerializeField]
        private TextMeshProUGUI _ShipsCountText;
        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        private void Start() {
            _SignalBus.Subscribe<ShipCreatedSignal>(OnShipCreated, this);
            _SignalBus.Subscribe<ShipDieSignal>(OnShipDead, this);
            RefreshShipsCount();
        }

        private void OnDestroy() {
            _SignalBus.UnSubscribeFromAll(this);
        }

        private void RefreshShipsCount() {
            var aliveShipsCount = Ship.Ships.Count(_ => !_.Dead);
            _ShipsCountText.text = $"{aliveShipsCount}/{DeathMatchService.PlayersCount}";
        }
        
        private void OnShipCreated(ShipCreatedSignal signal) {
            
        }

        private void OnShipDead(ShipDieSignal signal) {
            RefreshShipsCount();
        }
    }
}
