using System;
using System.Collections;
using System.Collections.Generic;
using Game.Meta.BattleLoading;
using Game.SeaGameplay.Data;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;
using UTPLib.Services.SceneManagement;

namespace UI.Screens.MenuMain {
    public class GoToBattlePanel : MonoBehaviour {
        [Dependency]
        private readonly GameStateLoadingService _GameStateLoadingService;

        [SerializeField]
        private Button _BattleButton;
        [SerializeField]
        private Button _ModeButton;
        
        private void Start() {
            ContainerHolder.Container.BuildUp(this);
            _BattleButton.onClick.AddListener(OnBattleClick);
            _ModeButton.onClick.AddListener(OnModeClick);
        }

        private void OnBattleClick() {
            _GameStateLoadingService.LoadSelectedGameMode();
        }
        
        private void OnModeClick() {
            if(_GameStateLoadingService.SelectedGameMode == GameMode.DeathMatch)
                _GameStateLoadingService.SelectedGameMode = GameMode.TeamDeathMatch;
            else if(_GameStateLoadingService.SelectedGameMode == GameMode.TeamDeathMatch)
                _GameStateLoadingService.SelectedGameMode = GameMode.DeathMatch;
        }
    }
}
