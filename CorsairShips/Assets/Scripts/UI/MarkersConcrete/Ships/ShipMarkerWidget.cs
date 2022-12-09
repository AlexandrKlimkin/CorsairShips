using System;
using UI.Widgets;
using UnityEngine;

namespace UI.Markers.Units {
	public class ShipMarkerWidget : MarkerWidget<ShipMarkerData> {
		[SerializeField]
		private BarWidget _BarWidget;

		private float _LastHealthValue;

		protected override void HandleData(ShipMarkerData data) {
			_BarWidget.SetProgressBarColor(data.ProgressbarColor);
			if (Math.Abs(data.NormalizedHealth - _LastHealthValue) > float.Epsilon) {
				_BarWidget.Display(data.NormalizedHealth);
			}
			_LastHealthValue = data.NormalizedHealth;
		}
	}
}
