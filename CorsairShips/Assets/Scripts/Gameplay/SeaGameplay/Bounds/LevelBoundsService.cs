using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Dmg;
using PestelLib.Localization;
using PestelLib.UI;
using PestelLib.Utils;
using UI.Battle;
using UI.FloatingTexts;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.SignalBus;

namespace Game.SeaGameplay.Bounds {
    public class LevelBoundsService : Scheduler<IDamageable>, ILoadableService, IUnloadableService, IDamageCaster {
        [Dependency]
        private readonly SignalBus _SignalBus;
        [Dependency]
        private readonly DamageService _DamageService;
        [Dependency]
        private readonly LevelBounds _LevelBounds;
        [Dependency]
        private readonly UnityEventsProvider _EventsProvider;
        [Dependency]
        private readonly ILocalization _Localization;
        [Dependency]
        private readonly Gui _Gui;
        [Dependency]
        private readonly ILocalShipProvider _LocalShipProvider;
        
        public byte DamageCasterId  => 100;
        private float DamagePeriod => 2f;
        
        private Dictionary<IDamageable, float> _LastTimeInsideDict = new ();
        protected override float ObjectsPerFrame => 2;
        private const float WarningTime = 5f;
        private bool _NotificationPlayed;

        private UnityEngine.Camera _Camera;
        
        public void Load() {
            _Camera = UnityEngine.Camera.main;
            _SignalBus.Subscribe<ShipCreatedSignal>(OnShipRegister, this);
            _SignalBus.Subscribe<LocalPlayerShipCreatedSignal>(OnLocalShipRegister, this);
            _DamageService.RegisterCaster(this);
            Play(_EventsProvider);
        }

        public void Unload() {
            Stop();
            _DamageService.UnregisterCaster(this);
            _SignalBus.UnSubscribeFromAll(this);
            HideWarningNotification();
        }
        
        protected override void UpdateObject(IDamageable target) {
            if (target as Ship == null)
                return;
            var ship = (Ship)target;
            var inBounds = _LevelBounds.PositionInBounds(target.Collider.transform.position);
            if (inBounds) {
                _LastTimeInsideDict[target] = Time.time;
                if (ship.IsLocalPlayerShip && _NotificationPlayed) {
                    HideWarningNotification();
                    _NotificationPlayed = false;
                }
                return;
            }
            if(ship.Dead)
                return;
            if (Time.time - _LastTimeInsideDict[target] > WarningTime) {
                var lastTimeCasters = ship.DamageBuffer.GetCastersForLastTime(DamagePeriod).ToList();
                if (lastTimeCasters.Count == 0 || !lastTimeCasters.Contains(this)) {
                    var dmg = new Damage {
                        Amount = 30,
                        CasterId = DamageCasterId,
                        ReceiverId = ship.DamageableId,
                    };
                    _DamageService.ApplyDamage(dmg);
                    //Debug.LogError("Bounds apply dmg");
                }
            }
            else {
                if (ship.IsLocalPlayerShip && !_NotificationPlayed) {
                    ShowWarningNotification();
                    _NotificationPlayed = true;
                }
            }
        }
        private void RegisterShip(Ship ship, float time) {
            Register(ship);
            ship.OnShipDestroy += OnShipDestroyed;
            ship.OnDie += ShipOnOnDie;
            _LastTimeInsideDict.Add(ship, time);
        }
        private void UnregisterShip(Ship ship) {
            Unregister(ship);
            ship.OnShipDestroy -= OnShipDestroyed;
            ship.OnDie -= ShipOnOnDie;
            if(ship.IsLocalPlayerShip)
                HideWarningNotification();
        }
        private void OnShipRegister(ShipCreatedSignal signal) {
            RegisterShip(signal.Ship, signal.Time);
        }
        private void ShipOnOnDie(Ship ship) {
            UnregisterShip(ship);
        }
        private void OnShipDestroyed(Ship ship) {
            UnregisterShip(ship);
        }


        private IEnumerator UpdateRoutine() {
            while (true) {
                //ToDo: Set bounds color
                yield return null;
            }
        }
        private void OnLocalShipRegister(LocalPlayerShipCreatedSignal signal) {
            
        }

        #region Warning
        
        private WarningNotification _Warning;
        private void ShowWarningNotification() {
            _Warning = _Gui.Show<WarningNotification>(GuiScreenType.Overlay);
            _Warning.Setup(_Localization.Get(LocalizationKeys.OutOfBoundsKey));
        }

        private void HideWarningNotification() {
            if(_Warning != null)
                _Gui.Hide(_Warning.gameObject);
        }
        #endregion
    }
}