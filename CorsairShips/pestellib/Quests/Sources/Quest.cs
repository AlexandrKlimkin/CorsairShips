using PestelLib.Chests;
using PestelLib.Localization;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using PestelLib.UI;
using S;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityDI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PestelLib.Quests
{
    public class Quest : MonoBehaviour, IPointerClickHandler
    {      
        [Dependency] protected ILocalization _localizationData;
        [Dependency] protected QuestModule _questModule;
        [Dependency] protected SharedTime _sharedTime;
        [Dependency] protected ChestsRewardVisualizer _chestsRewardVisualizer;
        [Dependency] protected Dictionary<string, ChestsRewardDef> _chestsRewardDefsDict;

        [Header("Details")]
        [SerializeField] protected Text _name;
        [SerializeField] protected Text _description;
        [SerializeField] protected Text _progress;
        [SerializeField] protected string _rewardStyle = "default";
        [SerializeField] protected RectTransform _rewardContainer;
        [SerializeField] protected Text _rerollLabel;
        [SerializeField] protected Text _rerollTime;

        [Header("Buttons")]
        [SerializeField] protected Button _getRevenueButton;
        [SerializeField] protected Button _getRevenueX2Button;
        [SerializeField] protected Button _rerollButton;

        [Header("Cooldown")]
        [SerializeField] protected GameObject _cooldownLocker;
        [SerializeField] protected Text _cooldownText;
        [SerializeField] protected Text _cooldownAdsButtonText;
        [SerializeField] protected Button _adsButton;
        protected const int _minimalCooldownMinutes = 30;

        [Header("Level lock")]
        [SerializeField] protected GameObject _levelLocker;
        [SerializeField] protected Text _levelLockerText;
        [SerializeField] protected Image _levelLockerIcon;

        [Header("State containers")]
        [SerializeField] protected GameObject _activeState;
        [SerializeField] protected GameObject _questContent;
        [SerializeField] protected Image _selectedMark;
        [SerializeField] protected Image _hasRevenueMark;
        [SerializeField] protected Image _selectedMarkProgress;
        [SerializeField] protected Color _selectedColor = Color.yellow;

        [Header("Background")]     
        [SerializeField] protected Image _backProgressImage;
        [SerializeField] protected Image _backQuestImage;
        [SerializeField] protected Image _backCooldownImage;

        [Header("Achievement level icons")]
        [SerializeField] protected GameObject _achievementsContainer;
        [SerializeField] protected GameObject[] _achievementLevelIcons;
        bool _initialized;

        protected int timeReroll = 0;
        public QuestDef QuestDef { get; protected set; }

        public Action<string> OnGetRevenue = (questId) => { };
        public Action<Quest> OnSelected = (quest) => { };


        public QuestState QuestState { get; set; }
        public bool QuestActive { get; protected set; }

        public string GetQuestName(QuestDef def)
        {
            if (_localizationData == null)
                _localizationData = ContainerHolder.Container.Resolve<ILocalization>();

            var localizedName = _localizationData.Get(def.Name);
            if (localizedName.Contains("{0}"))
            {
                return string.Format(localizedName, def.Amount);
            }
            else
            {
                return localizedName;
            }
        }

        private void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        protected virtual void OnEnable()
        {
            if (_rerollButton != null)
                SetRerollButton();

            StartCoroutine(UpdatePanelCourutine());
        }

        public virtual void Init(QuestDef def)
        {
            ContainerHolder.Container.BuildUp(this);
            QuestDef = def;           
            var level = GetPlayerLevel();

            if (_achievementsContainer != null)
                _achievementsContainer.SetActive(true);

            if (_levelLocker != null)
            {
                _levelLocker.SetActive(level < def.MinLevel);
            }

            _questContent.SetActive(level >= def.MinLevel);

            if (_levelLockerText != null)
            {
                _levelLockerText.text = def.MinLevel.ToString(CultureInfo.InvariantCulture);
            }

            if (_levelLockerIcon != null)
            {
                _levelLockerIcon.sprite = GetPlayerLevelIcon(def.MinLevel);
            }

            _name.text = GetQuestName(QuestDef);

            var localizedDesc = _localizationData.Get(QuestDef.Description);
            if (localizedDesc.Contains("{0}"))
            {
                //_description.text = string.Format(localizedDesc, QuestDef.Amount);
                _description.text = (string.Format(localizedDesc, QuestDef.Amount));
            }
            else
            {
                //_description.text = localizedDesc;
                _description.text = (localizedDesc);
            }

            if (_progress != null)
                _progress.text = "";

            SetupRevenue(QuestDef);

            _rewardContainer.gameObject.SetActive(true);

            UpdateProgress();

            var isInCooldown = /*RevenueReady && !RevenueUsed && */_questModule.IsQuestInCooldown(_sharedTime.Now, def.Id);
            QuestActive = !isInCooldown;
            _activeState.SetActive(QuestActive);

            if (_cooldownLocker != null)
            {
                _cooldownLocker.SetActive(isInCooldown);
            }

            if (_cooldownAdsButtonText != null)
            {
                //TODO U can use it like this: string.Format(_localizationData.Get("speed up_update"), 30); As well as with many {0}, {1}...
                _cooldownAdsButtonText.text = _localizationData.Get("speed up_update").Replace("{0}", "30");
            }

            _questModule.OnTimeUpdated.Subscribe(UpdateProgress);
            _questModule.OnQuestCompletedIncrement.Subscribe(OnQuestCompletedIncrement);

            if (_hasRevenueMark != null)
            {
                _hasRevenueMark.gameObject.SetActive(RevenueReady && !RevenueUsed);
            }

            UpdateAchievementLevelIcon();

            SubscribeQuestGetRevenueButton();
            Selected = false;

            if (_rerollButton != null)
            {
                _rerollButton.onClick.RemoveAllListeners();
                _rerollButton.onClick.AddListener(Reroll);
                var isRerollAllowed = (QuestDef.QuestClass == QuestClass.Regular) &&
                                      (QuestState.Completed < QuestDef.Amount);
                _rerollButton.gameObject.SetActive(isRerollAllowed);
            }
            
            var rewardView = _chestsRewardVisualizer.GetRewardView(QuestState.Rewards, _rewardStyle);

            for (int i = _rewardContainer.childCount - 1; i >= 0; i--)
            {
                 Destroy(_rewardContainer.GetChild(i).gameObject);
            }

            foreach (var rectTransform in rewardView)
            {
                rectTransform.SetParent(_rewardContainer, false);
            }

            _initialized = true;
        }


        private void UpdateAchievementLevelIcon()
        {
            for (var i = 0; i < _achievementLevelIcons.Length; i++)
            {
                if (QuestState.CompleteRegistred)
                {
                    _achievementLevelIcons[i].SetActive(QuestDef.AchievementLevel >= i);
                }
                else
                {
                    _achievementLevelIcons[i].SetActive(QuestDef.AchievementLevel > i);
                }
            }
        }


        protected void Reroll()
        {
            var cmd = new S.QuestModule_RerollQuest { questId = QuestDef.Id };
            CommandProcessor.Process<object, QuestModule_RerollQuest>(cmd);          
        }

        protected virtual void SubscribeQuestGetRevenueButton()
        {
            if (_getRevenueButton != null)
            {
                _getRevenueButton.onClick.RemoveAllListeners();
                _getRevenueButton.onClick.AddListener(GiveRevenue);
                _getRevenueButton.gameObject.SetActive(RevenueReady && !RevenueUsed);
            }

            if (_getRevenueX2Button != null)
            {
                _getRevenueX2Button.onClick.RemoveAllListeners();
                _getRevenueX2Button.onClick.AddListener(GiveRevenueX2);
                _getRevenueX2Button.gameObject.SetActive(RevenueReady && !RevenueUsed);
            }

            //if ((_progress != null) && (RevenueReady))
            //    _progress.text = "";
        }

        protected virtual void GiveRevenueX2()
        {
            ShowRewardMessage(true);
            var cmd = new QuestModule_GiveQuestRevenue() { questId = QuestDef.Id, doubleRevenue = true };
            CommandProcessor.Process<object, QuestModule_GiveQuestRevenue>(cmd);
           
        }

        protected virtual void GiveRevenue()
        {
            ShowRewardMessage(false);
            var cmd = new QuestModule_GiveQuestRevenue() { questId = QuestDef.Id, doubleRevenue = false };
            CommandProcessor.Process<object, QuestModule_GiveQuestRevenue>(cmd);
           
        }

        protected virtual void ShowRewardMessage(bool doubleReward)
        {
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef
            {
                Caption = "Reward!",
                Description = "Your reward: " + QuestDef.Id + (doubleReward?"X2":""),
                ButtonAAction = () => { },
                ButtonALabel = "OK"
            });
        }

        private void OnDisable()
        {
            if(_questModule != null)
            {
                _questModule.OnTimeUpdated.Unsubscribe(UpdateProgress);
                _questModule.OnQuestCompletedIncrement.Unsubscribe(OnQuestCompletedIncrement);
            }

            if (_rerollButton != null)
                _rerollButton.onClick.RemoveAllListeners();
            
            StopAllCoroutines();
        }

        void OnQuestCompletedIncrement(string id, float counter)
        {
            if (QuestDef.Id == id)
            {
                UpdateProgress();
            }
        }

        IEnumerator UpdatePanelCourutine()
        {
            var wait = new WaitForSeconds(0.5f);
            while (true)
            {
                UpdatePanel();
                yield return wait;
            }
        }

        public void UpdatePanel()
        {
            if (!_initialized)
            {
                return;
            }

            var cooldown = GetCooldown();
            if (_cooldownLocker != null)
            {
                if (QuestDef.QuestClass != "Everyday")
                    _cooldownLocker.SetActive(cooldown.Ticks > 0);
                else
                    _cooldownLocker.SetActive(false);
            }

            if (_adsButton != null)
            {
                _adsButton.gameObject.SetActive(cooldown.Ticks > TimeSpan.FromMinutes(_minimalCooldownMinutes).Ticks);
                _adsButton.interactable = CanStartAnyAd();
            }

           // _activeState.SetActive(cooldown.Ticks <= 0);

            string remainTime = string.Format("{0:D1}:{1:D2}:{2:D2}",
                cooldown.Hours,
                cooldown.Minutes,
                cooldown.Seconds
            );

            if (_cooldownText != null)
            {
                   _cooldownText.text = string.Format(_localizationData.Get("QuestCooldown"), ":    " +
                    "<size=50>" + remainTime + "  " + "</size>");
            }
        }

        protected bool CanStartAnyAd()
        {
            return true;
        }

        public TimeSpan GetCooldown()
        {
            var cooldown =  new TimeSpan(0,0,0, QuestDef.CooldownTime);// _questModule.QuestCooldown;
            return cooldown - new TimeSpan(_sharedTime.Now.Ticks - QuestState.Timestamp);
        }

        public bool RevenueReady
        {
            get { return QuestState.Completed >= QuestDef.Amount; }
        }

        public bool RevenueUsed
        {
            get { return QuestState.RevenueUsed; }
        }
        
        [ContextMenu("DebugIncrease")]
        public void DebugIncrease()
        {
            var cmd = new QuestModule_IncrementQuestById {delta = 2, questId = QuestState.Id};
            CommandProcessor.Process<object, QuestModule_IncrementQuestById>(cmd);
            Debug.Log(QuestState.Id + " Completed " + QuestState.Completed);
        }

        [ContextMenu("WatchAds")]
        public virtual void WatchAds()
        {
            OnAdWatchingFinished(true);
        }

        protected virtual void OnAdWatchingFinished(bool success)
        {
            if (!success) return;

            var cmd = new QuestModule_QuestWatchAds() { id = QuestDef.Id };
            CommandProcessor.Process<object, QuestModule_QuestWatchAds>(cmd);
        }

        protected virtual void UpdateProgress()
        {
            if (_questModule == null || QuestDef == null) return;

            QuestState = _questModule.QuestStates().FirstOrDefault(x => x.Id == QuestDef.Id);
            if (QuestState.CompleteRegistred && QuestState.RevenueUsed)
            {
                if (!QuestDef.AlwaysVisible)
                    Destroy(this);
                else
                {
                    if (_rewardContainer != null)
                        _rewardContainer.gameObject.SetActive(false);

                    if (_progress != null)
                        _progress.text = _localizationData.Get("QuestCompleted");

                    if (_achievementsContainer != null)
                        _achievementsContainer.SetActive(false);
                }

                return;
            }

            if (QuestState == null) return;

            if (QuestState.Completed < QuestDef.Amount)
            {
                if (_progress != null)
                {
                    _progress.text = string.Format(_localizationData.Get("QuestProgress"),
                        Mathf.RoundToInt(QuestState.Completed) + QuestDef.AmountOffset,
                        QuestDef.Amount + QuestDef.AmountOffset);
                }
            }
            else
            {
                if (_rerollButton != null)
                {
                    _rerollButton.gameObject.SetActive(false);
                }

                if (_getRevenueButton != null)
                    _getRevenueButton.gameObject.SetActive(true);

                if (_getRevenueX2Button != null)
                    _getRevenueX2Button.gameObject.SetActive(true);

                /*
                if (_questImage != null && _description != null)
                {
                    _questImage.gameObject.SetActive(false);
                    _description.transform.parent.gameObject.SetActive(false);
                }*/
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSelected(this);
        }

        public bool Selected
        {
            get
            {
                if (_selectedMark == null) return false;

                return _selectedMark.gameObject.activeSelf;
            }
            set
            {
                if (_selectedMark != null)               
                    _selectedMark.gameObject.SetActive(value);
                if (_selectedMarkProgress != null)
                    _selectedMarkProgress.gameObject.SetActive(value);
                if (_progress != null)
                    _progress.color = (value) ? _selectedColor : Color.white ;
            }
        }

        protected virtual int GetPlayerLevel()
        {
            return 1;
        }

        protected virtual Sprite GetPlayerLevelIcon(int level)
        {
            return null;
        }

        protected virtual void SetupRevenue(QuestDef questDef)
        {

        }

        public void SetColorBack(Color color)
        {
            if (_backQuestImage != null)
                _backQuestImage.color = color;

            if (_backProgressImage != null)
                _backProgressImage.color = color;

            if (_backCooldownImage != null)
                _backCooldownImage.color = color;
        }

        protected virtual void SetRerollButton()
        {
            timeReroll = 0;
            StopAllCoroutines();

            if ((_rerollLabel == null) && (_rerollTime == null))
                return;
            if (!_questModule.InQuestReroll())
            {
                _rerollLabel.gameObject.SetActive(true);
                _rerollTime.gameObject.SetActive(false);
            }
            else
            {
                _rerollLabel.gameObject.SetActive(false);
                _rerollTime.gameObject.SetActive(true);
                SetRerollTimeInButton();
            }
        }

        void SetTimeRerollText(int time)
        {
            TimeSpan t = new TimeSpan(0, 0, time);
            if (_rerollTime == null)
                return;
            _rerollTime.text = t.Hours.ToString("D2") + ":" + t.Minutes.ToString("D2") + ":" + t.Seconds.ToString("D2");
        }

        void SetRerollTimeInButton()
        {           
            timeReroll =(int)_questModule.GetTimeToNextReroll(_sharedTime.Now).TotalSeconds;

            if (timeReroll > 0)
                StartCoroutine(UpdateTimeTextCorutine());
        }

        IEnumerator UpdateTimeTextCorutine()
        {
            while (timeReroll > 0)
            {
                SetTimeRerollText(timeReroll);
                timeReroll--;
                yield return new WaitForSeconds(1f);
            }
            SetRerollButton();
        }
    }
}
