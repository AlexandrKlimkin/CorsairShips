using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GlobalConflictModule.Scripts;
using GlobalConflictModule.Sources.Scripts;
using Newtonsoft.Json;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using S;
using ServerShared.GlobalConflict;
using ServerShared.Sources.GlobalConflict;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace GlobalConflict.Example
{
    public class GlobalConflictExamplePlayer : MonoBehaviour
    {
        [Dependency]
        private GlobalConflictModuleBase _globalConflict;
        [SerializeField]
        private UiNodeController _nodePrefab;

        private List<Vector3> _lines = new List<Vector3>();

        private bool _initialized;
        private float _viewWidth;
        private float _viewHeight;


        void Awake()
        {
        }

        IEnumerator Start()
        {
            while (!GlobConflictInitializer.Initialized)
            {
                yield return new WaitForSeconds(0.5f);
            }
            ContainerHolder.Container.BuildUp(this);

            _globalConflict.Initalized += OnConflictInitDone;
            _globalConflict.OnConflictResultAvailable.Subscribe(ConflictResult);
            _globalConflict.OnQuestCompleted.Subscribe(QuestCompleted);
        }

        private void QuestCompleted(GlobalConflictDeployedQuest quest)
        {
            Debug.Log(quest.QuestId + " completed");
        }

        private void ConflictResult(ConflictResult conflictResult)
        {
            Debug.Log("ConflictResult: " + JsonConvert.SerializeObject(conflictResult));
            var cmdGoBattle = new GlobalConflictModuleBase_ClaimReward()
            {
                result = conflictResult
            };
            CommandProcessor.Process<object, GlobalConflictModuleBase_ClaimReward>(cmdGoBattle);
        }

        // Update is called once per frame
        void Update()
        {
            if (!_initialized && _globalConflict != null)
            {
                _globalConflict.Init();
                _initialized = true;
            }

            if (Math.Abs(_viewHeight - Screen.height) > 0.01 || Math.Abs(_viewWidth - Screen.width) > 0.01)
            {
                var nodes = GetComponentsInChildren<UiNodeController>();
                foreach (var uiNodeController in nodes)
                {
                    PlaceNode(uiNodeController);
                }

                _viewHeight = Screen.height;
                _viewWidth = Screen.width;
            }

            for (var i = 0; i < _lines.Count; i+=2)
            {
                Debug.DrawLine(_lines[i], _lines[i + 1], Color.white, 1);
            }
        }

        private void CleanNodes()
        {
            foreach (Transform t in transform)
            {
                Destroy(t.gameObject);
            }
        }

        private GlobalConflictDeployedQuest RefreshQuestInfo(GlobalConflictDeployedQuest quest)
        {
            var cmd = new GlobalConflictModuleBase_UpdateAndGetQuests();
            var quests = CommandProcessor.Process<GlobalConflictDeployedQuest[], GlobalConflictModuleBase_UpdateAndGetQuests>(cmd);

            quest = quests.FirstOrDefault(_ => _.QuestId == quest.QuestId);
            return quest;
        }

        private void PlaceNode(UiNodeController node)
        {
            var rt = GetComponent<RectTransform>();
            var image = GetComponent<Image>();
            var sz = PlacableNodeDesc.GetApectSize(rt, image);
            var hSz = sz / 2;
            var origin = new Vector2(transform.position.x, transform.position.y);
            var nodeState = node.NodeState;
            var p = origin + new Vector2(nodeState.PositionX * hSz.x, nodeState.PositionY * hSz.y);
            node.gameObject.transform.position = p;
        }

        private void OnConflictInitDone()
        {
            var cmd = new S.GlobalConflictModuleBase_Update();
            CommandProcessor.Process<object, S.GlobalConflictModuleBase_Update>(cmd);

            Debug.Log("Conflict init done. IamIn=" + _globalConflict.IamInConflict + " HasActiveConflict=" + _globalConflict.HasActiveConflict);

            if(_globalConflict.ConflictState == null)
                return;
            var teamId = "";
            if (_globalConflict.PlayerState != null)
            {
                teamId = _globalConflict.PlayerState.TeamId;

                // Чтобы увидеть в лидбордах имя игрока, нужно установить его через GlobalConflictClient
                // Дело в том что из коробки в Backend нет такого понятия как имя игрока, каждая игра реализиует по-своему
                if (string.IsNullOrEmpty(_globalConflict.PlayerState.Name))
                {
                    var name = "Player#" + _globalConflict.PlayerState.Id.Substring(0, 4);
                    _globalConflict.SetPlayerName(name, () => Debug.Log("Player name: " + name));
                }
            }
            var reachableNodes = _globalConflict.ConflictState.GetReachableNodes(teamId, true).ToArray();

            CleanNodes();

            var cmdGetQuests = new GlobalConflictModuleBase_UpdateAndGetQuests();
            var quests = CommandProcessor.Process<GlobalConflictDeployedQuest[], GlobalConflictModuleBase_UpdateAndGetQuests>(cmdGetQuests);

            var generalPoi = _globalConflict.GetGeneralPointOfInterest();
            var deployedPois = _globalConflict.GetDeployedPointOfInterests();
            var otherPois = _globalConflict.GetDeployablePointsOfInterests();
            var otherPoisTypes = otherPois.Select(_ => _.Type).Distinct().ToArray();
            var generalPoiDeployable = generalPoi != null && generalPoi.IsDeployable(_globalConflict.PlayerState);
            var nodes = new Dictionary<int, UiNodeController>();
            var stage = _globalConflict.GetCurrentStage();
            foreach (var nodeState in _globalConflict.ConflictState.Map.Nodes)
            {
                var o = Instantiate(_nodePrefab, transform);
                var reachable = reachableNodes.Any(_ => _.Id == nodeState.Id);
                o.name = "Node#" + nodeState.Id;
                o.NodeState = nodeState;
                o.SetReachable(reachable);
                o.AttackWin += OnAttackWin;
                o.AttackLose += OnAttackLose;
                o.DeployGeneralPoi += OnDeployGeneralPoi;
                o.DeployPoi += OnDeployPoi;
                // Точки интереса можно выставлять только на этапе боя
                // клиент должен следить за этим
                if (reachable && stage.Id == StageType.Battle)
                {
                    var poisOnNode = _globalConflict.GetPointsOfInterestOnNode(nodeState.Id);
                    if (poisOnNode.Length <
                        _globalConflict.ConflictState.MaxPointsOfInterestAtNode)
                    {
                        var maxByType = _globalConflict.ConflictState.MaxSameTypePointsOfInterestAtNode;
                        var allowedTypes = otherPoisTypes.Where(_ => poisOnNode.Count(n => n.Type == _) < maxByType).ToArray();
                        var filteredByMax = otherPois.Where(_ => allowedTypes.Contains(_.Type)).ToArray();
                        o.SetDeployablePoi(filteredByMax);
                        if(poisOnNode.Count(_ => _.Type == generalPoi.Type) < _globalConflict.ConflictState.MaxSameTypePointsOfInterestAtNode)
                            o.SetDeployGeneralPoi(generalPoiDeployable);
                    }
                }

                if (nodeState.NodeStatus == NodeStatus.Base)
                {
                    if (nodeState.BaseForTeam == teamId)
                        o.SetAllyNode(true);
                    else
                        o.SetEnemyNode(true);
                }

                if (nodeState.NodeStatus == NodeStatus.Captured)
                {
                    if(nodeState.Owner == teamId)
                        o.SetAllyNode(true);
                    else
                        o.SetEnemyNode(true);
                }

                var quest = quests.FirstOrDefault(_ => _.NodeId == nodeState.Id);
                if (quest != null)
                {
                    o.SetQuest(() => RefreshQuestInfo(quest));
                }

                if (generalPoi != null && generalPoi.IsDeployed(_globalConflict.CommandTimestamp, nodeState.Id))
                    o.SetTeamPoi(generalPoi);
                foreach (var pointOfInterest in deployedPois)
                {
                    if (pointOfInterest.IsDeployed(_globalConflict.CommandTimestamp, nodeState.Id))
                        o.SetTeamPoi(pointOfInterest);
                }


                PlaceNode(o);

                nodes[nodeState.Id] = o;
            }

            foreach (var kv in nodes)
            {
                var n = _globalConflict.ConflictState.Map.Nodes.FirstOrDefault(_ => _.Id == kv.Key);
                foreach (var linkedNode in n.LinkedNodes)
                {
                    UiNodeController o;
                    if(!nodes.TryGetValue(linkedNode, out o))
                        continue;
                    _lines.Add(kv.Value.transform.position);
                    _lines.Add(o.transform.position);
                }
            }

            if(!_globalConflict.IamInConflict)
                Debug.Log("Please join conflict to proceed!");
        }

        // Ручной метод выставления точек интереса из клиента
        // На проде рекомендуем использовать серверную логику автоматической расстановки точек интереса (реализауйте интерфейс IPointOfInterestNodePicker на сервере)
        private void OnDeployPoi(int nodeId, PointOfInterest poi)
        {
            _globalConflict.DeployPointOfInterest(poi, nodeId, b => Debug.Log("Deploy POI " + poi.Id + " result: " + b));
        }

        private void OnDeployGeneralPoi(int nodeId)
        {
            // Пытается выставить генеральскую точку интереса в указанную ноду
            // Чтобы выставить генеральскую точку необходимо:
            // * текущий игрок является генералом соответсвующего уровня
            // * мы находимся на этапе боя (т.е. не на этапе доната, завершения конфликта или кулдауна)
            // * точка еще не выставлена, либо ее NextDeploy уже наступил (см. PointOfInterest.IsDeployable())
            // * нода (nodeId) недоступна для команды (не соседняя от захваченных нод)
            // * другие условия см. PointOfInterestServer.DeployPointOfInterestAsync()
            var poi = _globalConflict.GetGeneralPointOfInterest();
            _globalConflict.DeployPointOfInterest(poi, nodeId, b => Debug.Log("General point deploy result: " + b));
        }

        private void OnAttackLose(int nodeId)
        {
            var stage = _globalConflict.GetCurrentStage();
            var allowed = stage.Id == StageType.Battle;
            if (!allowed)
            {
                Debug.LogWarning("Cant attack in '" + stage.Id + "' stage");
                return;
            }

            var cmdGoBattle = new GlobalConflictModuleBase_BattleForNode()
            {
                nodeId = nodeId
            };
            var canBattle = CommandProcessor.Process<bool, GlobalConflictModuleBase_BattleForNode>(cmdGoBattle);

            if (!canBattle)
            {
                var waitTime = _globalConflict.TimeToAttackNode(nodeId);
                Debug.LogWarning("Not enough energy. Wait " + waitTime + " to restore needed energy");
                return;
            }

            _globalConflict.BattleForNodeResult(nodeId, false);
            _globalConflict.Init();
        }

        private void OnAttackWin(int nodeId)
        {
            var stage = _globalConflict.GetCurrentStage();
            var allowed = stage.Id == StageType.Battle;
            if (!allowed)
            {
                Debug.LogWarning("Cant attack in '" + stage.Id + "' stage");
                return;
            }

            var cmdGoBattle = new GlobalConflictModuleBase_BattleForNode()
            {
                nodeId = nodeId
            };
            var canBattle = CommandProcessor.Process<bool, GlobalConflictModuleBase_BattleForNode>(cmdGoBattle);

            if (!canBattle)
            {
                var waitTime = _globalConflict.TimeToAttackNode(nodeId);
                Debug.LogWarning("Not enough energy. Wait " + waitTime + " to restore needed energy");
                return;
            }

            _globalConflict.BattleForNodeResult(nodeId, true);
            _globalConflict.Init();
        }
    }
}
