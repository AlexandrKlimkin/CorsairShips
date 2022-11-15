using System;
using Newtonsoft.Json;
using PestelLib.SharedLogic.Modules;
using ServerShared.GlobalConflict;
using UnityEngine;
using UnityEngine.UI;

namespace GlobalConflict.Example
{
    public class UiNodeController : MonoBehaviour
    {
        [SerializeField]
        private Button _attackWin;
        [SerializeField]
        private Button _attackLose;
        [SerializeField]
        private Button _teamPoi;
        [SerializeField]
        private Button _localPoi;
        [SerializeField]
        private GameObject _allyNode;
        [SerializeField]
        private GameObject _enemyNode;
        [SerializeField]
        private Text _teamScorePrefab;
        [SerializeField]
        private GameObject _teamScoreContainer;
        [SerializeField]
        private Button _btnNodeInfo;
        [SerializeField]
        private Button _btnDeployGeneralPoint;
        [SerializeField]
        private Button _poiPrefab;
        [SerializeField]
        private GameObject _poiContainer;
        private PointOfInterest _poi;
        private Func<GlobalConflictDeployedQuest> _quest;

        public NodeState NodeState;

        public bool _scoreInited;

        public event Action<int> AttackWin = i => { };
        public event Action<int> AttackLose = i => { };
        public event Action<int> DeployGeneralPoi = i => { };
        public event Action<int, PointOfInterest> DeployPoi = (i, p) => { };

        void OnAttack(int node, bool win)
        {
            var type = win ? "AttackWin" : "AttackLose";
            // GameMode и GameMap это своего рода рекомендации клиенту, как он их интерпретирует, зависит от конкретной реализации, но
            // подразумевается что GameMode определяет режим игры, а GameMap карту для боя (возможно определять набор занчений в произволном формате, 
            // но клиент должен уметь их понимать)
            var msg = string.Format("{0} Node#{1}. GameMode: '{2}', GameMap: '{3}'", type, node, NodeState.GameMode, NodeState.GameMap);
        }

        // Use this for initialization
        void Start()
        {
            AttackWin += i => OnAttack(i, true);
            AttackLose += i => OnAttack(i, false);
            _teamPoi.onClick.AddListener(OnShowTeamPoiInfo);
            _localPoi.onClick.AddListener(OnShowQuestInfo);
            _attackWin.onClick.AddListener(() => AttackWin(NodeState.Id));
            _attackLose.onClick.AddListener(() => AttackLose(NodeState.Id));
            _btnNodeInfo.onClick.AddListener(OnPrintNodeState);
            _btnDeployGeneralPoint.onClick.AddListener(() => DeployGeneralPoi(NodeState.Id));
        }

        void OnShowQuestInfo()
        {
            var q = _quest();
            Debug.Log(JsonConvert.SerializeObject(q));
        }

        private void OnPrintNodeState()
        {
            Debug.Log(JsonConvert.SerializeObject(NodeState));
        }

        void OnShowTeamPoiInfo()
        {
            Debug.Log(JsonConvert.SerializeObject(_poi));
            var nextDeployOffset = _poi.NextDeploy - DateTime.UtcNow;
            var bonusValidOffset = _poi.BonusExpiryDate - DateTime.UtcNow;
            Debug.Log("Next deploy: " + ((nextDeployOffset <= TimeSpan.Zero) ? "Now" : nextDeployOffset.ToString()));
            Debug.Log("Bonuses expires in: " + ((bonusValidOffset <= TimeSpan.Zero) ? "Now" : bonusValidOffset.ToString()));
        }

        // Update is called once per frame
        void Update()
        {
            if (!_scoreInited)
            {
                _scoreInited = true;
                if (NodeState.TeamPoints == null || NodeState.TeamPoints.Count == 0)
                    return;

                foreach (var kv in NodeState.TeamPoints)
                {
                    var score = Instantiate(_teamScorePrefab, _teamScoreContainer.transform);
                    score.gameObject.SetActive(true);
                    score.text = kv.Key + ": " + kv.Value;
                }
            }

        }

        public void SetReachable(bool value)
        {
            _attackWin.gameObject.SetActive(value);
            _attackLose.gameObject.SetActive(value);
        }

        public void SetTeamPoi(PointOfInterest poi)
        {
            _poi = poi;
            _teamPoi.gameObject.SetActive(poi != null);
        }

        public void SetQuest(Func<GlobalConflictDeployedQuest> quest)
        {
            _quest = quest;
            _localPoi.gameObject.SetActive(quest != null);
        }

        public void SetDeployGeneralPoi(bool value)
        {
            _btnDeployGeneralPoint.gameObject.SetActive(value);
        }

        public void SetDeployablePoi(params PointOfInterest[] pois)
        {
            foreach (Transform t in _poiContainer.transform)
            {
                if(t.gameObject.activeSelf)
                    Destroy(t.gameObject);
            }
            foreach (var pointOfInterest in pois)
            {
                var poiButton = Instantiate(_poiPrefab, _poiContainer.transform);
                var text = poiButton.GetComponentInChildren<Text>();
                text.text = pointOfInterest.Id;
                var interest = pointOfInterest;
                poiButton.gameObject.SetActive(true);
                poiButton.onClick.AddListener(() => DeployPoi(NodeState.Id, interest));
            }
        }

        public void SetAllyNode(bool value)
        {
            _allyNode.gameObject.SetActive(value);
        }

        public void SetEnemyNode(bool value)
        {
            _enemyNode.gameObject.SetActive(value);
        }
    }
}