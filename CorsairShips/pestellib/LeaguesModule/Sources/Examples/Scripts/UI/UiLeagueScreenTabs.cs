using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PestelLib.SharedLogicBase;
using UnityEngine;
using UnityEngine.UI;
using UnityDI;

namespace PestelLib.Leagues
{
    public class UiLeagueScreenTabs : UiLeagueScreen
    {
#pragma warning disable 649
        [SerializeField]
        private Toggle _divisionTopTab;
        [SerializeField]
        private Toggle _leagueTopTab;
        [SerializeField]
        private Toggle _globalTopTab;
        private ISharedLogic _sharedLogic;
        private bool _loggedIn;
#pragma warning restore 649

        void OnEnable()
        {
            _divisionTopTab.onValueChanged.AddListener(ToggleDivisionTop);
            _leagueTopTab.onValueChanged.AddListener(ToggleLeagueTop);
            _globalTopTab.onValueChanged.AddListener(ToggleGlobalTop);
        }

        private void ToggleDivisionTop(bool state)
        {
            _divisionTop.gameObject.SetActive(state);
        }

        private void ToggleLeagueTop(bool state)
        {
            _leagueTop.gameObject.SetActive(state);
        }

        private void ToggleGlobalTop(bool state)
        {
            _globalTop.gameObject.SetActive(state);
        }

        protected override void Update()
        {
            if(_loggedIn)
                base.Update();

            if (_sharedLogic == null)
            {
                _sharedLogic = ContainerHolder.Container.Resolve<ISharedLogic>();
            }

            if (_sharedLogic == null)
                return;

            SetData(_sharedLogic.PlayerId, "Player " + _sharedLogic.PlayerId.ToString().Substring(0, 4), "");
            _loggedIn = true;
        }
    }
}
