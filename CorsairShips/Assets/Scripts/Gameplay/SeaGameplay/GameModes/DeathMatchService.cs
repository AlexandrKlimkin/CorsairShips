using System.Linq;
using Game.SeaGameplay.Spawn;
using PestelLib.UI;
using UI.Battle;
using UnityDI;
using UTPLib.Services;
using UTPLib.SignalBus;

namespace Game.SeaGameplay.GameModes {
    public class DeathMatchService : ILoadableService, IUnloadableService {

        public enum MatchResult {
            Victory,
            Defeat,
        }
        
        [Dependency]
        private readonly ShipsSpawnService _ShipsSpawnService;
        [Dependency]
        private readonly Gui _Gui;
        [Dependency]
        private readonly SignalBus _SignalBus;

        public const int PlayersCount = 6;
        
        public void Load() {
            _ShipsSpawnService.SpawnLocalPlayerShip();
            _SignalBus.Subscribe<ShipDieSignal>(OnShipDieSignal, this);
            
            for(var i = 1; i < PlayersCount; i++)
                _ShipsSpawnService.SpawnEnemyShip();
            _Gui.Show<DeathMatchOverlay>(GuiScreenType.Overlay);
        }

        public void Unload() {
            _SignalBus.UnSubscribeFromAll(this);
        }

        private void FinishMatch(MatchResult result) {
            var endWindow = _Gui.Show<DeathMatchResultWindow>(GuiScreenType.Dialog);
            endWindow.Setup(result);
        }
        
        private void OnShipDieSignal(ShipDieSignal signal) {
            if (signal.Ship.IsLocalPlayerShip) {
                FinishMatch(MatchResult.Defeat);
            }
            else {
                var enemiesAlive = Ship.Ships.Any(_ => !_.Dead && !_.IsLocalPlayerShip);
                if(enemiesAlive)
                    return;
                FinishMatch(MatchResult.Victory);
            }
        }
    }
}