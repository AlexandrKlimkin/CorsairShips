using System.Collections.Generic;
using System.Linq;
using UnityDI;
using UnityEngine;
using UTPLib.Services;

namespace Game.SeaGameplay.Spawn {
    public class ShipsSpawnService : ILoadableService, IUnloadableService {
        [Dependency]
        private readonly SpawnPointsContainer _SpawnPointsContainer;
        [Dependency]
        private readonly ShipCreationService _ShipCreationService;

        private List<SpawnPoint> _FreeSpawnPoints;

        public const int EnemiesCount = 5;
        
        public void Load() {
            _FreeSpawnPoints = _SpawnPointsContainer.SpawnPoints.ToList();
            SpawnLocalPlayerShip();
            for(var i = 0; i <EnemiesCount; i++)
                SpawnEnemyShip();
        }

        public void Unload() {
            
        }

        public void SpawnLocalPlayerShip() {
            var shipData = new ShipData() {
                ShipId = "Sloop",
                IsPlayer = true,
            };
            SpawnShipInRandomPoint(shipData);
        }

        public void SpawnEnemyShip() {
            var shipData = new ShipData() {
                ShipId = "Sloop",
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
            _ShipCreationService.CreateShip(creationData);
        }
    }
}