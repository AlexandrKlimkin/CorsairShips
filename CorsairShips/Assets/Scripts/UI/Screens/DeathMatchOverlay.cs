using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.SeaGameplay;
using Game.SeaGameplay.GameModes;
using Game.SeaGameplay.Statistics;
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
        [Dependency]
        private readonly BattleStatisticsService _Statistics;
        
        [SerializeField]
        private TextMeshProUGUI _ShipsCountText;
        [SerializeField]
        private TextMeshProUGUI _KillsText;
        
        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        private void Start() {
            _SignalBus.Subscribe<ShipCreatedSignal>(OnShipCreated, this);
            _SignalBus.Subscribe<ShipDieSignal>(OnShipDead, this);
            _Statistics.KillsChanged += OnKillsChanged;
            RefreshShipsCount();
            RefreshKills();
        }

        private void OnDestroy() {
            _SignalBus.UnSubscribeFromAll(this);
            _Statistics.KillsChanged -= OnKillsChanged;
        }

        private void RefreshShipsCount() {
            var aliveShipsCount = Ship.Ships.Count(_ => !_.Dead);
            _ShipsCountText.text = $"{aliveShipsCount}/{DeathMatchConfig.Instance.DeathMatchParameters.PlayersCount}";
        }
        
        private void OnShipCreated(ShipCreatedSignal signal) {
            
        }

        private void OnShipDead(ShipDieSignal signal) {
            RefreshShipsCount();
        }

        private void OnKillsChanged(int kills) {
            RefreshKills();
        }

        private void RefreshKills() {
            _KillsText.text = _Statistics.Kills.ToString();
        }
    }
}
