using PestelLib.Roulette;
using System;
using System.Collections;
using System.Collections.Generic;
using PestelLib.Chests;
using PestelLib.SharedLogic.Modules;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace Submarines
{
    public class ChestRewardItemsScroll : MonoBehaviour
    {
        [SerializeField] protected CarouselHorizontal _rewardItemsContainer;
        [SerializeField] protected CarouselScrollRect _scrollRect;
        [SerializeField] protected int itemsToScroll = 100;
        [SerializeField] protected float _koeffVelocity = 0.6f;

        [SerializeField] protected GameObject _rewardItem;
        [SerializeField] protected string _style = "roulette";

        public Action<ChestsRewardDef> ScrollFinished;

        protected float _scrollLength;
        protected float _startScrollLength;
        protected int _finalItemNum;
        protected bool _isInitialized = false;
        protected Transform _actualRewardItem;
        protected ChestsRewardDef _finalRewardDef;
        protected Vector3 _startPos;

        public void OnScrollFinished()
        {
            _isInitialized = false;

            ClampToFinalReward();

            if (ScrollFinished != null)
                ScrollFinished(_finalRewardDef);
        }

        public void GenerateRewardItems(List<ChestsRewardDef> possibleRewards, int actualRewardIdx)
        {
            _finalRewardDef = possibleRewards[actualRewardIdx];

            if (_rewardItem == null)
            {
                var visualizer = ContainerHolder.Container.Resolve<ChestsRewardVisualizer>();
                if (visualizer != null)
                    _rewardItem = visualizer.GetRewardView(possibleRewards[0], _style).gameObject;
            }

            
            StartCoroutine(GenerateItemsWithDelay(possibleRewards, actualRewardIdx));
        }


        private IEnumerator GenerateItemsWithDelay(List<ChestsRewardDef> possibleRewards, int actualRewardIdx)
        {
            if (_startPos == Vector3.zero)
            {
                _startPos = _scrollRect.content.localPosition;
            }

            _scrollRect.content.localPosition = _startPos;
            
            yield return new WaitForEndOfFrame();

            var cellSize = (_rewardItemsContainer.Grid.transform as RectTransform).rect.size.y;
            _rewardItemsContainer.Grid.cellSize = new Vector2(cellSize, cellSize);

            if (possibleRewards.Count < 10)
            {
                possibleRewards.AddRange(possibleRewards);
            }

            _rewardItemsContainer.Init(possibleRewards.Count, _rewardItem, (rewardItem, n) =>
            {
                var chestRewardItem = rewardItem.GetComponent<ChestsRewardDrawer>();
                var itemIndex = n % possibleRewards.Count;
                //chestRewardItem.Initialize(possibleRewards[itemIndex], false);
                chestRewardItem.Draw(possibleRewards[itemIndex]);

                _scrollLength = (_finalItemNum - n) * _rewardItemsContainer.GridItemWidth + _rewardItemsContainer.GridItemWidth / 2;
                _startScrollLength = _scrollLength;
                if (itemIndex == actualRewardIdx)               
                    _actualRewardItem = rewardItem.transform;
                
            });

            var rotationsNum = Mathf.RoundToInt((float)itemsToScroll / possibleRewards.Count);
            var startElementIdx = GetCentralElement();
            var totalItemsToScroll = possibleRewards.Count * rotationsNum + actualRewardIdx - startElementIdx;
            _scrollLength = (float)totalItemsToScroll * _rewardItemsContainer.GridItemWidth + _rewardItemsContainer.GridItemWidth/2;
            _startScrollLength = _scrollLength;
            _finalItemNum = (int)totalItemsToScroll + _rewardItemsContainer.ElementsCount - 1;

            _isInitialized = true;
        }

        private int GetCentralElement()
        {
            return (int)Math.Ceiling((double)(_rewardItemsContainer.ElementsCount - 1) / 2);
        }


        private void Start()
        {
            _rewardItem.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            var delta = _scrollLength * Time.deltaTime * _koeffVelocity;
            _scrollLength -= delta;

            var position = _scrollRect.content.localPosition;
            position.x -= delta;
            _scrollRect.content.localPosition = position;

            if (_scrollLength <= 10)
                OnScrollFinished();
        }

        protected virtual void ClampToFinalReward()
        {
            if (_actualRewardItem == null)
                return;

            Vector3[] corners = new Vector3[4];
            var itemTransform = _actualRewardItem.transform as RectTransform;
            itemTransform.GetWorldCorners(corners);

            var leftSide = corners[0];
            var itemWidth = _rewardItemsContainer.Grid.cellSize.x;
            var targetPosX = -itemWidth / 2;
            var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, leftSide);
            Vector2 itemPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_scrollRect.transform as RectTransform, screenPoint, Camera.main, out itemPos);

            var delta = (itemPos.x - targetPosX);
            var position = _scrollRect.content.localPosition;
            position.x -= delta;
            _scrollRect.content.localPosition = position;
        }
    }
}