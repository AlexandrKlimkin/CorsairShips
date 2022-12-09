using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Widgets {
	public class BarWidget : MonoBehaviour {
		[SerializeField]
		private Image _FillImage;

		[SerializeField]
		private Image _FillLerpedImage;

		[SerializeField]
		private float _LerpTime = 1f;

		private Tweener _LerpTweener;

		public float CurrentVal => _FillImage.fillAmount;

		public void Display(float value, bool lerp = true) {
			if (Math.Abs(CurrentVal - value) < float.Epsilon)
				return;

			value = Mathf.Clamp01(value);
			_FillImage.fillAmount = value;

			_LerpTweener?.Kill();
			if (lerp)
				_LerpTweener = DOTween.To(() => _FillLerpedImage.fillAmount, (v) => _FillLerpedImage.fillAmount = v,
					value, _LerpTime);
			else
				_FillLerpedImage.fillAmount = value;
		}

		public void SetProgressBarColor(Color color) {
			_FillImage.color = color;
			_FillLerpedImage.color = Color.Lerp(color, Color.white, 0.33f);
		}
	}
}
