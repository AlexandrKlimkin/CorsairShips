using System.Collections;
using System.Collections.Generic;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.SignalBus;

namespace Game.SeaGameplay.Statistics {
    public class BattleStatisticsService : ILoadableService, IUnloadableService {

        [Dependency]
        private readonly SignalBus _SignalBus;
        
        public void Load() {

        }

        public void Unload() {

        }

        private void OnShipDieSignal() {
            
        }
    }
}
