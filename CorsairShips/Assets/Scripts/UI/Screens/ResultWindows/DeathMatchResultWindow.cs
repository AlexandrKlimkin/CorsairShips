using System;
using System.Collections;
using System.Collections.Generic;
using Game.Initialization.Parameters;
using Game.SeaGameplay.Data;
using Game.SeaGameplay.GameModes;
using Game.SeaGameplay.Points;
using Game.SeaGameplay.Statistics;
using PestelLib.SharedLogic.Modules;
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
        [Dependency]
        private readonly RewardsModule _RewardsModule;
        [Dependency]
        private readonly PointsService _PointsService;

        [SerializeField]
        private TextMeshProUGUI _ResultText;
        [SerializeField]
        private TextMeshProUGUI _KillsCountText;
        [SerializeField]
        private TextMeshProUGUI _PointsCountText;
        [SerializeField]
        private Button _RestartButton;
        [SerializeField]
        private Button _GoToMenuButton;
        [SerializeField]
        private Button _IncreaseRewardsButton;
        [SerializeField]
        private RewardsPanel _RewardsPanel;
        
        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        private void Start() {
            _RestartButton.onClick.AddListener(OnRestartButtonClick);
            _GoToMenuButton.onClick.AddListener(GoToMenu);
        }

        public void Setup(Match_Result result) {
            _ResultText.text = result.ToString();
            _KillsCountText.text = _Statistics.Kills.ToString();
            var rewards = _RewardsModule.CalculateRewardsData(result, _PointsService.GetLocalPlayerPointsCount());
            _RewardsPanel.Setup(rewards);
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
