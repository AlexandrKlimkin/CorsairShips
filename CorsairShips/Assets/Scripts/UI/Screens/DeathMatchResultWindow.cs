using System;
using System.Collections;
using System.Collections.Generic;
using Game.Initialization.Parameters;
using Game.SeaGameplay.Data;
using Game.SeaGameplay.GameModes;
using Game.SeaGameplay.Statistics;
using TMPro;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;
using UTPLib.Services.SceneManagement;

namespace UI.Battle {
    public class DeathMatchResultWindow : MonoBehaviour {
        [Dependency]
        private readonly BattleStatisticsService _Statistics;
        [Dependency]
        private readonly SceneManagerService _SceneManager;

        [SerializeField]
        private TextMeshProUGUI _ResultText;
        [SerializeField]
        private TextMeshProUGUI _KillsCountText;
        [SerializeField]
        private Button _RestartButton;
        [SerializeField]
        private Button _GoToMenuButton;
        
        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        private void Start() {
            _RestartButton.onClick.AddListener(OnRestartButtonClick);
            _GoToMenuButton.onClick.AddListener(GoToMenu);
        }

        private void OnDestroy() {
            
        }

        public void Setup(DeathMatchService.MatchResult result) {
            _ResultText.text = result.ToString();
            _KillsCountText.text = _Statistics.Kills.ToString();
        }

        private void OnRestartButtonClick() {
            _SceneManager.LoadScene(_SceneManager.ActiveGameModeScene, true);
        }

        private void GoToMenu() {
            _SceneManager.LoadScene(new GameModeScene() {
                SceneType = SceneType.Menu,
            });
        }
    }
}
