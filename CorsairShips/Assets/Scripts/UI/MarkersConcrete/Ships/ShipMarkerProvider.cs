using Game.SeaGameplay;
using UnityDI;
using UnityEngine;

namespace UI.Markers.Units {
	public class ShipMarkerProvider : MarkerProvider<ShipMarkerWidget, ShipMarkerData> {

		[SerializeField] private Color _AllyProgressbarColor;
		[SerializeField] private Color _EnemyProgressbarColor;
		
		private Ship _Ship;

		private void Awake() {
			ContainerHolder.Container.BuildUp(this);
			_Ship = GetComponentInParent<Ship>();
		}
		
		public override bool GetVisibility() {
			if (_Ship == null)
				return false;
			return _Ship.IsAlive;
		}

		protected override void RefreshData(ShipMarkerData data) {
			base.RefreshData(data);
			data.NormalizedHealth = _Ship.NormalizedHealth;
			data.IsEnemy = !_Ship.IsLocalPlayerShip;
			data.ProgressbarColor = data.IsEnemy ? _EnemyProgressbarColor : _AllyProgressbarColor;
			data.Nickname = _Ship.ShipData.Nickname;
		}
		
	}
}
