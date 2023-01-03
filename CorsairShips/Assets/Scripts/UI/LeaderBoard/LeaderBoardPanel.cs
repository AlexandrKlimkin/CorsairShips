using System.Linq;
using Game;
using Game.SeaGameplay;
using Game.SeaGameplay.Data;
using Game.SeaGameplay.Points;
using UnityDI;
using UnityEngine;

namespace UI.Battle.LeaderBoard {
    public class LeaderBoardPanel : MonoBehaviour {
        [Dependency]
        private readonly IPointsCounter _PointsCounter;
        [Dependency]
        private readonly ILocalShipProvider _LocalShipProvider;

        [SerializeField]
        private LeaderBoardWidget _RecordPrefab;
        [SerializeField]
        private RectTransform _RecordsHost;
        
        private MonoBehaviourPool<LeaderBoardWidget> _Pool;

        private void Awake() {
            _Pool = new MonoBehaviourPool<LeaderBoardWidget>(_RecordPrefab, _RecordsHost);
            ContainerHolder.Container.BuildUp(this);
        }

        private void OnEnable() {
            Refresh();
            _PointsCounter.OnPointsChanged += PointsCounterOnOnPointsChanged;
        }

        private void OnDisable() {
            _PointsCounter.OnPointsChanged -= PointsCounterOnOnPointsChanged;
        }

        private void Refresh() {
            _Pool.ReturnAllToPool();
            foreach (var ship in Ship.Ships) {
                var widget = _Pool.GetObject();
                SetupWidget(widget, ship.ShipData);
            }
            RefreshPlaces();
        }
        
        private void PointsCounterOnOnPointsChanged(byte playerId, int points) {
            var widget = _Pool.ActiveObjects.FirstOrDefault(_ => _.PlayerId == playerId);
            var ship = Ship.Ships.FirstOrDefault(_ => _.ShipData.ShipId == playerId);
            if(ship == null)
                return;
            SetupWidget(widget, ship.ShipData);
            RefreshPlaces();
        }
        
        private void SetupWidget(LeaderBoardWidget widget, ShipData shipData) {
            var isLocalPlayer = _LocalShipProvider.LocalShip && _LocalShipProvider.LocalShip.ShipData.ShipId == shipData.ShipId;
            widget.Setup(shipData.Nickname, _PointsCounter.PointsDict[shipData.ShipId].ToString(), isLocalPlayer, shipData.ShipId);
        }

        private void RefreshPlaces() {
            var widgets = _Pool.ActiveObjects.OrderByDescending(_ => _PointsCounter.PointsDict[_.PlayerId]).ToArray();
            for (var i = 0; i < widgets.Length; i++) {
                var place = i + 1;
                widgets[i].RefreshPlace(place);
                widgets[i].transform.SetSiblingIndex(i);
            }
        }
    }
}
