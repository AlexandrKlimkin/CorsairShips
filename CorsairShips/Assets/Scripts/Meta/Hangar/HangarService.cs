using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.SeaGameplay;
using Game.SeaGameplay.Spawn;
using PestelLib.SharedLogic;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.SignalBus;

namespace Menu.Hagar {
    public class HangarService : ILoadableService, IUnloadableService {
        [Dependency]
        private readonly HangarSceneDataContainer _SceneData;
        [Dependency]
        private readonly Definitions _Defs;
        [Dependency]
        private readonly ShipCreationService _ShipCreationService;
        [Dependency]
        private readonly SignalBus _SignalBus;
        
        private readonly Dictionary<string, ShipModelController> _ShipModelsDict = new();

        public ShipModelController SelectedShipModel { get; private set; }
        
        public void Load() {
            var firstShipDef = _Defs.ShipDefs.FirstOrDefault();
            if(firstShipDef == null)
                return;
            SetShip(firstShipDef.Id);
        }

        public void Unload() {
            
        }

        public void SetShip(string shipDefId) {
            var shipModel = _ShipModelsDict.ContainsKey(shipDefId) ? _ShipModelsDict[shipDefId] : CreateShipModel(shipDefId);
            _ShipModelsDict.ForEach(_ => _.Value.gameObject.SetActive(_.Value == shipModel));
            SelectedShipModel = shipModel;
            _SignalBus.FireSignal(new HangarShipChanged(shipModel));
        }

        private ShipModelController CreateShipModel(string shipDefId) {
            var def = _Defs.ShipDefs.FirstOrDefault(_ => _.Id == shipDefId);
            if(def == null)
                return null;
            var model = _ShipCreationService.CreateShipModel(def.ModelId, _SceneData.ShipSpawnPoint);
            _ShipModelsDict.Add(shipDefId, model);
            return model;
        }
    }
}
