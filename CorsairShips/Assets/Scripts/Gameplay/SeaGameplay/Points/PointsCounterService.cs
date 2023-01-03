using System;
using System.Collections.Generic;
using UnityDI;
using UTPLib.Services;
using UTPLib.SignalBus;

namespace Game.SeaGameplay.Points {
    public class PointsCounterService : ILoadableService, IUnloadableService, IPointsCounter {
        [Dependency]
        private readonly SignalBus _SignalBus;

        [Dependency]
        private readonly ILocalShipProvider _LocalShipProvider;

        public Dictionary<byte, int> PointsDict { get; private set; }
        public event Action<byte, int> OnPointsChanged;

        public void Load() {
            PointsDict = new Dictionary<byte, int>(Ship.Ships.Count);
            _SignalBus.Subscribe<ShipDieSignal>(OnShipDie, this);
            _SignalBus.Subscribe<ShipCreatedSignal>(OnShipCreated, this);
        }

        public void Unload() {
            _SignalBus.UnSubscribeFromAll(this);
        }

        private void OnShipCreated(ShipCreatedSignal signal) {
            PointsDict.Add(signal.Ship.ShipData.ShipId, 0);
        }

        private void OnShipDie(ShipDieSignal signal) {
            if (signal.Damage == null)
                return;
            var shipCaster = signal.Damage.Caster as Ship;
            if (shipCaster == null)
                return;
            // if (shipCaster != _LocalShipProvider.LocalShip)
            //     return;
            var killedShip = signal.Damage.Receiver as Ship;
            if (killedShip == null)
                return;
            var def = killedShip.ShipDef;
            var pointsAdd = def.KillPoints;
            PointsDict[shipCaster.ShipData.ShipId] += pointsAdd;
            OnPointsChanged?.Invoke(shipCaster.ShipData.ShipId, PointsDict[shipCaster.ShipData.ShipId]);
        }
    }
}