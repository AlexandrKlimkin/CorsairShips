using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
	public class CollapsedPanel : MonoBehaviour {
		[SerializeField]
		private Button _Button;

		[SerializeField]
		private RectTransform _Panel;

		// [SerializeField]
		// private Transform _CollapsedButtonPoint;
		//
		// [SerializeField]
		// private Transform _UncollapsedButtonnPoint;

		[SerializeField]
		private Transform _IconTransform;

		[SerializeField]
		private float _CollapsedIconRatation;

		[SerializeField]
		private Vector3 _EndScaleValue;

		[SerializeField]
		private float _Duration;

		[SerializeField]
		private bool _DisableInTheEnd;

		[SerializeField]
		private Ease _CollapseEase;

		[SerializeField]
		private Ease _UncollapsedEase;


		private Quaternion _StartRotation;
		private bool _Collapsed;
		private Vector3 _StartScaleValue;
		private Tweener _Tweener;

		private void Start() {
			_StartScaleValue = _Panel.localScale;
			_Button.onClick.AddListener(Collapse);
			_StartRotation = _IconTransform.localRotation;
		}

		private void Collapse() {
			AnimateCollapse(!_Collapsed);
		}

		private void AnimateCollapse(bool collapse) {
			_Tweener?.Kill();
			_Panel.gameObject.SetActive(true);
			_Button.gameObject.SetActive(false);

			var scale = collapse ? _EndScaleValue : _StartScaleValue;

			// var buttonPos = collapse ? _CollapsedButtonPoint : _UncollapsedButtonnPoint;

			var iconRotation = collapse ? Quaternion.Euler(0, 0, _CollapsedIconRatation) : _StartRotation;

			var ease = collapse ? _CollapseEase : _UncollapsedEase;

			_Tweener = _Panel.DOScale(scale, _Duration).SetEase(ease);
			_Tweener.OnComplete(() => {
				_Collapsed = collapse;
				_Button.gameObject.SetActive(true);
				_IconTransform.localRotation = iconRotation;
				// _Button.transform.position = buttonPos.position;
				// _Button.transform.localRotation = buttonPos.localRotation;
				if (_DisableInTheEnd && collapse) {
					_Panel.gameObject.SetActive(true);
				}
			});
		}
	}
}
