using System.Collections;
using System.Linq;
using Game.SeaGameplay.Points;
using Game.SeaGameplay.Spawn;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using PestelLib.UI;
using PestelLib.Utils;
using UI.Battle;
using UI.Screens;
using UI.Screens.ShipSelection;
using UnityDI;
using UnityEngine;
using UTPLib.Services;
using UTPLib.SignalBus;

namespace Game.SeaGameplay.GameModes {
    public partial class DeathMatchService : ILoadableService, IUnloadableService {

        public enum MatchState {
            Loading,
            InProgress,
            Finished,
        }

        [Dependency]
        private readonly UnityEventsProvider _EventsProvider;
        [Dependency]
        private readonly ShipsSpawnService _ShipsSpawnService;
        [Dependency]
        private readonly Gui _Gui;
        [Dependency]
        private readonly SignalBus _SignalBus;
        [Dependency]
        private readonly PointsService _PointsService;
        [Dependency]
        private readonly ILocalShipProvider _LocalShipProvider;
        
        public MatchState CurrentState { get; private set; }

        private DeathMatchParameters Parameters => DeathMatchConfig.Instance.DeathMatchParameters;
        
        public void Load() {
            _SignalBus.Subscribe<ShipDieSignal>(OnShipDieSignal, this);
            _SignalBus.Subscribe<ShipSelectedSignal>(OnShipSelectedSignal, this);
            _Gui.Show<ShipSelectionScreen>();
        }

        public void Unload() {
            _SignalBus.UnSubscribeFromAll(this);
        }

        private void StartMatch() {
            _Gui.Show<ControlsOverlay>(GuiScreenType.Overlay);
            _ShipsSpawnService.SpawnLocalPlayerShip();
            for(var i = 1; i < Parameters.PlayersCount; i++)
                _ShipsSpawnService.SpawnEnemyShip();
            CurrentState = MatchState.InProgress;
            _Gui.Show<DeathMatchOverlay>(GuiScreenType.Overlay);
            _SignalBus.FireSignal(new MatchStartSignal());
        }
        
        private void FinishMatch(Match_Result result) {
            if(CurrentState != MatchState.InProgress)
                return;
            CurrentState = MatchState.Finished;
            _Gui.Close<ControlsOverlay>();
            _Gui.Close<DeathMatchOverlay>();
            _EventsProvider.StartCoroutine(ShowResultRoutine(result));
            SharedLogicCommand.RewardsModule.ClaimRewards(result, _PointsService.GetLocalPlayerPointsCount());
            _SignalBus.FireSignal(new MatchFinishSignal());
        }

        private IEnumerator ShowResultRoutine(Match_Result result) {
            yield return new WaitForSeconds(Parameters.EndScreenShowDelay);
            var endWindow = _Gui.Show<DeathMatchResultWindow>(GuiScreenType.Dialog);
            endWindow.Setup(result);
        }
        
        private void OnShipDieSignal(ShipDieSignal signal) {
            if (signal.Ship.IsLocalPlayerShip) {
                FinishMatch(Match_Result.Defeat);
            }
            else {
                var enemiesAlive = Ship.Ships.Any(_ => !_.Dead && !_.IsLocalPlayerShip);
                if(enemiesAlive)
                    return;
                FinishMatch(Match_Result.Victory);
            }
        }

        private void OnShipSelectedSignal(ShipSelectedSignal signal) {
            StartMatch();
        }
    }
}