using System.Collections.Generic;
using System.Linq;
using Game.Dmg;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.SignalBus;

namespace Game.SeaGameplay.Bounds {
    public class LevelBoundsService : Scheduler<IDamageable>, ILoadableService, IUnloadableService, IDamageCaster {
        [Dependency]
        private readonly SignalBus _SignalBus;
        
        private Dictionary<IDamageable, float> _LastTimeInsideDict = new ();
        // private Dictionary<IDamageable, float>
        protected override float ObjectsPerFrame => 2;
        private const float WarningTime = 5f;
        
        protected override void UpdateObject(IDamageable target) {
            if (Time.time - _LastTimeInsideDict[target] > WarningTime) {
                if (target as Ship != null) {
                    var ship = (Ship)target;
                    var lastTimeCasters = ship.DamageBuffer.GetCastersForLastTime(1f).ToList();
                    
                }
            }
        }
        
        private void OnShipRegister(ShipCreatedSignal signal) {
            Register(signal.Ship);
            signal.Ship.OnShipDestroy += OnShipDestroyed;
            _LastTimeInsideDict.Add(signal.Ship, signal.Time);
        }

        private void OnShipDestroyed(Ship ship) {
            Unregister(ship);
            ship.OnShipDestroy -= OnShipDestroyed;
        }
        
        private void OnLocalShipRegister(LocalPlayerShipCreatedSignal signal) {
            
        }

        public void Load() {
            _SignalBus.Subscribe<ShipCreatedSignal>(OnShipRegister, this);
            _SignalBus.Subscribe<LocalPlayerShipCreatedSignal>(OnLocalShipRegister, this);
        }

        public void Unload() {
            _SignalBus.UnSubscribeFromAll(this);
        }
        
        private struct TargetData {
            
        }

        public byte DamageCasterId { get; }
    }
}