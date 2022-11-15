using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI.FloatingTexts {
	public class FloatingTextWidget : MonoBehaviour {
		[SerializeField]
		private CanvasGroup _CanvasGroup;
		[SerializeField]
		private TextMeshProUGUI _Text;

		private RectTransform _RectTransform;

		private Sequence _Sequence;

		private FloatingTextData _Data;
		
		public event Action<FloatingTextWidget> OnComplete;
		
		private void Awake() {
			_RectTransform = transform as RectTransform;
		}

		public void Play(FloatingTextData data) {
			_Data = data;
			_CanvasGroup.alpha = 1;
			_Text.text = data.Text;
			_RectTransform.localPosition = data.FromPos;
			_Text.color = _Data.TextColor;
			_Text.fontSize = _Data.TextSize;
			_RectTransform.sizeDelta = _Data.Size;
			_Sequence = DOTween.Sequence();

			Vector3 toPos = Vector3.zero;

			switch (data.MovementType) {
				case FloatingWidgetMovementType.Distance:
					toPos = _RectTransform.localPosition + new Vector3(0, data.FlyDistance, 0);
					break;
				case FloatingWidgetMovementType.FromPointToPoint:
					toPos = data.ToPos;
					break;
			}
			
			_Sequence.Append(_RectTransform.DOLocalMove(toPos, _Data.FlyTime).SetEase(data.FlyCurve));
			_Sequence.Insert(_Data.StartFadeTime, _CanvasGroup.DOFade(0, _Data.FadeTime));
			_Sequence.onComplete += () => {
				OnComplete?.Invoke(this);
			};
		}
	}
}