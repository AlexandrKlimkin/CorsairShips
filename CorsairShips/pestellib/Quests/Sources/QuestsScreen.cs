using System;
using System.Collections;
using System.Collections.Generic;
using PestelLib.Localization;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic.Defs;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using PestelLib.UI;
using PestelLib.Utils;
using S;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.Quests
{
    public class QuestsScreen : MonoBehaviour
    {
        [Dependency] protected Gui _gui;
        //[Dependency] private PseudoDefinitions _defs;
        [Dependency] protected QuestModule _questModule;
        [Dependency] protected QuestEventsModule _questEventsModule;
        [Dependency] protected SharedTime _sharedTime;
        [Dependency] protected ILocalization _localizationData;
        [Dependency] protected Dictionary<string, QuestDef> _questDefsDictionary;

        [SerializeField] protected GameObject _questPrefab;
        [SerializeField] protected GameObject _emptyQuestPrefab;
        [SerializeField] protected Transform _questContainer;
        [SerializeField] protected Text _counters;
        [SerializeField] protected Button _closeButton;

        [SerializeField] protected string _questClass = QuestClass.Regular;

        [SerializeField] protected Text _seasonRemainTime;
        [SerializeField] protected GameObject _noQuestsAvailable;
        [SerializeField] protected Image _seasonProgressBar;

        [SerializeField] protected Color _evenQuestColor = new Color();
        [SerializeField] protected Color _unevenQuestColor = new Color();

        [SerializeField] protected Scrollbar scrollbarContent;

        protected readonly List<GameObject> _questEmpty = new List<GameObject>();
        protected const int countPoolQuest = 10;
        protected Queue<GameObject> _poolEmpty = new Queue<GameObject>();

        protected readonly List<GameObject> _activeQuest = new List<GameObject>();
        public int sizeQuestPool = 20;
        protected Queue<GameObject> _poolQuest = new Queue<GameObject>();
        protected bool IsSubscrabe = false;
        protected virtual void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
            if ((!IsSubscrabe) && (_questClass == QuestClass.Regular))
            {               
                  _questModule.OnQuestError.Subscribe(OnQuestError);
                  _questModule.OnRerollFailed.Subscribe(OnRerollFailed);
            }

            if (!_questModule.CheckQuestErrors())
                CommandProcessor.Process<object, QuestModule_CleanAbsentQuests>(new QuestModule_CleanAbsentQuests());
           
            CreateQuestPool();
            if (_closeButton != null) 
                _closeButton.onClick.AddListener(() =>
                {
                    _gui.Close(gameObject);
                });
            //  scrollbarContent = GetComponentInChildren<Scrollbar>();
            IsSubscrabe = true;
        }

        private void OnRerollFailed(TimeSpan timeToNextReroll)
        {
            string remainTimeText = string.Format("{0:D1}:{1:D2}", timeToNextReroll.Hours, timeToNextReroll.Minutes);
           
            GenericMessageBoxScreen.Show(new GenericMessageBoxDef
            {                
                Caption = _localizationData.Get("Message"),
                Description = string.Format(_localizationData.Get("QuestRerollFailed"), remainTimeText),
                ButtonAAction = () => { },
                ButtonALabel = _localizationData.Get("OK")
            });
        }

        protected virtual void OnDestroy()
        {
            _questModule.OnQuestError.Unsubscribe(OnQuestError);
            _questModule.OnRerollFailed.Unsubscribe(OnRerollFailed);
        }

        private void OnQuestError(string s)
        {
            Debug.LogError(s);
        }

        protected virtual void OnEnable()
        {
            _questModule.OnListChanged.Subscribe(Rebuild);
            
            CommandProcessor.Process<object, QuestEventsModule_UpdateEvent>(new QuestEventsModule_UpdateEvent());

            Rebuild();

            StartCoroutine(UpdateCourutine());
        }


        protected virtual void OnDisable()
        {
            _questModule.OnListChanged.Unsubscribe(Rebuild);     
            StopAllCoroutines();
        }

        protected virtual void Rebuild()
        {
            ClearEmptyQuest();
            ClearQuest();
            var questStates = _questModule.QuestStates();

            //for (int i = _questContainer.childCount - 1; i >= 0; i--)
            //{               
            //   // Destroy(_questContainer.GetChild(i).gameObject);
            //}

            var actualQuestsCount = 0;

            for (int i = 0; i < questStates.Length; i++)
            {
                var questClass = _questDefsDictionary[questStates[i].Id].QuestClass;
                if (_questClass != questClass) continue;

                var def = _questDefsDictionary[questStates[i].Id];
                if (!def.AlwaysVisible)
                {
                    if (questStates[i].CompleteRegistred && questStates[i].RevenueUsed)
                    {
                        continue;
                    }
                }

                var quest = GetQuestFromPool();
                var script = quest.GetComponent<Quest>();

                _activeQuest.Add(quest);

                if (questStates[i].CompleteRegistred)
                    quest.transform.SetAsFirstSibling();
                else
                    quest.transform.SetAsLastSibling();    
                
                if (!_questDefsDictionary.ContainsKey(questStates[i].Id))
                {
                    Debug.LogError("Not found quest def: " + questStates[i].Id);
                }

               // Color color = (_activeQuest.IndexOf(quest) % 2 != 0) ? _unevenQuestColor : _evenQuestColor;
                script.Init(def);
                actualQuestsCount++;
                
                quest.SetActive(true);
            }

            int emptyCount = ((countPoolQuest - actualQuestsCount) > 0) ? countPoolQuest - actualQuestsCount : 1;
            for (int i = 0; i < emptyCount; i++)
            {
                GameObject emptyItem = GetEmptyQuestFromPool();
                _questEmpty.Add(emptyItem);
                emptyItem.SetActive(true);
                emptyItem.transform.SetAsLastSibling();
            }

            if (_counters != null)
            {
                _counters.enabled = _questModule.HasQuestsToReroll(_sharedTime.Now, _questClass);
            }

            if (_noQuestsAvailable != null)
            {
                _noQuestsAvailable.SetActive(actualQuestsCount == 0);
            }
            SetBackColorForAll();
            SetScrollBar();
            SetRerollText();
        }

        public void SetBackColorForAll()
        {
            int num = 0;
            for (int i = 0; i < _questContainer.childCount; i++)
            {
                if (_questContainer.GetChild(i).gameObject.activeInHierarchy)
                {
                    Color color = (num % 2 != 0) ? _unevenQuestColor : _evenQuestColor;
                    var script = _questContainer.GetChild(i).GetComponent<Quest>();
                    if ((script != null))
                        script.SetColorBack(color);
                    else
                        SetColorEmty(_questContainer.GetChild(i).gameObject,color);
                    num++;
                }
            }
        }

        public bool AnyUnclaimedQuests()
        {
            return _questModule.AnyCompletedQuests;
        }

        //TODO: This int is a temporary optimization. TODO: remove "_counters.text =" from update
        private int _lastValue = -1;

        IEnumerator UpdateCourutine()
        {
            var wait = new WaitForSeconds(0.5f);
            while (true)
            {
                UpdateScreen();
                yield return wait;
            }
        }

        protected virtual void UpdateScreen()
        {
            int newValue = _questModule.IsRerollInCooldown(_sharedTime.Now) ? _questModule.MaxRerollCounter - _questModule.RerollCounter : _questModule.MaxRerollCounter;
            if ((newValue != _lastValue))
            {
                _lastValue = newValue;
                SetRerollText(newValue);
            }

            if (_seasonRemainTime != null)
            {
                _seasonRemainTime.text = string.Format(_localizationData.Get("QuestEventRemain"), 
                    FormatTime.FormatAuto(_questEventsModule.RemainTime(_sharedTime.Now),
                        _localizationData.Get("offers_time_short"), 
                        _localizationData.Get("offers_time_long")
                    )
                );

                if (_questEventsModule.SavedWeekIndex != _questEventsModule.GetWeekIndex(_sharedTime.Now))
                {
                    CommandProcessor.Process<object, QuestEventsModule_UpdateEvent>(new QuestEventsModule_UpdateEvent());
                }
            }

            if (_seasonProgressBar != null)
            {
                _seasonProgressBar.fillAmount = _questEventsModule.NormalizedDuration(_sharedTime.Now);
            }
        }

        protected virtual void SetRerollText(int value = 0)
        {
            if (_counters != null)
            {
                _counters.text = string.Format(_localizationData.Get("QuestRerollCounter"), value);
            }
        }

        private void SetColorEmty(GameObject empty, Color color)
        {
            Image[] backImage = empty.GetComponentsInChildren<Image>();
            foreach(Image back in backImage)
            {
                back.color = color;
            }
        }


        protected void ClearEmptyQuest()
        {
            _questEmpty.ForEach(m =>
            {
                m.SetActive(false);
                _poolEmpty.Enqueue(m);
            });
            _questEmpty.Clear();
        }            

        protected void CreateEmptyQuestPool()
        { 
            for (int i = 0; i < countPoolQuest; i++)
            {
                if (_emptyQuestPrefab != null)
                    CreateEmptyQuest();
            }
        }

        protected GameObject CreateEmptyQuest()
        {
            GameObject empty = Instantiate(_emptyQuestPrefab, _questContainer);
            empty.SetActive(false);
            _poolEmpty.Enqueue(empty);        
            return empty;
        }

        protected GameObject GetEmptyQuestFromPool()
        {
            if (_poolEmpty.Count > 0)
            {
                return _poolEmpty.Dequeue();
            }
            return CreateEmptyQuest();
        }

        protected void ClearQuest()
        {
            _activeQuest.ForEach(m =>
            {
                m.gameObject.SetActive(false);
                _poolQuest.Enqueue(m);
            });
            _activeQuest.Clear();
        }

        protected void CreateQuestPool()
        {
            for (int i = 0; i < sizeQuestPool; i++)
            {
                CreateQuest();
            }
            CreateEmptyQuestPool();
        }

        protected GameObject CreateQuest()
        {
            GameObject quest = Instantiate(_questPrefab, _questContainer);          
            quest.SetActive(false);
            _poolQuest.Enqueue(quest);
            return quest;
        }

        protected GameObject GetQuestFromPool()
        {
            if (_poolQuest.Count > 0)
            {
                return _poolQuest.Dequeue();
            }
            return CreateQuest();
        }

        protected void SetScrollBar()
        {
            if (scrollbarContent != null)
            {
                scrollbarContent.value = 1f;
            }
        }
    }
}



