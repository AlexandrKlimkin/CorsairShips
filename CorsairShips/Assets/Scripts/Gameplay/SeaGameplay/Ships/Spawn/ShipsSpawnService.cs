using System.Collections.Generic;
using System.Linq;
using Game.SeaGameplay.Data;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.SignalBus;

namespace Game.SeaGameplay.Spawn {
    public class ShipsSpawnService : ILoadableService, IUnloadableService, ILocalShipProvider, IListStorage<Ship> {
        [Dependency]
        private readonly SpawnPointsContainer _SpawnPointsContainer;
        [Dependency]
        private readonly ShipCreationService _ShipCreationService;
        [Dependency]
        private readonly SignalBus _SignalBus;
        
        private List<SpawnPoint> _FreeSpawnPoints;

        public Ship LocalShip { get; private set; }

        public List<Ship> Storage { get; private set; } = new List<Ship>();
        
        public void Load() {
            ContainerHolder.Container.RegisterInstance<ILocalShipProvider>(this);
            ContainerHolder.Container.RegisterInstance<IListStorage<Ship>>(this);
            _FreeSpawnPoints = _SpawnPointsContainer.SpawnPoints.ToList();
        }

        public void Unload() {
            
        }

        public void SpawnLocalPlayerShip() {
            var shipData = new ShipData {
                ShipId = AllocateShipId(),
                ShipDefId = "Sloop",
                IsPlayer = true,
            };
            SpawnShipInRandomPoint(shipData);
        }

        public void SpawnEnemyShip() {
            var shipData = new ShipData {
                ShipId = AllocateShipId(),
                ShipDefId = "Sloop",
                IsPlayer = false,
            };
            SpawnShipInRandomPoint(shipData);
        }

        private void SpawnShipInRandomPoint(ShipData shipData) {
            var randIndex = Random.Range(0, _FreeSpawnPoints.Count);
            var spawnPoint = _FreeSpawnPoints[randIndex];
            _FreeSpawnPoints.Remove(spawnPoint);
            var creationData = new ShipCreationData {
                ShipData = shipData,
                Position = spawnPoint.transform.position,
                Rotation = spawnPoint.transform.rotation,
            };
            var ship = _ShipCreationService.CreateShip(creationData);
            if (ship.ShipData.IsPlayer) {
                LocalShip = ship;
                LocalShip.OnDestroy += OnLocalShipDestroy;
            }
            _SignalBus.FireSignal(new ShipCreatedSignal(ship));
            
            if(shipData.IsPlayer)
                _SignalBus.FireSignal(new LocalPlayerShipCreatedSignal(ship));
        }
        
        private void OnLocalShipDestroy(Ship ship) {
            if(LocalShip != ship)
                return;
            LocalShip.OnDestroy -= OnLocalShipDestroy;
            LocalShip = null;
        }

        private byte _CurrentAllocatedId;
        private byte AllocateShipId() {
            var id = _CurrentAllocatedId;
            _CurrentAllocatedId++;
            return id;
        }
    }
}