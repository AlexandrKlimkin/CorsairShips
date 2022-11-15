using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using S;
using ServerShared.GlobalConflict;
using UnityEngine;
using ServerShared;
using UnityDI;

namespace GlobalConflictModule.Scripts
{
    [ExecuteInEditMode]
    public class GlobalConflictDesc : MonoBehaviour
    {
        [SerializeField] private PlacableNodeDesc _nodePrefab;
        [SerializeField] private DonationLevelDesc _donationLevelPrefab;
        [SerializeField] private PointOfInterestsDesc _poiPrefab;
        [SerializeField] private NodeQuestDesc _questPrefab;
        private GameObject _nodeSelector;
        private int _nodeId;

        [Header("Main")] public string Id;
        public string MapId;
        public string[] TeamIds;
        public TeamAssignType AssignType;
        public int GeneralsCount;

        [Header("Stages")] public bool HasDonationStage;
        public int DonationStageLengthInMinutes;
        public int DonationStageCooldownLengthInMinutes;
        public int BattleStageLengthInMinutes;
        public int BattleStageCooldownLengthInMinutes;
        public int FinalStageLengthInMinutes;
        public int FinalStageCooldownLengthInMinutes;

        [Header("Battle Stage")] public int CaptureTime;
        public int BattleCost;

        [Header("Point of interest")]
        public int MaxPointsOfInterestAtNode = 1;
        public int MaxSameTypePointsOfInterestAtNode = 1;

        [Header("Prizes")] [Tooltip("топ игроков, которые получают призы")]
        public int PrizePlaces = 100;

        [Tooltip("градация призов по секторам PrizePlaces / PrizesCount (в базовом варианте 20 мест)")]
        public int PrizesCount = 5;

        private GlobalConflictState _globalConflictState = new GlobalConflictState() {Map = new MapState()};
        private List<DonationLevelDesc> _donationLevels = new List<DonationLevelDesc>();
        private List<PointOfInterestsDesc> _pois = new List<PointOfInterestsDesc>();
        private List<PlacableNodeDesc> _nodes = new List<PlacableNodeDesc>();
        private List<NodeQuestDesc> _quests = new List<NodeQuestDesc>();

        private bool _notEmpty;

        public bool ListsDestroyed
        {
            get { return !_notEmpty; }
        }

        void Start()
        {
            if(!Application.isPlaying)
                return;
            
            ContainerHolder.Container.BuildUp(this);
            RefreshLists();
        }

        public PlacableNodeDesc AddNode()
        {
            if (_nodeSelector != null)
            {
                Destroy(_nodeSelector);
                _nodeSelector = null;
            }

            if (_globalConflictState.Map == null)
                _globalConflictState.Map = new MapState() {Nodes = new NodeState[] { }, TextureId = "test"};

            var o = Instantiate(_nodePrefab, transform);
            o.NodeId = ++_nodeId;
            o.NodePlaced += v => _nodeSelector = null;

            return o;
        }

        public PlacableNodeDesc FindNode(int nodeId)
        {
            var result = _nodes.FirstOrDefault(_ => _.NodeId == nodeId);
            if(result == null)
                Debug.Log(nodeId + " not found");
            return result;
        }

        public DonationLevelDesc AddDonationLevel()
        {
            return Instantiate(_donationLevelPrefab, transform);
        }

        public PointOfInterestsDesc AddPointOfInterest()
        {
            return Instantiate(_poiPrefab, transform);
        }

        public NodeQuestDesc AddQuest()
        {
            return Instantiate(_questPrefab, transform);
        }

        private void GenerateDonations()
        {
            var levels = new List<DonationBonusLevels>();
            var bonuses = new List<DonationBonus>();
            for (var i = 0; i < _donationLevels.Count; ++i)
            {
                var t = _donationLevels[i];
                var l = new DonationBonusLevels()
                {
                    Level = t.Level,
                    Team = t.IsTeam,
                    Threshold = t.DonationsAmount
                };
                levels.Add(l);
                for (var b = 0; b < t.Bonuses.Length; ++b)
                {
                    var bonus = new DonationBonus()
                    {
                        ClientType = t.Bonuses[b].ClientType,
                        ServerType = t.Bonuses[b].ServerType,
                        Level = t.Level,
                        Team = t.IsTeam,
                        Value = (decimal) t.Bonuses[b].BonusValue
                    };
                    bonuses.Add(bonus);
                }
            }

            _globalConflictState.DonationBonusLevels = levels.ToArray();
            _globalConflictState.DonationBonuses = bonuses.ToArray();
        }

        private void GeneratePointOfInterest()
        {
            var pois = new List<PointOfInterest>();

            for (var i = 0; i < _pois.Count; ++i)
            {
                var p = _pois[i];
                var bonuses = new List<PointOfInterestBonus>();
                if (p.Bonuses != null)
                    for (int j = 0; j < p.Bonuses.Length; j++)
                    {
                        var bDesc = p.Bonuses[j];
                        var b = new PointOfInterestBonus
                        {
                            ClientType = bDesc.ClientType,
                            ServerType = bDesc.ServerType,
                            Value = (decimal) bDesc.Value
                        };
                        bonuses.Add(b);
                    }

                string poiIdBase;
                if (!string.IsNullOrEmpty(p.Name))
                    poiIdBase = p.Name;
                else
                    poiIdBase = p.name;
                var poiId = poiIdBase;
                var fix = 0;

                while (pois.Any(_ => _.Id == poiId))
                {
                    poiId = poiIdBase + "_" + fix;
                }

                var poi = new PointOfInterest
                {
                    Id = poiId,
                    Auto = p.AutoDeploy,
                    OwnerTeam = p.ForAllTeams ? "" : p.Team,
                    GeneralLevel = p.GeneralLevel,
                    BonusTime = TimeSpan.FromMinutes(p.BonusTimeInMinutes),
                    DeployCooldown = TimeSpan.FromMinutes(p.DeployCooldownInMinutes),
                    Type = p.Type,
                    Bonuses = bonuses.ToArray(),
                };
                pois.Add(poi);
            }

            _globalConflictState.PointsOfInterest = pois.ToArray();
        }

        public void GenerateQuests()
        {
            var quests = new List<NodeQuest>();

            for (var i = 0; i < _quests.Count; ++i)
            {
                var q = _quests[i];

                string questIdBase;
                if (!string.IsNullOrEmpty(q.Name))
                    questIdBase = q.Name;
                else
                    questIdBase = q.name;
                var questId = questIdBase;
                var fix = 0;

                while (quests.Any(_ => _.Id == questId))
                {
                    questId = questIdBase + "_" + fix;
                }

                var nodeQuest = new NodeQuest
                {
                    Id = questId,
                    QuestLevel = q.Level,
                    Weight = q.Weight,
                    ClientType = q.Type,
                    Auto = q.Auto,
                    RewardId = q.RewardId,
                    ActiveTime = TimeSpan.FromMinutes(q.ActiveTimeInMinutes),
                    DeployCooldown = TimeSpan.FromMinutes(q.DeployCooldownInMinutes)
                };
                quests.Add(nodeQuest);
            }

            _globalConflictState.Quests = quests.ToArray();
        }

        public GlobalConflictState GenerateState()
        {
            _globalConflictState.Id = Id;
            _globalConflictState.Map.TextureId = Id;
            _globalConflictState.AssignType = AssignType;
            _globalConflictState.GeneralsCount = GeneralsCount;
            _globalConflictState.BattleCost = BattleCost;
            _globalConflictState.CaptureTime = CaptureTime;
            _globalConflictState.MaxPointsOfInterestAtNode = MaxPointsOfInterestAtNode;
            _globalConflictState.MaxSameTypePointsOfInterestAtNode = MaxSameTypePointsOfInterestAtNode;
            _globalConflictState.PrizePlaces = PrizePlaces;
            _globalConflictState.PrizesCount = PrizesCount;
            GenerateDonations();
            GeneratePointOfInterest();
            GenerateQuests();

            var stages = new List<StageDef>();
            if (HasDonationStage)
            {
                stages.Add(new StageDef()
                {
                    Id = StageType.Donation,
                    Period = TimeSpan.FromMinutes(DonationStageLengthInMinutes)
                });
                if (DonationStageCooldownLengthInMinutes > 0)
                {
                    stages.Add(new StageDef()
                    {
                        Id = StageType.Cooldown,
                        Period = TimeSpan.FromMinutes(DonationStageCooldownLengthInMinutes)
                    });
                }
            }

            stages.Add(new StageDef()
            {
                Id = StageType.Battle,
                Period = TimeSpan.FromMinutes(BattleStageLengthInMinutes)
            });
            if (BattleStageCooldownLengthInMinutes > 0)
            {
                stages.Add(new StageDef()
                {
                    Id = StageType.Cooldown,
                    Period = TimeSpan.FromMinutes(BattleStageCooldownLengthInMinutes)
                });
            }

            stages.Add(new StageDef()
            {
                Id = StageType.Final,
                Period = TimeSpan.FromMinutes(FinalStageLengthInMinutes)
            });
            if (FinalStageCooldownLengthInMinutes > 0)
            {
                stages.Add(new StageDef()
                {
                    Id = StageType.Cooldown,
                    Period = TimeSpan.FromMinutes(FinalStageCooldownLengthInMinutes)
                });
            }

            _globalConflictState.Stages = stages.ToArray();
            _globalConflictState.Teams = TeamIds;
            return _globalConflictState;
        }

        void RefreshLists()
        {
            _donationLevels.Clear();
            _pois.Clear();
            _nodes.Clear();
            _quests.Clear();
            foreach (Transform o in transform)
            {
                var pn = o.gameObject.GetComponent<PlacableNodeDesc>();
                var don = o.gameObject.GetComponent<DonationLevelDesc>();
                var poi = o.gameObject.GetComponent<PointOfInterestsDesc>();
                var quest = o.gameObject.GetComponent<NodeQuestDesc>();

                if (don != null)
                    _donationLevels.Add(don);
                if (poi != null)
                    _pois.Add(poi);
                if (quest != null)
                    _quests.Add(quest);
                if(pn != null)
                    _nodes.Add(pn);
            }
        }

        void Update()
        {

            if (!Application.isPlaying)
            {
                RefreshLists();
                _notEmpty = true;
            }

            var ids = new List<int>();
            foreach (var node in _nodes)
            {
                if (ids.Contains(node.NodeId))
                {
                    Debug.Log("Node with ID already exists. New id is " + _nodeId + 1);
                    node.NodeId = ++_nodeId;
                }

                if (_globalConflictState.Map.Nodes.All(_ => _.Id != node.NodeId))
                {
                    _globalConflictState.Map.Nodes =
                        _globalConflictState.Map.Nodes.Union(new[] { node.NodeState }).ToArray();
                }
                else
                {
                    var idx = _globalConflictState.Map.Nodes.IndexOf(_ => _.Id == node.NodeId);
                    if (!ReferenceEquals(_globalConflictState.Map.Nodes[idx], node.NodeState))
                        _globalConflictState.Map.Nodes[idx] = node.NodeState;
                }

                ids.Add(node.NodeId);
            }

            var teamDonationLevels = _donationLevels.Where(_ => _.IsTeam && _.DonationsAmount > 0)
                .OrderBy(_ => _.DonationsAmount).ToArray();
            var playerDonationLevels = _donationLevels.Where(_ => !_.IsTeam && _.DonationsAmount > 0)
                .OrderBy(_ => _.DonationsAmount).ToArray();

            for (var i = 0; i < teamDonationLevels.Length; ++i)
            {
                var d = teamDonationLevels[i];
                d.Level = i + 1;
                var name = "TeamDonation#lvl" + d.Level;
                if (d.gameObject.name != name)
                    d.gameObject.name = name;
            }

            for (var i = 0; i < playerDonationLevels.Length; ++i)
            {
                var d = playerDonationLevels[i];
                d.Level = i + 1;
                var name = "Donation#lvl" + d.Level;
                if (d.gameObject.name != name)
                    d.gameObject.name = name;
            }

            for (var i = 0; i < _pois.Count; i++)
            {
                var p = _pois[i];
                var name = string.IsNullOrEmpty(p.Name) ? "POI#" + (i + 1) : p.Name;
                if (p.name != name)
                    p.name = name;
            }

            for (int i = 0; i < _quests.Count; i++)
            {
                var q = _quests[i];
                var name = string.IsNullOrEmpty(q.Name) ? "Quest#" + (i + 1) : q.Name;
                if (q.name != name)
                    q.name = name;
            }

            if (_globalConflictState.Map != null)
            {
                var nodesList = _globalConflictState.Map.Nodes.ToList();
                var removed = nodesList.RemoveAll(_ => !ids.Contains(_.Id));
                if (removed > 0)
                {
                    _globalConflictState.Map.Nodes = nodesList.ToArray();
                }
            }
        }

        public void Reset()
        {
            _nodeId = 0;
        }

        private static StageDef GetStageCooldown(GlobalConflictState state, int stageIdx)
        {
            var cooldownStageIdx = stageIdx + 1;
            if (state.Stages.Length <= cooldownStageIdx || state.Stages[cooldownStageIdx].Id != StageType.Cooldown)
                return null;
            return state.Stages[cooldownStageIdx];
        }

        public void Restore(GlobalConflictState state)
        {
            Reset();
            foreach (var t in transform.Cast<Transform>().ToArray())
            {
                DestroyImmediate(t.gameObject);
            }

            Id = state.Id;
            MapId = state.Map != null ? state.Map.TextureId : "";
            TeamIds = state.Teams;
            AssignType = state.AssignType;
            GeneralsCount = state.GeneralsCount;

            var donationStageIdx = state.Stages.IndexOf(_ => _.Id == StageType.Donation);
            var battleStageIdx = state.Stages.IndexOf(_ => _.Id == StageType.Battle);
            var finalStageIdx = state.Stages.IndexOf(_ => _.Id == StageType.Final);
            var donationStage = state.Stages != null && donationStageIdx> -1 ? state.Stages[donationStageIdx] : null;
            var battleStage = state.Stages != null && battleStageIdx > -1 ? state.Stages[battleStageIdx] : null;
            var finalStage = state.Stages != null && finalStageIdx > -1 ? state.Stages[finalStageIdx] : null;

            if (donationStage != null)
            {
                HasDonationStage = true;
                var donationCooldownStage = GetStageCooldown(state, donationStageIdx);
                DonationStageLengthInMinutes = (int) donationStage.Period.TotalMinutes;
                DonationStageCooldownLengthInMinutes = donationCooldownStage != null ? (int)donationCooldownStage.Period.TotalMinutes : 0;
            }

            if (battleStage != null)
            {
                var battleCooldownStage = GetStageCooldown(state, battleStageIdx);
                BattleStageLengthInMinutes = (int) battleStage.Period.TotalMinutes;
                BattleStageCooldownLengthInMinutes = battleCooldownStage != null ? (int)battleCooldownStage.Period.TotalMinutes : 0;
            }

            if (finalStage != null)
            {
                var finalCooldownStage = GetStageCooldown(state, finalStageIdx);
                FinalStageLengthInMinutes = (int) finalStage.Period.TotalMinutes;
                FinalStageCooldownLengthInMinutes = finalCooldownStage != null ? (int)finalCooldownStage.Period.TotalMinutes : 0;
            }

            foreach (var node in state.Map.Nodes.OrderBy(_ => _.Id))
            {
                var n = AddNode();
                n.NodeState = node;
                n.NodeId = node.Id;
                n.UpdatePosition();
                _nodeId = Math.Max(node.Id, _nodeId);
            }

            if (state.DonationBonusLevels != null)
            {
                foreach (var donation in state.DonationBonusLevels)
                {
                    var desc = AddDonationLevel();
                    desc.Level = donation.Level;
                    desc.DonationsAmount = donation.Threshold;
                    desc.IsTeam = donation.Team;
                    var bonuses = new List<DonationLevelBonusDesc>();
                    if (state.DonationBonuses != null)
                    {
                        foreach (var stateDonationBonus in state.DonationBonuses.Where(_ => _.Team == donation.Team && _.Level == donation.Level))
                        {
                            bonuses.Add(new DonationLevelBonusDesc()
                            {
                                ClientType = stateDonationBonus.ClientType,
                                ServerType = stateDonationBonus.ServerType,
                                BonusValue = (float)stateDonationBonus.Value
                            });
                        }
                    }

                    desc.Bonuses = bonuses.ToArray();
                }
            }

            if (state.PointsOfInterest != null)
            {
                foreach (var pointOfInterest in state.PointsOfInterest)
                {
                    var poi = AddPointOfInterest();
                    poi.Team = pointOfInterest.OwnerTeam;
                    poi.AutoDeploy = pointOfInterest.Auto;
                    poi.BonusTimeInMinutes = (int) pointOfInterest.BonusTime.TotalMinutes;
                    poi.DeployCooldownInMinutes = (int) pointOfInterest.DeployCooldown.TotalMinutes;
                    poi.ForAllTeams = string.IsNullOrEmpty(poi.Team);
                    poi.GeneralLevel = pointOfInterest.GeneralLevel;
                    poi.Name = pointOfInterest.Id;
                    poi.Type = pointOfInterest.Type;
                    var bonuses = new List<PointOfInterestBonusDesc>();
                    foreach (var bonus in pointOfInterest.Bonuses)
                    {
                        var b = new PointOfInterestBonusDesc
                        {
                            ServerType = bonus.ServerType,
                            ClientType = bonus.ClientType,
                            Value = (float) bonus.Value
                        };
                        bonuses.Add(b);
                    }
                    poi.Bonuses = bonuses.ToArray();
                }
            }

            if (state.Quests != null)
            {
                foreach (var stateQuest in state.Quests)
                {
                    var quest = AddQuest();
                    quest.Weight = stateQuest.Weight;
                    quest.Type = stateQuest.ClientType;
                    quest.RewardId = stateQuest.RewardId;
                    quest.Name = stateQuest.Id;
                    quest.Level = stateQuest.QuestLevel;
                    quest.DeployCooldownInMinutes = (int)stateQuest.DeployCooldown.TotalMinutes;
                    quest.Auto = stateQuest.Auto;
                    quest.ActiveTimeInMinutes = (int) stateQuest.ActiveTime.TotalMinutes;
                }
            }
        }
    }
}