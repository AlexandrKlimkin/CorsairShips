using System;
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
		public float FlyDistance;
		public float FlyTime;
		public AnimationCurve FlyCurve;
		[Space]
		public float StartFadeTime;
		public float FadeTime;
		[Space]
		public Vector2 Size;
	}

	public enum FloatingWidgetMovementType {
		Distance,
		FromPointToPoint,
	}
}