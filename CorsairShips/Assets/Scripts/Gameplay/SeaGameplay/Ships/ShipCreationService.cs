using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PestelLib.SharedLogic;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.Services.ResourceLoader;
using UTPLib.SignalBus;

namespace Game.SeaGameplay {
    public class ShipCreationService : ILoadableService, IUnloadableService {

        [Dependency]
        private readonly Definitions _Defs;
        [Dependency]
        private readonly IResourceLoaderService _ResourceLoader;
        [Dependency]
        private readonly SignalBus _SignalBus;
        
        private Ship _ShipBasePrefab;
        
        public class ShipCreationData {
            public string Id;
            public Vector3 Position;
            public Quaternion Rotation;
            public bool IsPlayer = false;
        }
        
        public void Load() {
            _ShipBasePrefab = _ResourceLoader.LoadResource<Ship>(ResourcePath.Ships.ShipBasePrefabPath);

            var data = new ShipCreationData {
                Id = "Sloop",
                Position = Vector3.zero,
                Rotation = Quaternion.identity,
                IsPlayer = true,
            };
            CreateShip(data);
        }

        public void Unload() {
            
        }

        public void CreateShip(ShipCreationData data) {
            var shipDef = _Defs.ShipDefs.FirstOrDefault(_ => _.Id == data.Id);
            if (shipDef == null) {
                Debug.LogError($"Ship with id {data.Id} doesnt exist");
                return;
            }
            var ship = Object.Instantiate(_ShipBasePrefab, data.Position, data.Rotation);

            var modelPath = ResourcePath.Ships.GetModelPath(shipDef.ModelId);
            var model = _ResourceLoader.LoadResourceOnScene<ShipModelController>(modelPath, ship.ModelContainer);
            
            ship.Setup(model);

            ship.gameObject.name = $"ship_{shipDef.Id}";
            
            if(data.IsPlayer)
                AddPlayerComponents(ship);
            
            _SignalBus.FireSignal(new ShipCreatedSignal(ship));
            
            if(data.IsPlayer)
                _SignalBus.FireSignal(new LocalPlayerShipCreatedSignal(ship));
        }

        private void AddPlayerComponents(Ship ship) {
            ship.gameObject.AddComponent<ShipPlayerController>();
        }
    }
}
