using System;
using System.Collections;
using System.Collections.Generic;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.SignalBus;

namespace Game.SeaGameplay.Statistics {
    public class BattleStatisticsService : ILoadableService, IUnloadableService {
        [Dependency]
        private readonly SignalBus _SignalBus;
        
        public int Kills { get; private set; }
        public event Action<int> KillsChanged;
        
        public void Load() {
            _SignalBus.Subscribe<ShipDieSignal>(OnShipDieSignal, this);
        }

        public void Unload() {
            _SignalBus.UnSubscribeFromAll(this);
        }

        private void OnShipDieSignal(ShipDieSignal signal) {
            var instigator = signal.Damage.Caster as Ship;
            if(instigator == null)
                return;
            if(!instigator.IsLocalPlayerShip)
                return;
            Kills++;
            KillsChanged?.Invoke(Kills);
        }
    }
}
