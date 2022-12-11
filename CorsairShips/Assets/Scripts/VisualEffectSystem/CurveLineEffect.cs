using System;
using System.Collections;
using UnityEngine;

namespace Tools.VisualEffects {
	public class CurveLineEffect : ParticleEffect {
		[SerializeField]
		private LineRenderer Line;
		[SerializeField]
		private AnimationCurve Y_AnimationCurve;
		[SerializeField]
		private int PointsCount;
		[SerializeField]
		private float Y_Mult;

		private Vector3 _StartPoint;
		private Vector3 _EndPoint;

		private bool _NeedUpdate;
		
		public void SetCurve(AnimationCurve curve) {
			Y_AnimationCurve = curve;
		}

		private void Awake() {
			Line.useWorldSpace = true;
		}

		private void Update() {
			if (!_NeedUpdate) 
				return;
			Line.positionCount = PointsCount;
			Line.SetPositions(GetPositions());
			_NeedUpdate = false;
		}
		
		private Vector3[] GetPositions() {
			if (PointsCount < 2)
				return new Vector3[2];
			
			var points = new Vector3[PointsCount];

			var vector = _EndPoint - _StartPoint;

			points[0] = _StartPoint;
			points[points.Length - 1] = _EndPoint;
			
			for (var i = 1; i < points.Length - 1; i++) {

				var fraction = i / (float)PointsCount;

				points[i] = _StartPoint + vector * fraction + new Vector3(0, Y_AnimationCurve.Evaluate(fraction), 0) * Y_Mult;
			}
			
			return points;
		}

		public void SetStartPoint(Vector3 startPoint) {
			_StartPoint = startPoint;
			_NeedUpdate = true;
		}

		public void SetEndPoint(Vector3 endPoint) {
			_EndPoint = endPoint;
			_NeedUpdate = true;
		}

		public void SetYMult(float yMult) {
			Y_Mult = yMult;
			_NeedUpdate = true;
		}

		public void SetPointsCount(int count) {
			PointsCount = count;
			_NeedUpdate = true;
		}

		protected override IEnumerator PlayTask() {
			yield break;
		}
		
		public override void Play() {
			_NeedUpdate = true;
			this.gameObject.SetActive(true);
		}

		public override void Stop() {
			transform.SetParent(EffectsHost);
			this.gameObject.SetActive(false);
		}
	}
}