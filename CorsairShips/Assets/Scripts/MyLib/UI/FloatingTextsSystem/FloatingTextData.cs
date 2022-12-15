using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.FloatingTexts {
	[Serializable]
	public struct FloatingTextData {
		[HideInInspector]
		public string Text;
		public Color TextColor;
		public float TextSize;
		[Space]
		public FloatingWidgetMovementType MovementType;
		[HideInInspector]
		public Vector3 FromPos;
		[HideInInspector]
		public Vector3 ToPos;
		[ShowIf(@"DistanceType")]
		public float FlyDistance;
		public float FlyTime;
		public AnimationCurve FlyCurve;
		[Space]
		public float StartFadeTime;
		public float FadeTime;
		[Space]
		public Vector2 Size;

		private bool DistanceType => MovementType == FloatingWidgetMovementType.Distance;
	}

	public enum FloatingWidgetMovementType {
		Distance,
		FromPointToPoint,
	}
}