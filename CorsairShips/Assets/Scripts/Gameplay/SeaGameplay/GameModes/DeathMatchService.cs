using System.Collections;
using System.Linq;
using Game.SeaGameplay.Spawn;
using PestelLib.UI;
using PestelLib.Utils;
using UI.Battle;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.SignalBus;

namespace Game.SeaGameplay.GameModes {
    public class DeathMatchService : ILoadableService, IUnloadableService {
        [Dependency]
        private readonly UnityEventsProvider _EventsProvider;
        
        public enum MatchState {
            Loading,
            InProgress,
            Finished,
        }

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
        //
        // public const int PlayersCount = 6;
        
        public MatchState CurrentState { get; private set; }

        private DeathMatchParameters Parameters => DeathMatchConfig.Instance.DeathMatchParameters;
        
        public void Load() {
            _SignalBus.Subscribe<ShipDieSignal>(OnShipDieSignal, this);
            _ShipsSpawnService.SpawnLocalPlayerShip();
            for(var i = 1; i < Parameters.PlayersCount; i++)
                _ShipsSpawnService.SpawnEnemyShip();
            CurrentState = MatchState.InProgress;
            
            _Gui.Show<DeathMatchOverlay>(GuiScreenType.Overlay);
        }

        public void Unload() {
            _SignalBus.UnSubscribeFromAll(this);
        }

        private void FinishMatch(MatchResult result) {
            if(CurrentState != MatchState.InProgress)
                return;
            CurrentState = MatchState.Finished;
            _Gui.CloseAllWindows();
            _EventsProvider.StartCoroutine(ShowResultRoutine(result));
        }

        private IEnumerator ShowResultRoutine(MatchResult result) {
            yield return new WaitForSeconds(Parameters.EndScreenShowDelay);
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