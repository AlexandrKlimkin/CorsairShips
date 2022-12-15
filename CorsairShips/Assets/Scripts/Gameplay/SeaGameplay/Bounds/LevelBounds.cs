using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PestelLib.SharedLogic.Extentions;
using UnityDI;
using UnityEditor;
using UnityEngine;
using UTPLib.Core.Utils;

namespace Game.SeaGameplay.Bounds {
    [ExecuteInEditMode]
    public class LevelBounds : MonoBehaviour {
        [SerializeField]
        private Renderer _Renderer;
        public float Radius;
        public float ViewScaleY;
        
        public Color MaxColor;
        public Color MinColor;
        public float MinColorDist;
        
        private void Awake() {
            ScaleView();
            ContainerHolder.Container.RegisterInstance(this);
        }

        private void OnDestroy() {
            ContainerHolder.Container.UnregisterInstance(this);
        }

        private void OnValidate() {
            ScaleView();
        }

        private void ScaleView() {
            _Renderer.transform.localScale = new Vector3(Radius, ViewScaleY, Radius);
        }

        public float DistanceFromCenter(Vector3 pos) {
            return Vector2.Distance(pos.ToVector2XZ(), transform.position.ToVector2XZ());
        }
        
        public bool PositionInBounds(Vector3 pos) {
            var vector = pos - transform.position;
            var vectorXZ = vector.ToVector2XZ();
            return vectorXZ.sqrMagnitude < Radius * Radius;
        }

        public Vector3 GetClosestPointInBounds(Vector3 pos, float offset) {
            if (PositionInBounds(pos))
                return new Vector3(pos.x, 0, pos.z);
            var vector = pos - transform.position;
            var normalizedVector = vector.normalized;
            return transform.position + vector - normalizedVector * offset;
        }

        public void UpdateColor(float normValue) {
            _Renderer.sharedMaterial.color = Color.Lerp(MinColor, MaxColor, normValue);
        }

        private void OnDrawGizmos() {
#if UNITY_EDITOR
            Handles.color = Color.red;
            Handles.DrawWireDisc(transform.position, Vector3.up, Radius);
#endif
        }
    }
}
