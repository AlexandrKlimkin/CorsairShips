using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using log4net;
using MessagePack;
using PestelLib.Serialization;
using PestelLib.SharedLogicBase;
using S;
using ServerShared;
using ServerShared.GlobalConflict;
using ServerShared.Sources.GlobalConflict;
using UnityDI;

namespace PestelLib.SharedLogic.Modules
{
    public class GlobalConflictModuleBase : SharedLogicModule<GlobalConflictModuleState>
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(GlobalConflictModuleBase));
        [GooglePageRef("GC_LeaderboardRewards")]
        [Dependency] protected List<ConflictLeaderboardRewardDef> _leaderboardRewardDefs;
        [GooglePageRef("GC_TeamRewards")]
        [Dependency] protected List<ConflictTeamRewardDef> _teamRewardDefs;

        private GlobalConflictApi _api;
        [Dependency]
        private RandomModule _randomModule;
        private IQuestNodePicker _questNodePicker;

        private GlobalConflictState _currentConflictState;
        private PlayerState _playerState;
        private PointOfInterest[] _myPointsOfInterests;
        private PointOfInterest[] _enemyPointsOfInterests;
        private object _sync = new object();
        private DateTime _lastUpdate;
        private TimeSpan _updateDelay = TimeSpan.FromSeconds(1);

        public ScheduledAction<ConflictResult> OnConflictResultAvailable;
        public ScheduledAction<GlobalConflictDeployedQuest> OnQuestCompleted;
        public ScheduledAction<GlobalConflictDeployedQuest> OnQuestExpired;

        public event Action Initalized = () => { };
        public event Action PlayerStateUpdated = () => { };

        public GlobalConflictModuleBase()
        {
            OnConflictResultAvailable = new ScheduledAction<ConflictResult>(ScheduledActionCaller);
            OnQuestCompleted = new ScheduledAction<GlobalConflictDeployedQuest>(ScheduledActionCaller);
            OnQuestExpired = new ScheduledAction<GlobalConflictDeployedQuest>(ScheduledActionCaller);


            _api = ContainerHolder.Container.Resolve<GlobalConflictApi>();
#if !UNITY_5_5_OR_NEWER
            _currentConflictState = _api.ConflictsScheduleApi.GetCurrentConflict();
            if (_currentConflictState != null)
                _playerState = _api.PlayersApi.GetPlayer(PlayerId, _currentConflictState.Id);
#else
            PlayerStateUpdated += UpdateState;
#endif
        }

        public override void MakeDefaultState()
        {
            base.MakeDefaultState();
            State.Quests = new List<GlobalConflictDeployedQuest>();
            State.ClaimedRewards = new List<string>();
            State.ConflictResults = new List<ConflictResult>();
            State.VisitedConflicts = new List<string>();
        }


        protected virtual decimal MaxEnergyBonus
        {
            get { return 1m; }
        }

        protected virtual decimal RestoreRateBonus
        {
            get { return 1m; }
        }

        protected virtual int DonatorsTopSize
        {
            get { return 100; }
        }

        protected virtual int WinPointsTopSize
        {
            get { return 100; }
        }

        public virtual int MaxEnergy
        {
            get { return (int)(10 * MaxEnergyBonus); }
        }

        public int CurrentEnergy
        {
            get { return State.Energy; }
        }

        public virtual int GetMaxQuestsAtLevel(int questLevel)
        {
            return 1;
        }

        public virtual int GetQuestsToLevelup(int questLevel)
        {
            return 1;
        }

        // ед в минуту
        protected virtual decimal EnergyRestoreRate
        {
            get { return .5m * RestoreRateBonus; }
        }

        protected decimal EnergyRestoreRatePerTick
        {
            get
            {
                return EnergyRestoreRate / TimeSpan.TicksPerMinute;
            }
        }

        /// <summary>
        /// Вычисляет время необходимое для восстановления указанного количества энергии
        /// </summary>
        /// <param name="neededEnergy"></param>
        /// <returns></returns>
        public TimeSpan TimeToRestoreEnergy(int neededEnergy)
        {
            return TimeSpan.FromMinutes((double)(neededEnergy / EnergyRestoreRate));
        }

        /// <summary>
        /// Возвращает количество энергии недостающее для выполнения атаки на указанную ноду
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public int GetMissingEnergyToAttack(int nodeId)
        {
            var cost = GetBattleCost(nodeId);
            var diff = cost - State.Energy;
            if (diff < 0)
                return 0;
            return diff;
        }

        /// <summary>
        /// Если недостаточно энергии для атаки, то возвращает время, через которое можно атаковать указанную точку
        /// иначе возвращает TimeSpan.Zero
        /// </summary>
        /// <returns></returns>
        public TimeSpan TimeToAttackNode(int nodeId)
        {
            var diff = GetMissingEnergyToAttack(nodeId);
            if(diff < 1)
                return TimeSpan.Zero;
            var dt = CommandTimestamp - new DateTime(State.LastEnergyRecalcTime);
            return TimeToRestoreEnergy(diff) - dt;
        }

        [SharedCommand]
        internal void ClaimReward(ConflictResult result)
        {
            UniversalAssert.IsTrue(!State.ClaimedRewards.Contains(result.ConflictId), "Rewards for conflict " + result.ConflictId + " already claimed");
            if(State.ClaimedRewards.Contains(result.ConflictId))
                return;
            State.ClaimedRewards.Add(result.ConflictId);
            GiveReward(result);
        }

        protected virtual void GiveReward(ConflictResult result)
        {
            _log.Debug("Pretend we are giving reward " + result.TeamRewardId + ", " + result.LeaderboardRewardId);
        }

        private string PlayerId
        {
            get { return SharedLogic.PlayerId.ToString(); }
        }

        private void InvalidatePlayerState()
        {
            _playerState = null;
        }

        /// <summary>
        /// Для облегчения инициализации сервера создание IQuestNodePicker вынесено в конкретную реализацию а не в DI
        /// Перегрузите для добавления вашей реализации алгоритма выбора места для квеста
        /// </summary>
        /// <returns></returns>
        protected virtual IQuestNodePicker CreateQuestNodePicker()
        {
            return new DefaultQuestNodePicker(max => _randomModule.RandomInt(max));
        }

        private IQuestNodePicker QuestNodePicker {
            get
            {
                if (_questNodePicker != null)
                    return _questNodePicker;
                _questNodePicker = CreateQuestNodePicker();
                return _questNodePicker;
            }
        }

        [SharedCommand]
        internal void Update()
        {
            RecalcEnergy();
            if (_currentConflictState != null)
            {
                if (!State.VisitedConflicts.Contains(_currentConflictState.Id))
                    State.VisitedConflicts.Add(_currentConflictState.Id);
                var stage = GetCurrentStage();
                if (_playerState == null)
                {
                    State.Quests = new List<GlobalConflictDeployedQuest>();
                    State.QuestLevel = 0;
                }
            }
        }

        private void GetMyState(Action callback)
        {
            GetCurrentConflictState(() =>
            {
                var playerId = PlayerId;
                if (_currentConflictState == null)
                {
                    _playerState = null;
                    PlayerStateUpdated();
                    callback();
                    return;
                }

                _api.PlayersApi.GetPlayer(playerId, _currentConflictState.Id, s =>
                {
                    _playerState = s;
                    PlayerStateUpdated();
                    callback();
                });
            });
        }

        private void GetCurrentConflictState(Action callback)
        {
            if (DateTime.UtcNow - _lastUpdate < _updateDelay)
            {
                callback();
                return;
            }

            _api.ConflictsScheduleApi.GetCurrentConflict((c) =>
            {
                _lastUpdate = DateTime.UtcNow;
                _currentConflictState = c;
                callback();
            });
        }

        private void GetPointsOfInterest(Action callback)
        {
            var teamsLoaded = 0;
            GetMyState(() =>
            {
                if (!HasActiveConflict)
                {
                    callback();
                    return;
                }

                for (var i = 0; i < _currentConflictState.Teams.Length; ++i)
                {
                    var team = _currentConflictState.Teams[i];
                    _api.PointOfInterestApi.GetTeamPointsOfInterest(_currentConflictState.Id, team, pois =>
                    {
                        lock (_sync)
                        {
                            ++teamsLoaded;
                            if (_playerState.TeamId == team)
                            {
                                _myPointsOfInterests = pois;
                            }
                            else
                            {
                                if(_enemyPointsOfInterests == null)
                                    _enemyPointsOfInterests = pois;
                                else
                                    _enemyPointsOfInterests = _enemyPointsOfInterests.Union(pois).ToArray();
                            }
                        }
                        if (teamsLoaded == _currentConflictState.Teams.Length)
                            callback();
                    });
                }
            });
        }

        public void SetPlayerName(string name, Action callback)
        {
            _api.PlayersApi.SetName(PlayerId, name, callback);
        }

        public PlayerState PlayerState
        {
            get
            {
                return MessagePackSerializer.Deserialize<PlayerState>(MessagePackSerializer.Serialize(_playerState));
            }
        }

        public void AddDonation(int amount, Action callback)
        {
            var id = _playerState.Id;
            InvalidatePlayerState();
            _api.DonationApi.Donate(id, amount, callback);
        }

        public StageInfo GetCurrentStage()
        {
            var result = new StageInfo();
            if (!HasActiveConflict)
                return null;
            result.Start = _currentConflictState.StartTime;
            var fromStart = DateTime.UtcNow - _currentConflictState.StartTime;
            for (var i = 0; i < _currentConflictState.Stages.Length; ++i)
            {
                var s = _currentConflictState.Stages[i];
                fromStart -= s.Period;
                if (fromStart <= TimeSpan.Zero)
                {
                    result.Id = s.Id;
                    result.Period = s.Period;
                    result.End = result.Start + s.Period;
                    return result;
                }

                result.Start += s.Period;
            }

            return null;
        }

        public GlobalConflictState ConflictState
        {
            get
            {
                return MessagePackSerializer.Deserialize<GlobalConflictState>(MessagePackSerializer.Serialize(_currentConflictState));
            }
        }

        public bool HasActiveConflict
        {
            get { return _currentConflictState != null; }
        }

        public bool IamInConflict
        {
            get { return PlayerState != null && ConflictState != null; }
        }

        public NodeQuest GetQuest(string questId)
        {
            var q = GetQuestInternal(questId);
            return q.Clone();
        }

        private NodeQuest GetQuestInternal(string questId)
        {
            if (_currentConflictState == null)
                return null;
            return _currentConflictState.Quests.FirstOrDefault(_ => _.Id == questId);
        }

        private bool HasQuestLevel(int level)
        {
            UniversalAssert.IsNotNull(_currentConflictState, "Current conflict is null. Call init first");
            return _currentConflictState.Quests.Any(_ => _.QuestLevel == level);
        }

        /// <summary>
        /// Ваша реализация выдачи наград за выполненный квест
        /// </summary>
        /// <param name="quest"></param>
        protected virtual void GiveQuestRewards(GlobalConflictDeployedQuest quest)
        {
            _log.Debug("Pretend we are giving reward for quest " + quest.QuestId);
        }

        [SharedCommand]
        internal GlobalConflictDeployedQuest[] UpdateAndGetQuests()
        {
            if (!HasActiveConflict || !IamInConflict)
                return new GlobalConflictDeployedQuest[] {};
            var result = State.Quests.Where(_ => !_.Completed).ToArray();
            for(var i = 0; i < result.Length; ++i)
            {
                if(result[i].Completed)
                    continue;
                UpdateQuest(result[i]);
                if (result[i].Completed)
                {
                    OnQuestCompleted.Schedule(result[i]);
                    GiveQuestRewards(result[i]);
                    var lvl = result[i].Level;
                    var questsToComplete = GetQuestsToLevelup(lvl);
                    var lvlUp = State.Quests.Count(_ => _.Completed && _.Level == lvl) >= questsToComplete;
                    if (lvlUp)
                    {
                        State.Quests.RemoveAll(_ => _.Level == lvl);
                        var nextLevel = _currentConflictState.Quests
                            .Where(_ => _.QuestLevel > lvl)
                            .Select(_ => _.QuestLevel)
                            .Distinct()
                            .OrderBy(_ => _)
                            .FirstOrDefault();

                        nextLevel = Math.Max(lvl + 1, nextLevel);
                        if (State.QuestLevel < nextLevel)
                            State.QuestLevel = nextLevel;
                    }
                }
            }

            return result;
        }

        protected virtual void UpdateQuestExternal(NodeQuest def, GlobalConflictDeployedQuest quest)
        {
            // TODO: Ваша реализация распознавания типов квестов и вычисление прогресса в квесте
            // например:
            quest.Progress = (int)((CommandTimestamp.Ticks - quest.DeployTime) / ((decimal)def.ActiveTime.Ticks / 2m) * 100m);
            quest.Completed = quest.Progress >= 100; // для удобства составных квестов в стейт добавлен параметр Progress (инетрпретация значения ложится целиком на вашу реализацию, для общего кода имеет смысл только флаг Completed)
        }

        private void UpdateQuest(GlobalConflictDeployedQuest quest)
        {
            SharedCommandCallstack.CheckCallstack();
            if (quest.Completed)
                return;
            var def = GetQuestInternal(quest.QuestId);
            UpdateQuestExternal(def, quest);
            if (!quest.Completed)
            {
                var secs = (double) (CommandTimestamp.Ticks - quest.DeployTime) / TimeSpan.TicksPerMinute;
                quest.Expired = secs > def.ActiveTime.TotalMinutes || GetCurrentStage().Id != StageType.Battle;
                if (quest.Expired)
                {
                    OnQuestExpired.Schedule(quest);
                }
            }
        }

        public bool CanDeployQuests
        {
            get { return HasActiveConflict && IamInConflict && GetCurrentStage().Id == StageType.Battle; }
        }

        [SharedCommand]
        internal bool DeployQuest()
        {
            UniversalAssert.IsNotNull(_currentConflictState, "Current conflict is null. Call init first");
            UniversalAssert.IsTrue(CanDeployQuests, "Deploy quest only in battle");
            var maxQuests = GetMaxQuestsAtLevel(State.QuestLevel);
            var deployedQuests = UpdateAndGetQuests().Where(_ => !_.Expired && _.Level == State.QuestLevel).ToArray();
            var deployedQuestsCount = deployedQuests.Length;
            var completedQuests = State.Quests.Count(_ => _.Level == State.QuestLevel && _.Completed);
            if (deployedQuestsCount >= maxQuests)
                return false;
            if (completedQuests >= GetQuestsToLevelup(State.QuestLevel))
                return false;
            var quests = _currentConflictState.Quests;
            var suitableQuests = quests.Where(_ => _.QuestLevel == State.QuestLevel)
                .Where(_ => deployedQuests.All(dq => dq.QuestId != _.Id))
                .ToArray();
            if (suitableQuests.Length < 1)
                return false;
            var quest = _randomModule.ChooseByRandom(suitableQuests, d => d.Weight);
            var node = QuestNodePicker.PickNode(_currentConflictState, _playerState.TeamId, State.Quests.ToArray());
            if (node == null)
                return false;
            var deployed = new GlobalConflictDeployedQuest()
            {
                QuestId = quest.Id,
                NodeId = node.Id,
                DeployTime = CommandTimestamp.Ticks,
                Level = quest.QuestLevel
            };
            UpdateQuest(deployed);
            State.Quests.Add(deployed);
            return true;
        }

        [SharedCommand]
        internal void RecalcEnergy()
        {
            if (State.LastEnergyRecalcTime == 0)
            {
                State.LastEnergyRecalcTime = CommandTimestamp.Ticks;
                State.Energy = MaxEnergy;
                return;
            }

            if(State.Energy == MaxEnergy)
                return;

            var dt = CommandTimestamp - new DateTime(State.LastEnergyRecalcTime);
            if(dt <= TimeSpan.Zero)
                return;

            var dtTicks = dt.Ticks; // не юзаем float, double (т.е. DateTime.TotalMinutes запрещено юзать)
            var energyToRestoreD = dtTicks * EnergyRestoreRatePerTick;
            var energyToRestore = (int) energyToRestoreD;
            State.Energy = Math.Min(energyToRestore + State.Energy, MaxEnergy);

            var tickBack = (energyToRestoreD - energyToRestore) / EnergyRestoreRatePerTick;
            State.LastEnergyRecalcTime = CommandTimestamp.Ticks - (long) tickBack;
        }

        protected virtual decimal CalculateWinMod(int nodeId)
        {
            return 1m;
        }

        protected virtual decimal CalculateLoseMod(int nodeId)
        {
            return 1m;
        }

        public NodeState GetNode(int nodeId)
        {
            return _currentConflictState.Map.Nodes.FirstOrDefault(_ => _.Id == nodeId);
        }

        public virtual int GetBattleCost(int nodeId)
        {
            return _currentConflictState.BattleCost;
        }

        [SharedCommand]
        internal bool BattleForNode(int nodeId)
        {
            RecalcEnergy();
            var cost = GetBattleCost(nodeId);
            UniversalAssert.IsNotNull(_currentConflictState, "Current conflict is null. Call init first");
            UniversalAssert.IsTrue(State.Energy >= cost, "Not enough energy. Have " + State.Energy + ". Need " + cost);
            if (State.Energy < cost)
            {
                Log(PlayerId + ": Not enough energy for battle");
                return false;
            }

            if (State.Energy >= MaxEnergy)
            {
                State.LastEnergyRecalcTime = CommandTimestamp.Ticks;
            }
            State.Energy -= cost;
            return true;
        }

        public void BattleForNodeResult(int nodeId, bool win)
        {
#if UNITY_5_6_OR_NEWER
            var winMod = win ? CalculateWinMod(nodeId) : 0;
            var loseMod = !win ? CalculateLoseMod(nodeId) : 0;
            _api.BattleApi.RegisterBattleResult(_playerState.Id, nodeId, win, winMod, loseMod, () => { });
#endif
        }

        public void Init()
        {
            Init(UpdateClientOnInit);
        }

        private void UpdateClientOnInit()
        {
#if UNITY_5_5_OR_NEWER
            UpdateResults(result =>
            {
                if(result == null)
                    return;
                var cmd = new GlobalConflictModuleBase_RegisterResult()
                {
                    result = result
                };
                PestelLib.SharedLogicClient.CommandProcessor.Process<object, GlobalConflictModuleBase_RegisterResult>(cmd);
            });
#endif
        }

        private void UpdateState()
        {
#if UNITY_5_5_OR_NEWER
            var cmd = new GlobalConflictModuleBase_Update();
            PestelLib.SharedLogicClient.CommandProcessor.Process<object, GlobalConflictModuleBase_Update>(cmd);
#endif
        }

        public void Init(Action callback)
        {
            GetCurrentConflictState(() =>
            {
                Log("Global conflict: Conflict state loaded");
                GetMyState(() =>
                {
                    Log("Global conflict: Player state loaded");
                    if (!IamInConflict)
                    {
                        Initalized();
                        callback();
                        return;
                    }

                    GetPointsOfInterest(() =>
                    {
                        Log("Global conflict: Points of interest loaded");
                        Initalized();
                        callback();
                    });
                });
            });
        }

        [SharedCommand]
        internal void RegisterResult(ConflictResult result)
        {
            UniversalAssert.IsTrue(State.ConflictResults.All(_ => _.ConflictId != result.ConflictId), "Conflict " + result.ConflictId + " already registered");
            var win = result.WinnerTeam == result.MyTeam;
            var leaderboardReward = _leaderboardRewardDefs.FirstOrDefault(_ =>
                _.ConflictId == _currentConflictState.Id
                && _.Sector == result.MySector && _.ForLoser != win
            ) ?? _leaderboardRewardDefs.FirstOrDefault(_ =>
                                        string.IsNullOrEmpty(_.ConflictId)
                                        && _.Sector == result.MySector && _.ForLoser != win
                                    );
            var teamReward = _teamRewardDefs.FirstOrDefault(_ =>
                _.ConflictId == _currentConflictState.Id && _.ForLoser ^ win) 
                ?? _teamRewardDefs.FirstOrDefault(_ =>
                                 string.IsNullOrEmpty(_.ConflictId) && _.ForLoser ^ win);

            result.LeaderboardRewardId = leaderboardReward != null ? leaderboardReward.RewardId : null;
            result.TeamRewardId = teamReward != null ? teamReward.RewardId : null;

            State.ConflictResults.Add(result);

            OnConflictResultAvailable.Schedule(result.Clone());
        }

        public void AddPointOfInterest(PointOfInterest pointOfInterest, bool team, Action<bool> callback)
        {
        }

        public enum ClientBonusFilter : byte
        {
            None = 0,
            PlayerDonations = 1,
            TeamDonations = 2,
            TeamInterests= 4,
            EnemyInterests = 8,
            All = 15
        }

        public PointOfInterest[] GetAllyNotDeployedPointsOfInterest()
        {
            UniversalAssert.IsNotNull(_playerState, "Player state is null. Call init first");
            UniversalAssert.IsNotNull(_currentConflictState, "Current conflict is null. Call init first");
            var deployedIds = GetAllyDeployedPointsOfInterest().Select(_ => _.Id);
            return 
                _currentConflictState.PointsOfInterest
                        .Where(_ => _.NextDeploy > DateTime.UtcNow && 
                                    (string.IsNullOrEmpty(_.OwnerTeam) || _.OwnerTeam == _playerState.TeamId) &&
                                    !deployedIds.Contains(_.Id)).ToArray();
        }

        public PointOfInterest[] GetAllyDeployedPointsOfInterest()
        {
            UniversalAssert.IsTrue(_myPointsOfInterests != null && _enemyPointsOfInterests != null, "Points of interest not loaded. Call init first");
            return _myPointsOfInterests;
        }

        public PointOfInterest[] GetEnemyPointsOfInterest()
        {
            UniversalAssert.IsTrue(_myPointsOfInterests != null && _enemyPointsOfInterests != null, "Points of interest not loaded. Call init first");
            return _enemyPointsOfInterests;
        }

        public void DeployPointOfInterest(PointOfInterest point, int nodeId, Action<bool> callback)
        {
            UniversalAssert.IsNotNull(_currentConflictState, "Current conflict is null. Call init first");
            UniversalAssert.IsNotNull(_playerState, "Player state is null. Call init first");
            _api.PointOfInterestApi.DeployPointOfInterest(_currentConflictState.Id, _playerState.Id,
                _playerState.TeamId, nodeId, point.Id, b =>
                {
                    if (b)
                        Init(() => { callback(true); });
                    else
                        callback(false);
                });
        }

        /// <summary>
        /// Возвращает в колбэк топ донаторов (PlayerState[]) и место игрока в топе (long)
        /// </summary>
        /// <param name="myTeamOnly">true чтобы показать топ только среди своей команды</param>
        /// <param name="callback"></param>
        public void GetTopDonators(bool myTeamOnly, Action<PlayerState[], long> callback)
        {
            var team = myTeamOnly ? _playerState.TeamId : null;
            _api.LeaderboardsApi.GetDonationTop(_currentConflictState.Id, team, 0, DonatorsTopSize,
                (top) =>
                {
                    var idx = top.IndexOf(_ => _.Id == _playerState.Id);
                    if (idx > -1)
                        callback(top, idx + 1);
                    else
                        _api.LeaderboardsApi.GetDonationTopMyPosition(_playerState.Id, myTeamOnly, _currentConflictState.Id,
                            (myPlace) => { callback(top, myPlace); });
                });
        }

        /// <summary>
        /// Возвращает в колбэк топ игроков по WinPoint (PlayerState[]) и место игрока в топе (long)
        /// </summary>
        /// <param name="myTeamOnly">true чтобы показать топ только среди своей команды</param>
        /// <param name="callback"></param>
        public void GetTopWinPoints(bool myTeamOnly, Action<PlayerState[], long> callback)
        {
            var team = myTeamOnly ? _playerState.TeamId : null;
            _api.LeaderboardsApi.GetWinPointsTop(_currentConflictState.Id, team, 0, WinPointsTopSize, (top) =>
            {
                var idx = top.IndexOf(_ => _.Id == _playerState.Id);
                if (idx > -1)
                    callback(top, idx + 1);
                else
                    _api.LeaderboardsApi.GetWinPointsTopMyPosition(_playerState.Id, myTeamOnly, _currentConflictState.Id,
                        myPlace => { callback(top, myPlace); });
            });
        }

        public PointOfInterest[] GetPointsOfInterestOnNode(int nodeId)
        {
            if (!IamInConflict || !HasActiveConflict)
                return new PointOfInterest[] { };
            UniversalAssert.IsNotNull(_currentConflictState, "Current conflict is null. Call init first");
            UniversalAssert.IsNotNull(_playerState, "Player state is null. Call init first");
            UniversalAssert.IsNotNull(_myPointsOfInterests, "POIs in null");

            var deployed = _myPointsOfInterests.Where(_ => _.NodeId == nodeId && _.IsDeployed(CommandTimestamp)).ToArray();
            return deployed;
        }

        /// <summary>
        /// Возвращает задеплоенные точки, чей срок редеплоя не наступил
        /// </summary>
        /// <returns></returns>
        public PointOfInterest[] GetDeployedPointOfInterests()
        {
            if (!IamInConflict || !HasActiveConflict)
                return new PointOfInterest[] { };
            UniversalAssert.IsNotNull(_currentConflictState, "Current conflict is null. Call init first");
            UniversalAssert.IsNotNull(_playerState, "Player state is null. Call init first");
            UniversalAssert.IsNotNull(_myPointsOfInterests, "POIs in null");

            var deployed = _myPointsOfInterests.Where(_ => !_.IsGeneralPoi() && _.IsDeployed(CommandTimestamp)).ToArray();
            return deployed;
        }

        /// <summary>
        /// Возвращает все точки, которые можно деплоить:
        /// * поле прототипа точки Auto = false
        /// * время указанное в NextDeploy наступило от CommandTimestamp
        /// * OwnerTeam = null или равно команде текущего игрока
        /// </summary>
        /// <returns></returns>
        public PointOfInterest[] GetDeployablePointsOfInterests()
        {
            if (!IamInConflict || !HasActiveConflict)
                return new PointOfInterest[] {};
            UniversalAssert.IsNotNull(_currentConflictState, "Current conflict is null. Call init first");
            UniversalAssert.IsNotNull(_playerState, "Player state is null. Call init first");
            UniversalAssert.IsNotNull(_myPointsOfInterests, "POIs in null");
            var redeploy = _myPointsOfInterests.Where(_ => !_.IsGeneralPoi() && _.IsDeployable(_playerState, CommandTimestamp)).ToArray();
            var redeployIDs = _myPointsOfInterests.Select(_ => _.Id).ToArray();
            var deploy = _currentConflictState.PointsOfInterest.Where(_ => !redeployIDs.Contains(_.Id) && _.IsTeamValid(_playerState) && !_.IsGeneralPoi()).ToArray();

            return redeploy.Concat(deploy).ToArray();
        }

        /// <summary>
        /// Возвращает точку интереса если текущий игрок генерал, иначе null
        /// Проверье можно ли деплоить точку через PointOfInterest.IsDeployable()
        /// </summary>
        /// <returns></returns>
        public PointOfInterest GetGeneralPointOfInterest()
        {
            if (!IamInConflict || !HasActiveConflict)
                return null;
            UniversalAssert.IsNotNull(_currentConflictState, "Current conflict is null. Call init first");
            UniversalAssert.IsNotNull(_playerState, "Player state is null. Call init first");
            UniversalAssert.IsNotNull(_myPointsOfInterests, "POIs in null");
            if (_playerState.GeneralLevel < 1)
                return null;

            var deployed = _myPointsOfInterests.FirstOrDefault(_ => _.IsGeneralPoi() && _.IsGeneralValid(_playerState));
            if (deployed != null)
                return deployed;
            return _currentConflictState.PointsOfInterest.FirstOrDefault(_ => _.IsGeneralPoi() && _.IsGeneralValid(_playerState) && _.IsTeamValid(_playerState));
        }

        public Dictionary<string, decimal> GetBonuses(ClientBonusFilter filter = ClientBonusFilter.All, int nodeId = 0)
        {
            UniversalAssert.IsNotNull(_currentConflictState, "Current conflict is null. Call init first");
            UniversalAssert.IsNotNull(_playerState, "Player state is null. Call init first");
            var teamDonations = (filter & ClientBonusFilter.TeamDonations) != 0;
            var playerDonations = (filter & ClientBonusFilter.PlayerDonations) != 0;
            var teamPoi = (filter & ClientBonusFilter.TeamInterests) != 0;
            var emenyTeamPoi = (filter & ClientBonusFilter.EnemyInterests) != 0;
            var myTeam = _currentConflictState.TeamsStates.First(_ => _.Id == _playerState.TeamId);
            var allBonuses = myTeam.DonationBonuses.Where(_ => !string.IsNullOrEmpty(_.ClientType)).Select(_ => new {type = _.ClientType, value = _.Value}).ToList();
            if(!teamDonations)
                allBonuses.Clear();
            if (playerDonations)
            {
                var playerDonationBonuses = _playerState.DonationBonuses.Where(_ => !string.IsNullOrEmpty(_.ClientType)).Select(_ => new {type = _.ClientType, value = _.Value});
                allBonuses = allBonuses.Union(playerDonationBonuses).ToList();
            }

            if(nodeId > 0 && (teamPoi || emenyTeamPoi))
            {
                UniversalAssert.IsTrue(_myPointsOfInterests != null && _enemyPointsOfInterests != null, "Points of interest not loaded. Call init first");
                if (teamPoi)
                {
                    var teamPointOfInterest =
                        _myPointsOfInterests
                            .Where(_ => _.BonusExpiryDate > DateTime.UtcNow &&
                                        _.NodeId == nodeId)
                            .SelectMany(_ => _.Bonuses)
                            .Select(_ => new {type = _.ClientType, value = _.Value});
                    allBonuses = allBonuses.Union(teamPointOfInterest).ToList();
                }

                if (emenyTeamPoi)
                {
                    var enemyPointOfInterest = 
                        _enemyPointsOfInterests
                            .Where(_ => _.BonusExpiryDate > DateTime.UtcNow &&
                                        _.NodeId == nodeId)
                            .SelectMany(_ => _.Bonuses)
                            .Select(_ => new { type = _.ClientType, value = _.Value });
                    allBonuses = allBonuses.Union(enemyPointOfInterest).ToList();
                }
            }

            return allBonuses.GroupBy(_ => _.type).ToDictionary(_ => _.Key, _ => _.Sum(p => p.value));
        }

        private ConflictResult[] GetUnclaimedResults()
        {
            return State.ConflictResults.Where(_ => !State.ClaimedRewards.Contains(_.ConflictId)).Select(_ => _.Clone()).ToArray();
        }

        private void UpdateResults(Action<ConflictResult> callback)
        {
            var regResults = State.ConflictResults.Select(_ => _.ConflictId).ToList();
            regResults.Sort();

            var conflictsToUpdate = State.VisitedConflicts.Where(_ => regResults.BinarySearch(_)< 0).ToArray();

            if (conflictsToUpdate.Length < 1)
            {
                return;
            }

            for (var i = 0; i < conflictsToUpdate.Length; ++i)
            {
                _api.ConflictResultsApi.GetResult(conflictsToUpdate[i], r =>
                {
                    if (r == null)
                    {
                        callback(null);
                        return;
                    }

                    _api.LeaderboardsApi.GetWinPointsTopMyPosition(PlayerId, true, _currentConflictState.Id, place =>
                    {
                        r.MyPlace = (int) place;
                        r.MyTeam = _playerState.TeamId;
                        r.MySector = (int) (place / ((decimal) _currentConflictState.PrizePlaces /
                                                     _currentConflictState.PrizesCount));
                        callback(r);
                    });
                });
            }
        }

        
        protected virtual void SelectTeamToJoin(Action<string> callback)
        {
            GetCurrentConflictState(() =>
            {
                UniversalAssert.IsNotNull(_currentConflictState, "No conflicts running");
                if (_currentConflictState.AssignType != TeamAssignType.BasicAuto)
                {
                    return;
                }
                _api.PlayersApi.GetTeamPlayersStat(_currentConflictState.Id, stat =>
                {
                    var m = stat.PlayersCount.Min();
                    var variants = stat.PlayersCount.Select((o, i) => new {idx = i, val = o}).Where(_ => _.val == m).ToArray();
                    var teamIdx = variants.First().idx;
                    var team = stat.Teams[teamIdx];
                    callback(team);
                });
            });
        }

        public void Register(Action callback)
        {
            SelectTeamToJoin(t =>
            {
                Register(t, callback);
            });
        }

        public void Register(string team, Action callback)
        {
            GetCurrentConflictState(() =>
            {
                UniversalAssert.IsNotNull(_currentConflictState, "No conflicts running");
                if(_currentConflictState == null)
                    return;
                _api.PlayersApi.Register(_currentConflictState.Id, PlayerId, team, s =>
                {
                    _playerState = s;
                    PlayerStateUpdated();
                });
            });
        }

        public class StageInfo : StageDef
        {
            public DateTime Start;
            public DateTime End;
        }
    }
}
