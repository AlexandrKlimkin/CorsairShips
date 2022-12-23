using System.Linq;
using Game.SeaGameplay.AI;
using Game.SeaGameplay.Data;
using PestelLib.SharedLogic;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.Services.ResourceLoader;

namespace Game.SeaGameplay {
    public class ShipCreationService : ILoadableService, IUnloadableService, ILocalShipProvider {
        [Dependency]
        private readonly Definitions _Defs;
        [Dependency]
        private readonly IResourceLoaderService _ResourceLoader;
        [Dependency]
        private readonly AIService _AIService;
        
        private Ship _ShipBasePrefab;

        public Ship LocalShip { get; private set; }
        
        public void Load() {
            ContainerHolder.Container.RegisterInstance<ILocalShipProvider>(this);
            _ShipBasePrefab = _ResourceLoader.LoadResource<Ship>(ResourcePath.Ships.ShipBasePrefabPath);
        }

        public void Unload() {
            
        }

        public Ship CreateShip(ShipCreationData creationData) {
            var shipData = creationData.ShipData;
            if(shipData == null)
                return null;
            var shipDef = _Defs.ShipDefs.FirstOrDefault(_ => _.Id == shipData.ShipDefId);
            if (shipDef == null) {
                return null;
            }
            var ship = Object.Instantiate(_ShipBasePrefab, creationData.Position, creationData.Rotation);

            var model = CreateShipModel(shipDef.ModelId, ship.ModelContainer);

            ship.Setup(shipData, shipDef, model);

            ship.gameObject.name = $"ship_{shipDef.Id}";

            if (shipData.IsPlayer) {
                AddPlayerComponents(ship);
                LocalShip = ship;
                LocalShip.OnShipDestroy += OnShipLocalShipDestroy;
            }
            else {
                AddBotComponents(ship);
            }
            return ship;
        }

        public ShipModelController CreateShipModel(string modelId, Transform parent) {
            var modelPath = ResourcePath.Ships.GetModelPath(modelId);
            var model = _ResourceLoader.LoadResourceOnScene<ShipModelController>(modelPath, parent);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = parent.localScale;
            return model;
        }

        private void AddPlayerComponents(Ship ship) {
            ship.gameObject.AddComponent<ShipPlayerController>();
            // var aimController = ship.WeaponController.gameObject.AddComponent<AimController>();
        }
        
        private void AddBotComponents(Ship ship) {
            var aiPrefab = _ResourceLoader.LoadResource<BotAIController>(ResourcePath.Ships.GetAIPath("ShipBotAI"));
            _AIService.MakeShipAI(ship, aiPrefab);
        }
        
        private void OnShipLocalShipDestroy(Ship ship) {
            if(LocalShip != ship)
                return;
            LocalShip.OnShipDestroy -= OnShipLocalShipDestroy;
            LocalShip = null;
        }
    }

    public class ShipCreationData {
        public ShipData ShipData;
        public Vector3 Position;
        public Quaternion Rotation;
    }
}
