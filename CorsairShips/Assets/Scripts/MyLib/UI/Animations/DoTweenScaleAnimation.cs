using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DoTweenScaleAnimation : MonoBehaviour {

    [SerializeField]
    private Ease _Ease;
    [SerializeField]
    private Vector3 _ToScale;
    [SerializeField]
    private float _Duration;
    [SerializeField]
    private int _Loops = -1;
    [SerializeField]
    private LoopType _LoopType;
    
    [SerializeField]
    private bool _PlayOnStart;

    private Vector3 _StartScale;

    private void Start() {
        _StartScale = transform.localScale;
        if(_PlayOnStart)
            Play();
    }

    public void Play() {
        transform.DOScale(_ToScale, _Duration).SetLoops(_Loops, _LoopType).SetEase(_Ease);
    }
}
