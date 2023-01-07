using System;
using System.Collections.Generic;
using Game.SeaGameplay.GameModes;
using Game.SeaGameplay.Points;
using PestelLib.SharedLogic.Modules;
using UI.Screens.MenuMain;
using UnityDI;
using UnityEngine;
using UTPLib.Core.Utils;

namespace UI.Battle {
    public class RewardsPanel : MonoBehaviour {
        [Dependency]
        private readonly IPointsCounter _PointsCounter;
        [Dependency]
        private readonly CurrencyModule _CurrencyModule;
        
        [SerializeField]
        private CurrencyWidget _CurrencyWidgetPrefab;
        [SerializeField]
        private RectTransform _RewardsHost;
        [SerializeField]
        private GameObject _NoRewardsObj;
        
        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        public void Setup(List<RewardData> rewards) {
            _RewardsHost.ClearChildren();
            foreach (var reward in rewards) {
                if (!_CurrencyModule.TryGetCurrencyTypeByItemId(reward.ItemId, out var currencyType)) 
                    continue;
                var currencyWidget = Instantiate(_CurrencyWidgetPrefab, _RewardsHost);
                currencyWidget.Setup(currencyType.Value, reward.Count);
            }
            _NoRewardsObj.SetActive(rewards.Count == 0);
        }
    }
}