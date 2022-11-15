using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.Leagues
{
    // не используйте в проде
    class DebugToolbar : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        private UiLeagueScreen _leagueScreen;
        [SerializeField]
        private Button _btnUpdate;
        [SerializeField]
        private Button _btnScore;
        [SerializeField]
        private Button _btnRelogin;
        [Dependency] private LeaguesExampleGameInterface _api;
        System.Random _rnd = new System.Random();
#pragma warning restore 649

        private void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        void OnEnable()
        {
            _btnUpdate.onClick.AddListener(OnUpdate);
            _btnScore.onClick.AddListener(OnScore);
            _btnRelogin.onClick.AddListener(OnRelogin);
        }

        void OnUpdate()
        {
            _leagueScreen.UpdateData();
        }

        void OnScore()
        {
            var score = _rnd.Next(1, 101);
            _api.Score(score);
            OnUpdate();
        }

        void OnRelogin()
        {
            _leagueScreen.Relogin();
        }
    }
}
