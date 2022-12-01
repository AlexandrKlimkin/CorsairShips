using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.SeaGameplay.AI;
using Game.SeaGameplay.Data;
using PestelLib.SharedLogic;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.Services.ResourceLoader;
using UTPLib.SignalBus;

namespace Game.SeaGameplay {
    public class ShipCreationService : ILoadableService, IUnloadableService, ILocalShipProvider {
        [Dependency]
        private readonly Definitions _Defs;
        [Dependency]
        private readonly IResourceLoaderService _ResourceLoader;
        [Dependency]
        private readonly SignalBus _SignalBus;
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

        public void CreateShip(ShipCreationData creationData) {
            var shipData = creationData.ShipData;
            if(shipData == null)
                return;
            var shipDef = _Defs.ShipDefs.FirstOrDefault(_ => _.Id == shipData.ShipId);
            if (shipDef == null) {
                return;
            }
            var ship = Object.Instantiate(_ShipBasePrefab, creationData.Position, creationData.Rotation);

            var modelPath = ResourcePath.Ships.GetModelPath(shipDef.ModelId);
            var model = _ResourceLoader.LoadResourceOnScene<ShipModelController>(modelPath, ship.ModelContainer);
            
            ship.Setup(shipData, shipDef, model);

            ship.gameObject.name = $"ship_{shipDef.Id}";

            if (shipData.IsPlayer) {
                AddPlayerComponents(ship);
                LocalShip = ship;
                LocalShip.OnDestroy += OnLocalShipDestroy;
            }
            else {
                AddBotComponents(ship);
            }
            
            _SignalBus.FireSignal(new ShipCreatedSignal(ship));
            
            if(shipData.IsPlayer)
                _SignalBus.FireSignal(new LocalPlayerShipCreatedSignal(ship));
        }

        private void AddPlayerComponents(Ship ship) {
            ship.gameObject.AddComponent<ShipPlayerController>();
        }
        
        private void AddBotComponents(Ship ship) {
            var aiPrefab = _ResourceLoader.LoadResource<BotAIController>(ResourcePath.Ships.GetAIPath("ShipBotAI"));
            _AIService.MakeShipAI(ship, aiPrefab);
        }


        private void OnLocalShipDestroy(Ship ship) {
            if(LocalShip != ship)
                return;
            LocalShip.OnDestroy -= OnLocalShipDestroy;
            LocalShip = null;
        }
    }

    public class ShipCreationData {
        public ShipData ShipData;
        public Vector3 Position;
        public Quaternion Rotation;
    }
}
