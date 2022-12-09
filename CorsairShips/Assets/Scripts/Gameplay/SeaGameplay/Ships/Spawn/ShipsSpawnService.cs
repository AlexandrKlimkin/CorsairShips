using System.Collections.Generic;
using System.Linq;
using Game.SeaGameplay.Data;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.Services.ResourceLoader;
using UTPLib.SignalBus;

namespace Game.SeaGameplay.Spawn {
    public class ShipsSpawnService : ILoadableService, IUnloadableService {
        [Dependency]
        private readonly SpawnPointsContainer _SpawnPointsContainer;
        [Dependency]
        private readonly ShipCreationService _ShipCreationService;
        [Dependency]
        private readonly SignalBus _SignalBus;
        [Dependency]
        private readonly IResourceLoaderService _ResourceLoader; 
        
        private List<SpawnPoint> _FreeSpawnPoints;
        public ShipsSpawnConfig Config { get; private set; }

        public void Load() {
            Config = _ResourceLoader.LoadResource<ShipsSpawnConfig>(ResourcePath.Spawn.ShipsSpawnConfigPath);
            ContainerHolder.Container.RegisterInstance(Config);
            _FreeSpawnPoints = _SpawnPointsContainer.SpawnPoints.ToList();
        }

        public void Unload() {
            
        }

        public void SpawnLocalPlayerShip() {
            var shipData = new ShipData {
                ShipId = AllocateShipId(),
                ShipDefId = Config.DefaultLocalPlayerShipId,
                IsPlayer = true,
            };
            SpawnShipInRandomPoint(shipData);
        }

        public void SpawnEnemyShip(string shipId) {
            var shipData = new ShipData {
                ShipId = AllocateShipId(),
                ShipDefId = shipId,
                IsPlayer = false,
            };
            SpawnShipInRandomPoint(shipData);
        }

        public void SpawnEnemyShip() {
            SpawnEnemyShip(Config.DefaultEnemyShipId);
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
            
            _SignalBus.FireSignal(new ShipCreatedSignal(ship));
            
            if(shipData.IsPlayer)
                _SignalBus.FireSignal(new LocalPlayerShipCreatedSignal(ship));
        }

        private byte _CurrentAllocatedId;
        private byte AllocateShipId() {
            var id = _CurrentAllocatedId;
            _CurrentAllocatedId++;
            return id;
        }
    }
}