using System;
using System.Collections.Generic;
using UnityEngine;
using PestelLib.Chests;
using PestelLib.SharedLogic.Modules;
using UnityEngine.UI;

namespace Submarines
{
    public class UiRouletteMechanic : MonoBehaviour
    {
        [SerializeField] protected ChestRewardItemsScroll _itemsScroll;
        [SerializeField] protected GameObject _centralCursor;
        [SerializeField] protected ChestsRewardDrawer _finalReward;
        [SerializeField] protected Button _finalRewardCloseButton;
        [SerializeField] protected Button _instantResultButton;

        public Action OnClose;
        public Action<ChestShopState> OnSetState;

        public Action<ChestsRewardDef> ScrollFinished
        {
            get { return _itemsScroll.ScrollFinished; }
            set { _itemsScroll.ScrollFinished = value; }
        }
        public Action OnReroll;
        public Action OnMultiReroll;
        public Action OnAdsAction;

        private void Start()
        {
            _finalRewardCloseButton.onClick.AddListener(OnClickClose);
            _instantResultButton.onClick.AddListener(ShowInstantResult);
        }

        private void OnClickClose()
        {
            if (OnClose != null)
            {
                OnClose();
            }
        }

        public void GenerateRewardItems(List<ChestsRewardDef> possibleRewards, int actualRewardIdx)
        {
            _itemsScroll.GenerateRewardItems(possibleRewards, actualRewardIdx);
        }

        public virtual void SetState(ChestShopState state)
        {
            switch (state)
            {
                //_nextButton
                case ChestShopState.ChestsTab:
                    _finalReward.gameObject.SetActive(false);
                    _itemsScroll.gameObject.SetActive(false);
                    _centralCursor.SetActive(false);                    
                    break;
                case ChestShopState.ScrollTab:
                    _itemsScroll.gameObject.SetActive(true);
                    _centralCursor.SetActive(true);
                    _instantResultButton.interactable = false;
                    break;
                case ChestShopState.Rolling:
                    _finalReward.gameObject.SetActive(false);
                    _itemsScroll.gameObject.SetActive(true);
                    _centralCursor.SetActive(true);
                    _instantResultButton.interactable = true;
                    break;
                default:
                    break;
            }

            if (OnSetState != null)
            {
                OnSetState(state);
            }
        }

        public virtual void SetFinalReward(ChestsRewardDef rewardDef)
        {
            _finalReward.Draw(rewardDef);
            _finalReward.gameObject.SetActive(true);
        }

        public void SetCloseButtonState(bool state)
        {
            _finalRewardCloseButton.enabled = state;
        }

        private void OnDestroy()
        {
            _finalRewardCloseButton.onClick.RemoveAllListeners();
            _instantResultButton.onClick.RemoveAllListeners();
        }

        private void ShowInstantResult()
        {
            _itemsScroll.OnScrollFinished();
        }

        //for override
        public virtual void SetRerollButtonView(int keyCount, bool isMulti, bool active = false)
        {

        }
        //for override
        public virtual void SetAdsButtonActive(bool active = false)
        {
           
        }
    }
}
