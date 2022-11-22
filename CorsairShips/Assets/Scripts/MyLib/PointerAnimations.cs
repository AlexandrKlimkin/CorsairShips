using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerAnimations : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public PointerAnimationInfo PressAnimationInfo;

	public RectTransform OverridedRectTransform;
    
	private RectTransform _rectTransform;

	public bool Interactable = true;
	public bool ReactOnPointerEvents = true;

	private Vector3 _startScale;
    
	private void Awake()
	{
		if(OverridedRectTransform == null)
			_rectTransform = transform as RectTransform;
		else
		{
			_rectTransform = OverridedRectTransform;
		}
		_startScale = _rectTransform.localScale;
	}

	public void OnPointerDown(PointerEventData eventData) {
		if(ReactOnPointerEvents)
			AnimateDown();
	}

	public void OnPointerUp(PointerEventData eventData) {
		if(ReactOnPointerEvents)
			AnimateUp();
	}

	public void AnimateUp() {
		if(Interactable)
			_rectTransform.DOScale(_startScale, PressAnimationInfo.Time);
	}

	public void AnimateDown() {
		if (Interactable)
		{
			var scaleMult = PressAnimationInfo.Scale;
			_rectTransform.DOScale(new Vector3(_startScale.x * scaleMult.x, _startScale.y * scaleMult.y, _startScale.z * scaleMult.z),
				PressAnimationInfo.Time);
		}
	}
	
	[Serializable]
	public class PointerAnimationInfo
	{
		public Vector3 Scale = new Vector3(0.95f, 0.95f, 1f);
		public float Time = 0.1f;
	}
}