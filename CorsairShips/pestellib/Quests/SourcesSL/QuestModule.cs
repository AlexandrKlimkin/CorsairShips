using System;
using System.Collections.Generic;
using System.Linq;
using PestelLib.Serialization;
using PestelLib.SharedLogicBase;
using S;
using UnityDI;

namespace PestelLib.SharedLogic.Modules
{
    public class QuestModule : SharedLogicModule<QuestModuleState>
    {
        //internal Action<RevenueDef> OnReward = x => { }; //Context.RewardsModule.GiveRevenue(revenue);

        public ScheduledAction OnListChanged;
        public ScheduledAction<string> OnNewQuest;
        public ScheduledAction<string, float> OnQuestCompletedIncrement;
        public ScheduledAction<QuestDef> OnQuestCompleted;
        public ScheduledAction OnTimeUpdated;
        public ScheduledAction<string> OnQuestError;
        public ScheduledAction<TimeSpan> OnRerollFailed;
        public ScheduledAction<int> QuestCooldownStarted;

        [GooglePageRef("Quests")] [Dependency] protected List<QuestDef> _questDefs;
        [Dependency] protected Dictionary<string, QuestDef> _questDefsDictionary;
        [GooglePageRef("ChestRewards")] [Dependency] protected List<ChestsRewardDef> _chestsRewardDefs;
        [Dependency] protected RandomModule _randomModule;

        internal event Action<string> OnQuestCompletedInternal = i => { };

        public QuestModule()
        {
            OnListChanged = new ScheduledAction(ScheduledActionCaller);
            OnNewQuest = new ScheduledAction<string>(ScheduledActionCaller);
            OnQuestCompletedIncrement = new ScheduledAction<string, float>(ScheduledActionCaller);
            OnQuestCompleted = new ScheduledAction<QuestDef>(ScheduledActionCaller);
            OnTimeUpdated = new ScheduledAction(ScheduledActionCaller);
            OnQuestError = new ScheduledAction<string>(ScheduledActionCaller);
            OnRerollFailed = new ScheduledAction<TimeSpan>(ScheduledActionCaller);
            QuestCooldownStarted = new ScheduledAction<int>(ScheduledActionCaller);
        }

        protected List<QuestState> _questStates
        {
            get { return State.Quests; }
        }

        public override void MakeDefaultState()
        {
            base.MakeDefaultState();

            if (_questDefs == null) return;

            foreach (var questDef in _questDefs)
            {
                if (questDef.AddToDefaultState)
                {
                    AddQuest(new QuestState { Id = questDef.Id });
                }
            }
        }

        public void SyncMissingQuests()
        {
            foreach (var questDef in _questDefs)
            {
                if (questDef.AddToDefaultState && questDef.KeepInState && !_questStates.Any(q => q.Id == questDef.Id))
                {
                    AddQuest(new QuestState { Id = questDef.Id });
                }
            }
        }

        public bool CheckQuestErrors()
        {
            for (var i = _questStates.Count - 1; i >= 0; i--)
            {
                if (!_questDefsDictionary.ContainsKey(_questStates[i].Id))
                {
                    OnQuestError.Schedule("Quest " + _questStates[i].Id + " doesn't exist in defs!");
                    _questStates.RemoveAt(i);
                    return false;
                }
            }

            return true;
        }
        
        public QuestState[] QuestStates()
        {
            return CloneMessagePackObject(State.Quests).ToArray();
        }

        protected void IncrementCompletedCounter(QuestState questState, int increment)
        {
            var def = _questDefsDictionary[questState.Id];
            if (def.MinLevel > PlayerLevel) {
                return;
            }

            if (questState.CompleteRegistred && questState.RevenueUsed)
                return;

            if (IsQuestInCooldown(CommandTimestamp, questState.Id))
            {
                return;
            }

            questState.Completed += increment;
            try
            {
                OnQuestCompletedIncrement.Schedule(questState.Id, questState.Completed);
            }
            catch (Exception e)
            {
                Log("OnQuestCompletedIncrement exception: " + e.Message);
            }
        }

        internal void ResetQuestByType(string questType) {
            for (var i = 0; i < _questStates.Count; i++)
            {
                var questId = _questStates[i].Id;
                var def = _questDefsDictionary[questId];
                if (def.QuestType == questType)
                {
                    _questStates[i].Completed = 0;
                    try
                    {
                        OnQuestCompletedIncrement.Schedule(_questStates[i].Id, _questStates[i].Completed);
                    }
                    catch (Exception e)
                    {
                        Log("OnQuestCompletedIncrement exception: " + e.Message);
                    }
                }
            }
        }

        public void IncrementQuestByType(string questType, int increment)
        {
            SharedCommandCallstack.CheckCallstack();
            Log("QuestModuleLog IncrementCompletedCounter: " + questType + " increment: " + increment);
            IncrementCompletedCounter((def) => def.QuestType == questType, increment, CommandTimestamp);
        }

        [SharedCommand]
        internal void IncrementQuestByTypeSL(string questType, int increment)
        {
            IncrementQuestByType(questType, increment);
        }
        
        [SharedCommand]
        internal void IncrementQuestById(string questId, int delta)
        {
            IncrementCompletedCounter(x => x.Id == questId, delta, CommandTimestamp);
        }

        internal void IncrementCompletedCounter(Func<QuestDef, bool> questChecker, int increment, DateTime time)
        {
            var questStates = _questStates;

            for (var i = 0; i < questStates.Count; i++)
            {
                QuestDef questDef = null;
                for (int j = 0; j < _questDefs.Count; j++)
                {
                    if (_questDefs[j].Id == questStates[i].Id)
                    {
                        questDef = _questDefs[j];
                        break;
                    }
                }
                if (questDef == null)
                    continue;

                if (questChecker(questDef))
                {
//                    Debug.Log("QuestModuleLog IncrementCompletedCounter: " + questDef.Id + " increment: " + increment);

                    IncrementCompletedCounter(questStates[i], increment);
                }
            }

            CheckCompletion(time);
        }

        protected void CheckCompletion(DateTime time)
        {
//            Debug.Log("QuestModuleLog CheckCompletion");
            var questStates = _questStates;
            for (var i = questStates.Count - 1; i >= 0; i--)
            {
                QuestState questState = questStates[i];
                QuestDef questDef = null;
                for (int j = 0; j < _questDefs.Count; j++)
                {
                    if (_questDefs[j].Id == questStates[i].Id)
                    {
                        questDef = _questDefs[j];
                        break;
                    }
                }
                
                if (questDef == null)
                    continue;

                if (IsQuestInCooldown(time, questState))
                {
                    continue;
                }

                if (!questState.CompleteRegistred && questState.Completed >= questDef.Amount)
                {
//                    Debug.Log("QuestModuleLog CompleteQuest: " + questDef.Id);
                    CompleteQuest(questDef.Id, time);
                }
            }
        }
       
        protected bool IsQuestInCooldown(DateTime time, QuestState quest)
        {
            var questDef = _questDefs.First(x => x.Id == quest.Id);
            if (questDef.QuestClass == "Everyday")
                return IsEverydayQuestInCooldown(quest);

            var endCooldownTimestamp = new DateTime(quest.Timestamp) + new TimeSpan(0,0,0, questDef.CooldownTime);
            return time < endCooldownTimestamp;
        }

        public bool IsQuestInCooldown(DateTime time, string questId)
        {
            for (int i = 0; i < _questStates.Count; i++)
            {
                if (_questStates[i].Id == questId)
                    return IsQuestInCooldown(time, _questStates[i]); 
            }
            return false;
        }

        public bool IsEverydayQuestInCooldown(QuestState quest)
        {
            for (int i = 0; i < _questStates.Count; i++)
            {
                if (_questStates[i].Id == quest.Id)
                {
                    var timestampDatetime = new DateTime(_questStates[i].Timestamp);
                        return CommandTimestamp.Day == timestampDatetime.Day;
                }
            }

            return false;
        }

        internal void CompleteQuest(string questId, DateTime time)
        {
            var questState = GetQuestState(questId);
            var questDef = _questDefs.First(x => x.Id == questState.Id);

            if (questState.Completed < questDef.Amount)
            {
                throw new Exception("Can't complete quest with id = " + questId + " questState.Completed: " + questState.Completed + " questDef.Amount: " + questDef.Amount);
            }

            if (!questState.CompleteRegistred)
            {
                questState.CompleteRegistred = true;
                IncrementQuestByTypeSL(QuestType.COMPLETE_MISSIONS, 1);
                OnQuestCompleted.Schedule(questDef);
                OnQuestCompletedInternal?.Invoke(questId);
            }
        }

        public List<QuestDef> ComplitedQuestByClass(string questClass)
        {
            List<QuestDef> questDef = new List<QuestDef> ();
            for (var i = _questStates.Count - 1; i >= 0; i--)
            {
                var def = _questDefsDictionary[_questStates[i].Id];
                if (def.QuestClass == questClass && _questStates[i].CompleteRegistred && _questStates[i].RevenueUsed)
                {
                    questDef.Add(def);
                }
            }
            return questDef;
        }

        [SharedCommand]
        internal void GiveQuestRevenue(string questId, bool doubleRevenue)
        {
            var questState = GetQuestState(questId);
            if (questState.RevenueUsed) return;

            questState.RevenueUsed = true;
            GiveRevenue(questState, doubleRevenue, CommandTimestamp);
            
            
            foreach (var questDef in _questDefs)
            {
                if (questDef.Id == questId)
                {
                    if (questDef.QuestClass == "Everyday")
                        GiveEverydayRevenue(questDef, doubleRevenue, CommandTimestamp);
                    else
                        break;
                }
            }

            ReplaceQuestOnNewRandom(questId, CommandTimestamp);

            OnListChanged.Schedule();
            //end of Tutorial quest
        }

        protected internal virtual void AddQuest(QuestState questState)
        {
            _questStates.Add(questState);
            SetupRevenue(questState);
            OnListChanged.Schedule();
        }

        protected virtual void InsertQuest(int index, QuestState questState)
        {
            _questStates.Insert(index, questState);
            SetupRevenue(questState);
            OnNewQuest.Schedule(questState.Id);
            OnListChanged.Schedule();
        }

        [SharedCommand]
        internal void RemoveQuest(string questId)
        {
            var questToRemove = _questStates.FirstOrDefault(x => x.Id == questId);
            State.Quests.Remove(questToRemove);
            //_questStates.Remove(questToRemove);
        }
        
        [SharedCommand]
        internal void CleanAbsentQuests()
        {
            for (var i = _questStates.Count - 1; i >= 0; i--)
            {
                if (!_questDefsDictionary.ContainsKey(_questStates[i].Id))
                {
                    RemoveQuest(_questStates[i].Id);
                }
            }
        }


        internal void RemoveQuestByClass(string questClass)
        {
            for (var i = _questStates.Count - 1; i >= 0; i--)
            {
                var def = _questDefsDictionary[_questStates[i].Id];
                if (def.QuestClass == questClass)
                {
                    _questStates.RemoveAt(i);
                }
            }
            OnListChanged.Schedule();
        }

        public TimeSpan GetTimeFromLastReroll(DateTime now)
        {
            var lastReroll = new DateTime(State.LastQuestRerollTimestamp);
            return now - lastReroll;
        }

        public TimeSpan GetAdsRevenueX2Cooldown(DateTime now)
        {
            TimeSpan cooldown = RevenueCooldown;
            DateTime lastAdsTime = GetLastAdsRevenueX2Date();
            return cooldown - new TimeSpan(now.Ticks - lastAdsTime.Ticks);
        }

        private DateTime GetLastAdsRevenueX2Date()
        {
            return new DateTime(State.LastQuestRevenueX2Timestamp);
        }

        [SharedCommand]
        internal void SetAdsRevenueX2TimeStamp()
        {
            State.LastQuestRevenueX2Timestamp = CommandTimestamp.Ticks;
        }

        public bool IsRerollInCooldown(DateTime now)
        {
            return (GetTimeFromLastReroll(now) < RerollCooldown);
        }

      
        [SharedCommand]
        internal void RerollQuest(string questId)
        {
            if (IsRerollInCooldown(CommandTimestamp) && (State.QuestRerollCounter == MaxRerollCounter))
            {              
                OnRerollFailed.Schedule(RerollCooldown - GetTimeFromLastReroll(CommandTimestamp));
                return;
            }

            if (!IsRerollInCooldown(CommandTimestamp))
            {
                State.QuestRerollCounter = 0; //reset reroll counter
            }

            State.LastQuestRerollTimestamp = CommandTimestamp.Ticks;

            ReplaceQuestOnNewRandom(questId, CommandTimestamp - TimeSpan.FromDays(365));

            State.QuestRerollCounter++;
        }

        public bool InQuestReroll()
        {
            return (State.QuestRerollCounter == MaxRerollCounter);
        }

        public TimeSpan GetTimeToNextReroll(DateTime now)
        {
            return RerollCooldown - GetTimeFromLastReroll(now);
        }

        public int RerollCounter
        {
            get { return State.QuestRerollCounter; }
        }

        protected virtual void ReplaceQuestOnNewRandom(string questId, DateTime time)
        {
            var questDef = _questDefsDictionary[questId];
            QuestCooldownStarted.Schedule(questDef.CooldownTime);

            var filtredQuests = _questDefs.Where(x => 
                _questStates.All(q => q.Id != x.Id)  //нет таких же квестов в стейтах
                && x.MinLevel <= PlayerLevel && PlayerLevel <= x.MaxLevel //квест подходит по уровню
                && x.CanBeGivenByRandom //квест может быть выдан механизмом случайной выдачи. Ачивки к примеру не могут быть выданы
                && x.QuestClass == questDef.QuestClass //нельзя заменить квест одного класса на квест другого класса
                && x.QuestGroupIndex == questDef.QuestGroupIndex //квесты должны быть из одной группы
            ).ToArray();

            var questToRemove = _questStates.FirstOrDefault(x => x.Id == questId);
            var indexToRemove = _questStates.IndexOf(questToRemove);

            //туториальные квесты не удаляем из стейта никогда, что бы они не выдались повторно
            //ещё не удаляем ачивки максимального уровня
            if (!_questDefsDictionary[questId].KeepInState)
            {
                _questStates.RemoveAt(indexToRemove);
            }
           
            //если задан конкретный следующий квест, заменяем существующий на заданный
            //"избыточный" прогресс при этом переносится на новый квест.
            if (!string.IsNullOrEmpty(questDef.NextLevelQuestId))
            {
                var nextLevelAchievement = new QuestState()
                {
                    Id = questDef.NextLevelQuestId,
                    Timestamp = GetQuestTimestamp(questDef.NextLevelQuestId, time),
                    Completed =
                        questToRemove.Completed - questDef.Amount //transfer completed counter on next level achievement
                };
                
                InsertQuest(indexToRemove, nextLevelAchievement);
                OnQuestCompletedIncrement.Schedule(nextLevelAchievement.Id, nextLevelAchievement.Completed);
                CheckCompletion(CommandTimestamp);
            }
            //если среди доступных квестов есть хотя бы один туториальный, добавляем именно его
            else if (filtredQuests.Any(x => x.IsTutorial))
            {
                var tutorialQuest = filtredQuests.First(x => x.IsTutorial);

                InsertQuest(indexToRemove, new QuestState
                {
                    Id = tutorialQuest.Id,
                    Timestamp = GetQuestTimestamp(tutorialQuest.Id, time),
                    Completed = GetInitialCompleted(tutorialQuest.Id)
                });

                CheckCompletion(time);
            }
            else if (filtredQuests.Length > 0)
            {
                //иначе пытаемся добавить обычный
                var randomIndex = _randomModule.RandomInt(filtredQuests.Length);
                var randomQuestDef = filtredQuests[randomIndex];

                InsertQuest(indexToRemove, new QuestState
                {
                    Id = randomQuestDef.Id,
                    Timestamp = GetQuestTimestamp(randomQuestDef.Id, time),
                    Completed = GetInitialCompleted(randomQuestDef.Id)
                });

                CheckCompletion(time);
            }

            OnListChanged.Schedule();
        }

        protected virtual long GetQuestTimestamp(string questId, DateTime time)
        {
            return time.Ticks;
        }

        protected virtual int GetInitialCompleted(string questId)
        {
            return 0;
        }

        protected QuestState GetQuestState(string questId)
        {
            var questState = _questStates.FirstOrDefault(x => x.Id == questId);
            if (questState == null)
            {
                throw new Exception("Can't find quest with id = " + questId);
            }
            return questState;
        }

        [SharedCommand]
        internal void QuestWatchAds(string id)
        {
            var quests = _questStates.Where(_ => _.Id == id);
            foreach (var quest in quests)
            {
                quest.Timestamp -= TimeSpan.FromMinutes(30).Ticks;
                if (quest.Timestamp < 0)
                {
                    quest.Timestamp = 0;
                }
            }

            OnTimeUpdated.Schedule();
        }

        public virtual bool AnyCompletedQuests
        {
            get { return _questStates.Any(x => x.CompleteRegistred && !x.RevenueUsed); }
        }

        public bool CompletedQuestsForClass(string questClass)
        {
             var typeQuest = _questDefs.FindAll(x => x.QuestClass == questClass);

             List<QuestState> statType = new List<QuestState>();
             foreach (QuestState state in _questStates)
                foreach (QuestDef def in typeQuest)
                    if (def.Id == state.Id)
                        statType.Add(state);

             return statType.Any(x => x.CompleteRegistred && !x.RevenueUsed); 
        }

        private bool IsQuestLockedByLevel(QuestDef def)
        {
            return def.MinLevel > PlayerLevel || PlayerLevel > def.MaxLevel;
        }

        public bool HasActiveQuests(DateTime now)
        {
            for (int i = 0; i < _questStates.Count; ++i)
            {
                var quest = _questStates[i];
                var questDef = _questDefs.First(x => x.Id == quest.Id);

                if (quest.Completed >= questDef.Amount)
                    continue;

                if (quest.RevenueUsed)
                    continue;

                if (IsQuestLockedByLevel(questDef))
                    continue;

                if (!IsQuestInCooldown(now, quest))
                    return true;
            }

            return false;
        }

        public bool HasQuestsToReroll(DateTime now, string questClass)
        {
            for (int i = 0; i < _questStates.Count; ++i)
            {
                var quest = _questStates[i];
                var questDef = _questDefs.First(x => x.Id == quest.Id);

                if (quest.Completed >= questDef.Amount)
                    continue;

                if (quest.RevenueUsed)
                    continue;

                if (IsQuestLockedByLevel(questDef))
                    continue;

                if (!IsQuestInCooldown(now, quest))
                {
                    if (questDef.QuestClass == questClass)
                        return true;
                }
            }

            return false;
        }

        public virtual TimeSpan RerollCooldown
        {
            get { return TimeSpan.FromDays(1); }
        }

        public virtual TimeSpan RevenueCooldown
        {
            get { return TimeSpan.FromHours(3); }
        }

        public virtual int MaxRerollCounter
        {
            get { return 2; }
        }

        protected virtual int PlayerLevel
        {
            get { return 1; }
        }

        /*
         * метод для настройки QuestState.Reward в соответствии с уровнем игрока, его инвентарём и прочими условиями.
         */
        protected virtual void SetupRevenue(QuestState questState)
        {
            var questDef = _questDefsDictionary[questState.Id];

            foreach (var sourceChestReward in _chestsRewardDefs.Where(x => x.Tag == questDef.RevenueId))
            {
                var rewardDef = CloneMessagePackObject(sourceChestReward);

                switch (rewardDef.ItemType)
                {
                    case "ingame":
                        break;
                    case "real":
                        break;
                    case "ship":
                        var allShips = new[] { "ArbitrCruiser", "ArchangelBattleship" };
                        rewardDef.ItemId = allShips[_randomModule.RandomInt(allShips.Length)];
                        break;
                    case "item":
                        var allItems = new[] { "AdditionalMissiles", "ClusterMissiles" };
                        rewardDef.ItemId = allItems[_randomModule.RandomInt(allItems.Length)];
                        break;
                        /*
                        case "weapon":
                             //В сундуке может быть указано, что он даёт какое-то определенное оружие
                             //тогда у него будет в rewardDef UseRarityAndTier == false
                             //А может быть просто случайное оружие заданной редкости и тира
                            if (rewardDef.UseRarityAndTier)
                            {
                                var allPossibleWeapons = _weaponDefs.Where(x => x.Tier == rewardDef.Tier && x.Rarity == rewardDef.Rarity).ToList();
                                if (allPossibleWeapons.Count == 0) return false; //если мы не можем выдать ничего игроку подходящего по параметрам, нужно вернуть false

                                if (rewardDef.UseMaxPlayerTier)
                                {
                                    //тут можно дополнительно отфильтровать слишком высокоуровневое для игрока оружие
                                }

                                //не забывайте что нельзя рандом не из ШЛ использовать внутри команды ШЛ
                                //var randomWeaponDef = allPossibleWeapons[_randomModule.RandomInt(allPossibleWeapons.Count)];

                                rewardDef.ItemId = randomWeaponDef.Id; //нужно поставить в ItemType, что бы сундук мог в итоге показать, что в нём выпало
                                                                       //тут должен быть сам код, который добавляет в стейт игрока оружие по дефу randomWeaponDef
                            }
                            else
                            {
                                var weaponDef = _weaponDefsDict[rewardDef.ItemId];
                                //в этом случае геймдизайнером уже задано, какое должно выдаться оружие
                            }
                            break;
                            */
                }

                questState.Rewards.Add(rewardDef);
            }
        }

        protected virtual void GiveRevenue(QuestState questDef, bool doubleRevenue, DateTime dateTime)
        {

        }
        
        private void GiveEverydayRevenue(QuestDef questDef, bool doubleRevenue, DateTime dateTime)
        {
            for (int i = 0; i < State.Quests.Count; i++)
            {
                if (State.Quests[i].Id == questDef.Id)
                {
                    State.Quests[i].Timestamp = new DateTime(CommandTimestamp.Year,CommandTimestamp.Month,CommandTimestamp.Day).Ticks;
                    State.Quests[i].CompleteRegistred = false;
                    State.Quests[i].RevenueUsed = false;
                    State.Quests[i].Completed = 0;
                }
            }
        }
    }
}
