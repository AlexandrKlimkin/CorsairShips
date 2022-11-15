using System;
using PestelLib.Roulette;
using PestelLib.UI;
using PestelLib.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PestelLib.Chests;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using S;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace Submarines
{
    public enum ChestShopState { ChestsTab, ScrollTab, Rolling }

    public class ChestShopScreen : MonoBehaviour
    {
        [Header("UltraBox")]
        [SerializeField] protected RectTransform _currentProgress;
        [SerializeField] protected RectTransform _progressBarBG;
        [SerializeField] protected Text _seasonRemainTime;
        [SerializeField] protected Button _megaChestButton;
        [SerializeField] protected Image _highlightImage;

        [Header("Keys")]
        [SerializeField] protected Text _keysText;

        [Header("UI Blocks")]
        [SerializeField] protected Transform _header;
        [SerializeField] protected Transform _description;
        [SerializeField] protected Transform _chestContainer;
        [SerializeField] protected Transform _buttonsBlock;
        [SerializeField] protected Transform _rerollButton;
        [SerializeField] protected Transform _nextButton;
        [SerializeField] protected GameObject _pirateBoxItem;
        [SerializeField] protected Text _rerollSingleCost;
        [SerializeField] protected Text _rerollMultiCost;
        [SerializeField] protected Button _backButton;

        [Header("Roulette")]
        [SerializeField] protected UiRouletteMechanic _roulette;
        [SerializeField] protected Transform _rouletteContainer;

        [Dependency] protected Gui _gui;
        [Dependency] private RouletteModule _exampleRouletteModule;
        [Dependency] protected RouletteEventsModule _rouletteEventsModule;
        [Dependency] protected RouletteDefinitionsContainer _exampleDefinitions;
        [Dependency] protected SharedTime _sharedTime;
        [Dependency] protected SpritesDatabase _spritesDatabase;
        [Dependency] protected ChestsRewardVisualizer _chestsRewardVisualizer;

        protected RouletteModule _rouletteModule;

        protected const int PACK_OF_BOXES_AMOUNT = 10;

        protected float _progress;
        protected PirateBoxChestDef _lastBox;
        protected bool _continue = false;
        protected bool _isRewardClaimed = false;
        protected ChestShopState _currentState;

        protected virtual void Awake()
        {
            ContainerHolder.Container.BuildUp(this);

            _rouletteModule = _exampleRouletteModule;
        }

        protected void Start()
        {
            _roulette = Instantiate(_roulette, _rouletteContainer);
            _roulette.OnClose = BackToChestsTab;
            _roulette.OnReroll = Reroll;
            _roulette.OnMultiReroll = OpenPackOfBoxes;

            SetState(ChestShopState.ChestsTab);

            GenerateChests();

            _progress = _rouletteModule.MegaChestProgress / 100f;
            _keysText.text = _rouletteModule.Keys.ToString();
            StartCoroutine(DelayedUpdateProgressBar());

            StartCoroutine(UpdateMessageRoutine());

            _rouletteModule.OnKeysChanged.Subscribe(OnKeysChanged);
            _rouletteModule.OnProgressChanged.Subscribe(UpdateProgressBar);
        }

        protected void OnKeysChanged()
        {
            _keysText.text = _rouletteModule.Keys.ToString();
        }

        public void Reroll()
        {
            OpenBox(_lastBox);
        }

        public void OpenMegaChest()
        {
            OpenBox(GetUltraBox());
        }

        //TODO override
        protected virtual PirateBoxChestDef GetUltraBox()
        {
            return _exampleDefinitions.SharedLogicDefs.PirateBoxChestDefs.FirstOrDefault(x => x.Id.Contains("ultra"));
        }

        protected virtual List<PirateBoxChestDef> GetBoxes()
        {
            return _exampleDefinitions.SharedLogicDefs.PirateBoxChestDefs;
        }

        //TODO override
        public virtual void OnFreeBoxClicked(PirateBoxChestDef box)
        {
            Debug.Log("Implement ADS!!!");
            /*
            AdsMediationBehaviour.Instance.TryShowAds(AdType.REWARDED, result =>
            {
                if (result == UnityEngine.Advertisements.ShowResult.Finished)
                {
                    ChestTimers.Instance.SetLastAdsTime();
                    OpenBox(box, true);
                }
            });
            */

            StartAdsCooldown();
            OpenBox(box, true);
        }

        public void BackToChestsTab()
        {
            SetState(ChestShopState.ChestsTab);
        }


        protected void OnBoxBuyClicked(PirateBoxChestDef box)
        {
            OpenBox(box);
        }

        protected virtual void OpenBox(PirateBoxChestDef box, bool free = false)
        {
            PirateBoxResult boxContent = OpenRouletteBox(box.Id, free);
            if (boxContent == null)
                return;

            _lastBox = box;
            SetState(ChestShopState.Rolling);

            StartCoroutine(GenerateScroll(boxContent));

            /*
            var rewardIds = new List<string>();
            foreach (var idx in boxContent.RewardIndices)
                rewardIds.Add(boxContent.PossibleRewards[idx].ItemId);
            */
        }

        public void OpenPackOfBoxes()
        {
            if (!HaveMoneyForCost(_lastBox.CostKeys * PACK_OF_BOXES_AMOUNT))
            {
                BankOpenKeyPacks("BoxShop");
                return;
            }

            var rewards = new List<ChestsRewardDef>();
            int fullBonus = 0;
            for (int i = 0; i < PACK_OF_BOXES_AMOUNT; i++)
            {
                PirateBoxResult boxContent = OpenRouletteBox(_lastBox.Id, false);
                if (boxContent == null)
                    continue;

                fullBonus += boxContent.BonusSoft;
                foreach (var idx in boxContent.RewardIndices)
                {
                    var reward = boxContent.PossibleRewards[idx];
                    //rewards.Add(_concreteGameInterface.GetRewardVisualData(reward));
                    rewards.Add(reward);
                }
            }

            OpenPackResultScreen(rewards, fullBonus);

            SetState(ChestShopState.ChestsTab);
        }
        //TODO refact it, shift it to UiRouletteMechanic
        protected IEnumerator GenerateScroll(PirateBoxResult boxContent)
        {
            bool multiroll = boxContent.RewardIndices.Count > 1;
            _continue = false;

            for (int i = 0; i < boxContent.RewardIndices.Count; i++)
            {
                //TODO implement if needed
                //_itemsScroll.RewardClaimed += OnRewardClaimed;
                _roulette.ScrollFinished += OnScrollFinished;
                _isRewardClaimed = false;

                int idx = boxContent.RewardIndices[i];
                var rewardsWithBaits = new List<ChestsRewardDef>();
                rewardsWithBaits.AddRange(boxContent.PossibleRewards);
                rewardsWithBaits.InsertRange(idx, boxContent.BaitRewards);
                idx += boxContent.BaitRewards.Count;
                _roulette.GenerateRewardItems(rewardsWithBaits, idx);

                while (!_isRewardClaimed)
                    yield return null;

                _nextButton.gameObject.SetActive(multiroll);
                _roulette.SetCloseButtonState(!multiroll);

                while (multiroll && !_continue)
                    yield return null;

                _nextButton.gameObject.SetActive(false);
            }


            if (multiroll)
                SetState(ChestShopState.ChestsTab);
            else
                SetState(ChestShopState.ScrollTab);
        }

        protected virtual void GenerateChests()
        {
            var boxes = GetBoxes();

            var chooseBoxes = boxes.Where(b => !b.Id.Contains("ultra"));

            foreach (var def in chooseBoxes)
            {
                GameObject boxItem = Instantiate(_pirateBoxItem, _chestContainer) as GameObject;
                var boxShopItem = boxItem.GetComponent<PirateBoxShopItem>();
                //TODO icons
                boxShopItem.Initialize(def);
                boxShopItem.BoxBuyClicked += OnBoxBuyClicked;
                boxShopItem.BoxForFreeClicked += OnFreeBoxClicked;
                boxItem.SetActive(true);
            }
            _pirateBoxItem.gameObject.SetActive(false);

            StartCoroutine(SetBottomSize());
        }

        protected IEnumerator SetBottomSize()
        {
            yield return new WaitForEndOfFrame();

            var width = (_chestContainer as RectTransform).rect.width;
            var height = (_buttonsBlock as RectTransform).rect.height;
            var size = new Vector2(width, height);
            (_buttonsBlock as RectTransform).sizeDelta = size;
        }

        public void OnContinue()
        {
            _continue = true;
        }

        protected virtual void OnScrollFinished(ChestsRewardDef rewardDef)
        {
            _roulette.ScrollFinished -= OnScrollFinished;

            _roulette.SetFinalReward(rewardDef);        
            _isRewardClaimed = true;
            _roulette.SetCloseButtonState(true);

            //StartCoroutine(PlayEffect(rewardDef));
        }
        /*
        protected IEnumerator PlayEffect(ChestsRewardDef rewardDef)
        {
            //for animations
            yield return new WaitForSeconds(0.5f);

            var rewardVisual = _concreteGameInterface.GetRewardVisualData(rewardDef);
            ShowBuyEffect(rewardVisual.Name, rewardVisual.Description, rewardVisual.Icon, rewardVisual.IconColor);
            yield return new WaitForSeconds(0.5f);

            _isRewardClaimed = true;
            _finalRewardCloseButton.enabled = true;
        }
        */
        protected virtual void SetState(ChestShopState state)
        {
            _currentState = state;
            _roulette.SetState(state);
            switch (state)
            {
                //_nextButton
                case ChestShopState.ChestsTab:
                    TogglePlayerPanel(true);
                    _header.gameObject.SetActive(true);
                    _chestContainer.gameObject.SetActive(true);
                    _description.gameObject.SetActive(true);
                    _buttonsBlock.gameObject.SetActive(false);
                    //_rerollButton.gameObject.SetActive(false);
                    if (_backButton != null)
                        _backButton.gameObject.SetActive(true);
                    break;
                case ChestShopState.ScrollTab:
                    TogglePlayerPanel(true);
                    _header.gameObject.SetActive(true);
                    _chestContainer.gameObject.SetActive(false);
                    _description.gameObject.SetActive(true);
                    _buttonsBlock.gameObject.SetActive(true);
                    if(_backButton != null)
                        _backButton.gameObject.SetActive(true);
                    //_rerollButton.gameObject.SetActive(true);
                    _rerollSingleCost.text = _lastBox.CostKeys.ToString();
                    _rerollSingleCost.color = Color.magenta;// currencyReference.Color; //TODO: load color?
                    _rerollMultiCost.text = (_lastBox.CostKeys * PACK_OF_BOXES_AMOUNT).ToString();
                    _rerollMultiCost.color = Color.magenta;//currencyReference.Color; //TODO: load color?
                    break;
                case ChestShopState.Rolling:
                    TogglePlayerPanel(false);
                    _header.gameObject.SetActive(false);
                    _chestContainer.gameObject.SetActive(false);
                    _description.gameObject.SetActive(false);
                    _buttonsBlock.gameObject.SetActive(false);
                    if (_backButton != null)
                        _backButton.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }

        protected IEnumerator DelayedUpdateProgressBar()
        {
            yield return new WaitForEndOfFrame();
            UpdateProgressBar();
        }

        protected void UpdateProgressBar()
        {
            _progress = _rouletteModule.MegaChestProgress / 100;
            var progressToShow = Mathf.Clamp(_progress, 0, 1);
            var currentProgressSize = _progressBarBG.rect.size;
            currentProgressSize.x *= progressToShow;
            _currentProgress.sizeDelta = currentProgressSize;

            SetMegaChest(progressToShow == 1);
        }

        protected void SetMegaChest(bool enabled)
        {
            _megaChestButton.gameObject.SetActive(enabled);
        }

        protected IEnumerator UpdateMessageRoutine()
        {
            while (true)
            {
                UpdateMessage();
                yield return new WaitForSeconds(1f);
            }
        }

        protected void UpdateMessage()
        {
            TimeSpan remainTime = _rouletteEventsModule.GetSeasonRemainTime(_sharedTime.Now);
            _seasonRemainTime.text = FormatTime.Format(remainTime);

            if (remainTime.Ticks < 0)
            {
                _progress = 0;
                UpdateProgressBar();

                StartUltraBoxSeason();
            }
        }
        //TODO override
        protected virtual void StartUltraBoxSeason()
        {
            var cmd = new RouletteEventsModule_StartSeason();
            CommandProcessor.Process<int, RouletteEventsModule_StartSeason>(cmd);
        }
        //TODO override
        protected virtual void StartAdsCooldown()
        {
            var cmd = new RouletteEventsModule_SetAdsTimeStamp();
            CommandProcessor.Process<int, RouletteEventsModule_SetAdsTimeStamp>(cmd);
        }

        //TODO override
        protected virtual void OpenPackResultScreen(List<ChestsRewardDef> rewards, int bonus)
        {
            var resultScreen = _gui.Show<BoxesPackResultScreen>(GuiScreenType.Dialog);
            resultScreen.Initialize(rewards);
        }

        //TODO override
        public virtual PirateBoxResult OpenRouletteBox(string boxId, bool free)
        {
            var cmd = new RouletteModule_OpenBox() { boxId = boxId, free = free};
            return CommandProcessor.Process<PirateBoxResult, RouletteModule_OpenBox>(cmd);
        }

        //TODO override
        public virtual void OpenKeysShop()
        {
            Debug.Log("Here should be info about buying keys");
            //TODO change it to use SharedLogicCommand
            var cmd = new RouletteModule_AddKeys { amount = 500 };
            CommandProcessor.Process<int, RouletteModule_AddKeys>(cmd);
        }

        //TODO: implement if needed
        public virtual void TogglePlayerPanel(bool enable)
        {
            //Switch on/off player currencies panel
        }

        public bool HaveMoneyForCost(int costKeys)
        {
            return _rouletteModule.Keys >= costKeys;
        }

        //TODO: implement if needed
        public virtual void ShowBuyEffect(string name, string description, Sprite icon, Color iconColor)
        {
            
        }

        //TODO override
        public virtual void BankOpenKeyPacks(string type)
        {
            Debug.Log("IMPLEMENT SHOW STORE");
        }

        protected virtual void OnDestroy()
        {
            _rouletteModule.OnKeysChanged.Unsubscribe(OnKeysChanged);
            _rouletteModule.OnProgressChanged.Unsubscribe(UpdateProgressBar);
        }

        public virtual void OnBack()
        {
            _gui.GoBack();
            _gui.Close(gameObject);
        }
    }
}
