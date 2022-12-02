using Game.SeaGameplay.Spawn;
using PestelLib.UI;
using UI.Battle;
using UnityDI;
using UTPLib.Services;

namespace Game.SeaGameplay.GameModes {
    public class DeathMatchService : ILoadableService, IUnloadableService {
        [Dependency]
        private readonly ShipsSpawnService _ShipsSpawnService;
        [Dependency]
        private readonly Gui _Gui;

        public const int PlayersCount = 6;
        
        public void Load() {
            _ShipsSpawnService.SpawnLocalPlayerShip();
            for(var i = 1; i < PlayersCount; i++)
                _ShipsSpawnService.SpawnEnemyShip();
            _Gui.Show<DeathMatchOverlay>(GuiScreenType.Overlay);
        }

        public void Unload() {
            
        }
    }
}