using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.SeaGameplay;
using JetBrains.Annotations;
using Tools.VisualEffects;
using UnityEngine;

public class AimController : MonoBehaviour {

    [SerializeField]
    private string _LineEffectId = "IndicatorParabolaLineEffect";
    [SerializeField]
    private List<Transform> _StartPoints;

    public float Range = 10f;
    public float Angle;
    
    private Dictionary<Transform, CurveLineEffect> _Lines = new();

    private ShipWeaponController _WeaponController;
    
    private void Awake() {
        _WeaponController = GetComponentInParent<ShipWeaponController>();
    }

    private void Start() {
        _StartPoints = _WeaponController.Weapons.Select(_ => _.FirePoint).ToList();
    }

    private void Update() {
        if(_StartPoints == null)
            return;
        foreach (var startPoint in _StartPoints) {
            SetupLine(startPoint);
        }
    }

    private void OnDestroy() {
        _Lines.ForEach(_ => _.Value?.Stop());
    }

    public void SetupStartPoints(List<Transform> points) {
        _StartPoints = points;
    }
    
    private void SetupLine(Transform startPoint) {
        if (!_Lines.ContainsKey(startPoint)) {
            var lineEffect = VisualEffect.GetEffect<CurveLineEffect>(_LineEffectId);
            _Lines.Add(startPoint, lineEffect);
        }
        var line = _Lines[startPoint];
        line.Play();
        line.SetStartPoint(startPoint.position);
        var endPoint = startPoint.position + startPoint.forward * Range;
        line.SetEndPoint(endPoint);
    }

}
